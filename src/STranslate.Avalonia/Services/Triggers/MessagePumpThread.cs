using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Avalonia.Threading;

namespace STranslate.Avalonia.Services.Triggers;

/// <summary>
/// 专用消息泵线程：隐藏 message-only 窗口承载全局热键（WM_HOTKEY），
/// 低级钩子也在该线程安装（回调经 GetMessage 派发，不阻塞 UI）。
/// </summary>
public sealed class MessagePumpThread : IDisposable
{
    private const int HotkeyBaseId = 0xB000;

    private Thread? _thread;
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly ManualResetEventSlim _exitGate = new(false);

    private IntPtr _hwnd;
    private Thread? _pumpThread;
    private readonly Win32Native.WNDCLASSEX _classGuard;
    private readonly Win32Native.WndProcDelegate _wndProcDelegate; // 防 GC 回收
    private readonly Action? _onPumpThreadStarted;
    private readonly Dictionary<int, Action> _hotkeys = [];
    private readonly object _hotkeyLock = new();
    private int _nextHotkeyId = HotkeyBaseId;

    public MessagePumpThread(Action? onPumpThreadStarted = null)
    {
        _onPumpThreadStarted = onPumpThreadStarted;
        _wndProcDelegate = WndProc;
        _classGuard = CreateWindowClass();
    }

    public void Start()
    {
        _thread = new Thread(Run) { IsBackground = true, Name = "STranslate.TriggerPump" };
        _thread.SetApartmentState(ApartmentState.STA);
        _pumpThread = _thread;
        _thread.Start();
        if (!_ready.Task.Wait(TimeSpan.FromSeconds(5)))
            throw new InvalidOperationException("触发消息泵线程启动超时");
    }

    private Win32Native.WNDCLASSEX CreateWindowClass()
    {
        var wc = new Win32Native.WNDCLASSEX
        {
            cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<Win32Native.WNDCLASSEX>(),
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            hInstance = Win32Native.GetModuleHandleW(null),
            // 进程生命周期常驻的类名指针（AOT 友好：结构体内不嵌字符串）
            lpszClassName = Marshal.StringToHGlobalUni("STranslate_TriggerWnd_" + Guid.NewGuid().ToString("N")),
            style = 0
        };
        _ = Win32Native.RegisterClassEx(in wc);
        return wc;
    }

    private void Run()
    {
        _hwnd = Win32Native.CreateWindowExW(
            Win32Native.WS_EX_NOACTIVATE | Win32Native.WS_EX_TOOLWINDOW,
            _classGuard!.lpszClassName, "", Win32Native.WS_POPUP,
            0, 0, 0, 0, Win32Native.HWND_MESSAGE, IntPtr.Zero, _classGuard.hInstance, IntPtr.Zero);

        _onPumpThreadStarted?.Invoke();
        _ready.SetResult();

        while (Win32Native.GetMessageW(out var msg, IntPtr.Zero, 0, 0) > 0)
        {
            _ = Win32Native.TranslateMessage(in msg);
            _ = Win32Native.DispatchMessageW(in msg);
        }
        _exitGate.Set();
    }

    /// <summary>注册全局热键；回调投递到 Avalonia UI 线程。仅在泵线程内调用（Start 时由 onStarted 驱动）。</summary>
    public bool RegisterHotkey(uint modifiers, uint vk, Action onTriggered)
    {
        var id = _nextHotkeyId++;
        var ok = Win32Native.RegisterHotKey(_hwnd, id, modifiers | Win32Native.MOD_NOREPEAT, vk);
        var error = Marshal.GetLastWin32Error();
#if DEBUG
        DebugTrace.Log($"RegisterHotkey vk={vk:X2} hwnd={_hwnd} ok={ok} err={error}");
#endif
        if (!ok)
            return false;
        lock (_hotkeyLock)
            _hotkeys[id] = onTriggered;
        return true;
    }

    /// <summary>注销全部热键（可从任意线程调用，注册需回泵线程场景由调用方保证串行）。</summary>
    public void UnregisterHotkeys()
    {
        lock (_hotkeyLock)
        {
            foreach (var id in _hotkeys.Keys)
                _ = Win32Native.UnregisterHotKey(_hwnd, id);
            _hotkeys.Clear();
        }
    }

    /// <summary>把动作投递到泵线程执行（热键注册必须在该线程，见 WindowsTriggerService.ApplyHotkeys）。</summary>
    public void Post(Action action)
    {
        _pumpActions.Enqueue(action);
        _ = Win32Native.PostMessageW(_hwnd, Win32Native.WM_APP, IntPtr.Zero, IntPtr.Zero);
    }

    private readonly ConcurrentQueue<Action> _pumpActions = new();

    private IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        // 原生回调上下文，异常会 CLR fail-fast（无托管日志闪退），必须整体兜住
        try
        {
            if (msg == Win32Native.WM_APP)
            {
                while (_pumpActions.TryDequeue(out var action))
                    action();
                return IntPtr.Zero;
            }

            if (msg == Win32Native.WM_HOTKEY)
            {
                Action? callback;
                lock (_hotkeyLock)
                    _hotkeys.TryGetValue(wParam.ToInt32(), out callback);
                if (callback is not null)
                {
#if DEBUG
                    DebugTrace.Log($"WM_HOTKEY id={wParam}");
#endif
                    Dispatcher.UIThread.Post(callback);
                    return IntPtr.Zero;
                }
            }
        }
        catch (Exception e)
        {
            Program.LogError("PumpWndProc", e);
        }
        return Win32Native.DefWindowProcW(hwnd, msg, wParam, lParam);
    }

    public void Dispose()
    {
        foreach (var id in _hotkeys.Keys)
            _ = Win32Native.UnregisterHotKey(_hwnd, id);
        _hotkeys.Clear();
        if (_hwnd != IntPtr.Zero)
        {
            // PostQuitMessage 只作用于调用线程，必须向泵线程投递 WM_QUIT
            Win32Native.PostThreadMessageW((uint)_pumpThread!.ManagedThreadId, Win32Native.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
        }
        // 正常 <100ms；1s 兜底避免拖慢退出
        _exitGate.Wait(TimeSpan.FromSeconds(1));
        GC.KeepAlive(_classGuard);
        GC.KeepAlive(_wndProcDelegate);
    }
}
