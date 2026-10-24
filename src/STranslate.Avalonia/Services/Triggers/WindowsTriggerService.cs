using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using STranslate.Avalonia.Services;
using STranslate.Sdk;
using STranslate.Avalonia.ViewModels;

namespace STranslate.Avalonia.Services.Triggers;

/// <summary>
/// Windows 触发系统（M2，对齐 WPF 行为的核心路径）：
/// 全局热键 Alt+G（开关主窗）/ Alt+A（输入翻译）/ Alt+D（划词翻译），
/// Ctrl+C+C 剪贴板划词（默认关），鼠标划词（默认关）。
/// 取词 = 模拟 Ctrl+C + 剪贴板序列号轮询 + Win32 剪贴板读取（500ms 超时，失败回退输入翻译）。
/// 快捷键配置/增量翻译/悬浮图标/全屏忽略 → M4。
/// </summary>
public sealed class WindowsTriggerService : IDisposable
{
    private const uint VkG = 0x47, VkA = 0x41, VkD = 0x44, VkC = 0x43, VkS = 0x53, VkF = 0x46, VkX = 0x58;

    private readonly MessagePumpThread _pump;
    private readonly MainViewModel _viewModel;
    private readonly Func<bool> _isMainWindowVisible;
    private readonly Action<bool> _showMainWindow;
    private readonly Func<IOcrPlugin?> _resolveOcr;
    private readonly Func<ITranslatePlugin?> _resolveTranslate;
    private readonly Func<ITtsPlugin?> _resolveTts;
    private readonly Action<STranslate.Sdk.AudioData> _playAudio;
    private readonly Func<double> _getScaling;

    private IntPtr _keyboardHook;
    private IntPtr _mouseHook;
    private long _lastCtrlCTick;
    private uint _incVk;                // 增量翻译触发键 vk；0 = 关闭
    private bool _incActive;            // 会话进行中（按住触发键）
    private bool _incDirty;             // 会话内是否有新捕获
    private bool _dragging;
    private bool _dragMoved;
    private int _dragStartX, _dragStartY;
    private long _lastUpTick;
    private int _lastUpX = -100, _lastUpY = -100;

    // 钩子委托必须字段保活：临时方法组经封送后仅在调用期存活，
    // SetWindowsHookExW 返回后指针悬空，GC 回收后回调即原生崩溃（表现为系统鼠标卡顿+闪退）
    private readonly Win32Native.HookProc _keyboardHookProc;
    private readonly Win32Native.HookProc _mouseHookProc;

    public bool CtrlCcEnabled { get; private set; }
    public bool MouseSelectionEnabled { get; private set; }
    public bool MouseSelectionIconEnabled { get; private set; }

    /// <summary>注册失败的热键（Start() 返回后可读）。</summary>
    public IReadOnlyList<string> Conflicts { get; private set; } = [];
    private readonly List<string> _conflicts = [];

    public WindowsTriggerService(
        MainViewModel viewModel,
        Func<bool> isMainWindowVisible,
        Action<bool> showMainWindow,
        Func<IOcrPlugin?> resolveOcr,
        Func<ITranslatePlugin?> resolveTranslate,
        Func<ITtsPlugin?> resolveTts,
        Action<STranslate.Sdk.AudioData> playAudio,
        Func<double> getScaling)
    {
        _viewModel = viewModel;
        _isMainWindowVisible = isMainWindowVisible;
        _showMainWindow = showMainWindow;
        _resolveOcr = resolveOcr;
        _resolveTranslate = resolveTranslate;
        _resolveTts = resolveTts;
        _playAudio = playAudio;
        _getScaling = getScaling;
        _keyboardHookProc = KeyboardHookProc;
        _mouseHookProc = MouseHookProc;
        _pump = new MessagePumpThread(OnPumpThreadStarted);
    }

    public void Start()
    {
        _pump.Start();
        Conflicts = _conflicts.ToArray();
    }

    // 与 WPF 版默认一致的热键（冲突重试 → M4）
    private void OnPumpThreadStarted()
    {
        InstallHooks();
        ApplyHotkeys(AppSettings.Current.Hotkeys);
    }

    /// <summary>设置页保存后的冲突回调（UI 线程）。</summary>
    public event Action<IReadOnlyList<string>>? HotkeysApplied;

    /// <summary>按配置（重）注册全部热键。任意线程可调用，实际注册投递到泵线程。</summary>
    private int _hotkeyGeneration;

    public void ApplyHotkeys(HotkeyEntry hotkeys)
    {
        _hotkeyGeneration++;
        var generation = _hotkeyGeneration;
        _pump.Post(() =>
        {
            _pump.UnregisterHotkeys();
            _conflicts.Clear();

            TryRegister(hotkeys.OpenWindow, Win32Native.MOD_ALT, VkG, ToggleWindow);
            TryRegister(hotkeys.InputTranslate, Win32Native.MOD_ALT, VkA, () => Dispatcher.UIThread.Post(() => _viewModel.InputClear()));
            TryRegister(hotkeys.CrosswordTranslate, Win32Native.MOD_ALT, VkD, () => BeginCrosswordCapture(forceForeground: false));
            TryRegister(hotkeys.ScreenshotTranslate, Win32Native.MOD_ALT, VkS, BeginScreenshot);
            TryRegister(hotkeys.SilentOcr, Win32Native.MOD_ALT | Win32Native.MOD_SHIFT, VkF, BeginSilentOcr);
            TryRegister(hotkeys.SilentTts, Win32Native.MOD_ALT | Win32Native.MOD_SHIFT, VkG, BeginSilentTts);
            TryRegister(hotkeys.OcrWindow, Win32Native.MOD_ALT | Win32Native.MOD_SHIFT, VkS, BeginOcrWindow);
            TryRegister(hotkeys.ImageTranslate, Win32Native.MOD_ALT | Win32Native.MOD_SHIFT, VkX, BeginImageTranslate);

            // 增量翻译键：裸键（无修饰键），不注册全局热键，由键盘钩子跟踪按下/松开
            _incVk = HotkeyCodec.TryParseBareKey(AppSettings.Current.IncrementalTranslateKey, out var incVk)
                ? incVk : 0;
            Conflicts = _conflicts.ToArray();
            HotkeysApplied?.Invoke(Conflicts);

            // WPF 同款：启动期被占用的热键按 0.5/1/2/4s 延迟重试（占用方常为正在退出的其他应用）
            if (_conflicts.Count > 0)
                _ = RetryConflictsAsync(hotkeys, generation);
        });
    }

    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4)
    ];

    private async Task RetryConflictsAsync(HotkeyEntry hotkeys, int generation)
    {
        foreach (var delay in RetryDelays)
        {
            await Task.Delay(delay);
            if (generation != _hotkeyGeneration || _conflicts.Count == 0)
                return;
            _pump.Post(() =>
            {
                if (generation != _hotkeyGeneration || _conflicts.Count == 0)
                    return;
                // 重试前必须先注销全部：已成功的键用新 id 重注册必然失败（自我冲突）
                _pump.UnregisterHotkeys();
                _conflicts.Clear();
                TryRegister(hotkeys.OpenWindow, Win32Native.MOD_ALT, VkG, ToggleWindow);
                TryRegister(hotkeys.InputTranslate, Win32Native.MOD_ALT, VkA, () => Dispatcher.UIThread.Post(() => _viewModel.InputClear()));
                TryRegister(hotkeys.CrosswordTranslate, Win32Native.MOD_ALT, VkD, () => BeginCrosswordCapture(forceForeground: false));
                TryRegister(hotkeys.ScreenshotTranslate, Win32Native.MOD_ALT, VkS, BeginScreenshot);
                TryRegister(hotkeys.SilentOcr, Win32Native.MOD_ALT | Win32Native.MOD_SHIFT, VkF, BeginSilentOcr);
                TryRegister(hotkeys.SilentTts, Win32Native.MOD_ALT | Win32Native.MOD_SHIFT, VkG, BeginSilentTts);
            TryRegister(hotkeys.OcrWindow, Win32Native.MOD_ALT | Win32Native.MOD_SHIFT, VkS, BeginOcrWindow);
                TryRegister(hotkeys.ImageTranslate, Win32Native.MOD_ALT | Win32Native.MOD_SHIFT, VkX, BeginImageTranslate);
                Conflicts = _conflicts.ToArray();
                HotkeysApplied?.Invoke(Conflicts);
            });
        }
    }

    private void TryRegister(string hotkeyText, uint fallbackMods, uint vk, Action action)
    {
        // 配置里的热键串解析成功则用配置，否则退回默认组合
        var mods = fallbackMods;
        var vkActual = vk;
        if (HotkeyCodec.TryParse(hotkeyText, out var parsedMods, out var parsedVk))
        {
            mods = parsedMods;
            vkActual = parsedVk;
        }
        if (!_pump.RegisterHotkey(mods, vkActual, action))
            _conflicts.Add(hotkeyText);
    }

    // ── 开关（M2 内存态，M4 设置页持久化）──

    public void SetCtrlCcEnabled(bool enabled)
    {
        CtrlCcEnabled = enabled;
        _lastCtrlCTick = 0;
    }

    public void SetMouseSelectionEnabled(bool enabled)
    {
        MouseSelectionEnabled = enabled;
        _dragging = false;
    }

    public void SetMouseSelectionIconEnabled(bool enabled)
    {
        MouseSelectionIconEnabled = enabled;
        _dragging = false;
        HideIcon?.Invoke();
    }

    /// <summary>隐藏悬浮图标（由宿主窗口实现）。</summary>
    public event Action? HideIcon;

    // ── 钩子安装（泵线程内执行）──

    private void InstallHooks()
    {
        _keyboardHook = Win32Native.SetWindowsHookExW(
            Win32Native.WH_KEYBOARD_LL, _keyboardHookProc, Win32Native.GetModuleHandleW(null), 0);
        _mouseHook = Win32Native.SetWindowsHookExW(
            Win32Native.WH_MOUSE_LL, _mouseHookProc, Win32Native.GetModuleHandleW(null), 0);
    }

    private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        // 钩子回调处于原生调用上下文，任何异常都会 CLR fail-fast（无托管日志的闪退），必须整体兜住
        try
        {
            return OnKeyboard(nCode, wParam, lParam);
        }
        catch (Exception e)
        {
            Program.LogError("KeyboardHook", e);
            return Win32Native.CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
        }
    }

    private IntPtr OnKeyboard(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var hook0 = Marshal.PtrToStructure<Win32Native.KBDLLHOOKSTRUCT>(lParam);
            var down0 = wParam.ToInt64() is 0x0100 or 0x0104;
            if (_incVk != 0 && hook0.vkCode == _incVk)
            {
                DebugTrace.Log($"inc key down={down0} active={_incActive}");
                if (down0 && !_incActive)
                {
                    _incActive = true;
                    _incDirty = false;
                    Dispatcher.UIThread.Post(() => _viewModel.BeginIncrementalSession());
                }
                else if (!down0 && _incActive)
                {
                    _incActive = false;
                    Dispatcher.UIThread.Post(() => _viewModel.EndIncrementalSession());
                }
            }
        }

        if (nCode >= 0 && CtrlCcEnabled)
        {
            var hook = Marshal.PtrToStructure<Win32Native.KBDLLHOOKSTRUCT>(lParam);
            var down = wParam.ToInt64() is 0x0100 or 0x0104; // WM_KEYDOWN / WM_SYSKEYDOWN

            // 低级钩子报左/右专属键值（VK_LCONTROL=0xA2 / VK_RCONTROL=0xA3），通用 VK_CONTROL 不会出现
            switch (hook.vkCode)
            {
                case Win32Native.VK_CONTROL or 0xA2 or 0xA3:
                    _ctrlDown = down;
                    break;
                case VkC when down:
                {
                    var now = Environment.TickCount64;
                    if (_ctrlDown && now - _lastCtrlCTick is < 500 and >= 0)
                    {
                        _lastCtrlCTick = 0;
                        // WPF 语义：Ctrl+CC 用强制置前上下文
                        BeginCrosswordCapture(forceForeground: true);
                    }
                    else
                        _lastCtrlCTick = _ctrlDown ? now : 0;
                    break;
                }
                default:
                    // 其他按键（仅按下时）打断双击窗口；松键事件不重置
                    if (down)
                        _lastCtrlCTick = 0;
                    break;
            }
        }
        return Win32Native.CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
    }

    private bool _ctrlDown;

    private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        // 同键盘钩子：原生回调上下文禁止异常逃逸
        try
        {
            return OnMouse(nCode, wParam, lParam);
        }
        catch (Exception e)
        {
            Program.LogError("MouseHook", e);
            return Win32Native.CallNextHookEx(_mouseHook, nCode, wParam, lParam);
        }
    }

    private IntPtr OnMouse(int nCode, IntPtr wParam, IntPtr lParam)
    {
        // 增量会话/悬浮图标模式均独立于鼠标划词开关（对齐 WPF：任一消费者开启即共享钩子）
        if (nCode >= 0 && (MouseSelectionEnabled || MouseSelectionIconEnabled || _incActive))
        {
            var hook = Marshal.PtrToStructure<Win32Native.MSLLHOOKSTRUCT>(lParam);
            switch (wParam.ToInt64())
            {
                case Win32Native.WM_LBUTTONDOWN:
                    _dragging = true;
                    _dragMoved = false;
                    _dragStartX = hook.ptX;
                    _dragStartY = hook.ptY;
                    break;
                case Win32Native.WM_MOUSEMOVE when _dragging:
                    if (Math.Abs(hook.ptX - _dragStartX) + Math.Abs(hook.ptY - _dragStartY) > 8)
                        _dragMoved = true;
                    break;
                case Win32Native.WM_LBUTTONUP when _dragging:
                    _dragging = false;
                    // LL 钩子收不到合成的 WM_LBUTTONDBLCLK——手动检测双击（时间+距离）
                    var isDoubleClick = Math.Abs(hook.ptX - _lastUpX) < 8 &&
                                        Math.Abs(hook.ptY - _lastUpY) < 8 &&
                                        _lastUpTick != 0 &&
                                        Environment.TickCount64 - _lastUpTick <= Win32Native.GetDoubleClickTime();
                    _lastUpTick = Environment.TickCount64;
                    _lastUpX = hook.ptX;
                    _lastUpY = hook.ptY;

                    if (_dragMoved || isDoubleClick)
                    {
                        if (_incActive)
                            BeginIncrementalCapture();
                        else if (MouseSelectionEnabled)
                            BeginCrosswordCapture(forceForeground: false);
                        else if (MouseSelectionIconEnabled)
                            ShowIconNear(hook.ptX, hook.ptY);
                    }
                    break;
            }
        }
        return Win32Native.CallNextHookEx(_mouseHook, nCode, wParam, lParam);
    }

    // ── 触发动作 ──

    private void ToggleWindow()
    {
        if (_isMainWindowVisible())
            _showMainWindow(false); // 隐藏
        else
            _showMainWindow(true);
    }

    /// <summary>静默 OCR（Alt+Shift+F）：截图 → OCR → 写剪贴板，全程不弹窗；失败才显示主窗。</summary>
    internal void BeginSilentOcr()
    {
        if (_isMainWindowVisible())
            _showMainWindow(false); // 避免截到自身

        _ = Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                var desktop = App.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                var topLevel = desktop?.MainWindow as TopLevel;
                var mainWindow = desktop?.MainWindow;
                if (topLevel is null || mainWindow is null)
                    return;

                var result = await Services.Capture.ScreenshotSession.CaptureAsync(topLevel);
                if (result is null)
                    return;

                var ocr = _resolveOcr();
                if (ocr is null)
                {
                    _showMainWindow(true);
                    _viewModel.ErrorMessage = "没有可用的 OCR 扩展";
                    return;
                }

                var ocrResult = await ocr.RecognizeAsync(
                    new STranslate.Sdk.OcrRequest(result.PngBytes, "image/png", _viewModel.SourceLang, result.PixelSize.Width, result.PixelSize.Height));
                var text = ocrResult.Text;
                if (string.IsNullOrWhiteSpace(text))
                {
                    _showMainWindow(true);
                    _viewModel.ErrorMessage = ocrResult.IsSuccess ? "OCR 未识别到文字" : ocrResult.ErrorMessage;
                    return;
                }

                // WPF 行为：静默 OCR 结果写入剪贴板
                if (topLevel.Clipboard is { } clipboard)
                {
                    var transfer = new DataTransfer();
                    transfer.Add(DataTransferItem.CreateText(text));
                    await clipboard.SetDataAsync(transfer);
                }
            }
            catch (Exception e)
            {
                Program.LogError("SilentOcr", e);
                _showMainWindow(true);
                _viewModel.ErrorMessage = e.Message;
            }
        });
    }

    /// <summary>图片翻译（Alt+Shift+X）：截图 → OCR 坐标块 → 并行翻译 → 原位贴图覆盖。</summary>
    internal void BeginImageTranslate()
    {
        if (_isMainWindowVisible())
            _showMainWindow(false); // 避免截到自身

        _ = Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                var desktop = App.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                var topLevel = desktop?.MainWindow as TopLevel;
                if (topLevel is null)
                    return;

                var result = await Services.Capture.ScreenshotSession.CaptureAsync(topLevel);
                if (result is null)
                    return;

                var ocr = _resolveOcr();
                if (ocr is null)
                {
                    _viewModel.ErrorMessage = "没有可用的 OCR 扩展";
                    return;
                }

                await _viewModel.HandleImageTranslateAsync(result, ocr, _resolveTranslate());
            }
            catch (Exception e)
            {
                Program.LogError("ImageTranslate", e);
            }
        });
    }

    /// <summary>增量翻译：会话内每次选区追加到输入框，松开触发键后由 EndIncrementalSession 翻译。</summary>
    private void BeginIncrementalCapture()
    {
        _ = Task.Run(async () =>
        {
            var text = await CaptureSelectedTextAsync();
            DebugTrace.Log($"inc capture={(text is null ? "null" : $"len={text.Length}")}");
            if (string.IsNullOrWhiteSpace(text))
                return;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _incDirty = true;
                _viewModel.AppendIncrementalText(text);
            });
        });
    }

    /// <summary>OCR 独立窗口（Alt+Shift+S）：截图 → 窗口内识别展示。</summary>
    internal void BeginOcrWindow()
    {
        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                var topLevel = (App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as TopLevel;
                if (topLevel is null)
                    return;
                var result = await Services.Capture.ScreenshotSession.CaptureAsync(topLevel);
                if (result is null)
                    return;
                var ocr = _resolveOcr();
                if (ocr is null)
                    return;
                var (w, h) = PngSize(result.PngBytes);
                var window = new Views.OcrWindow();
                window.Show();
                await window.RunAsync(result.PngBytes, w, h, ocr, _viewModel.SourceLang);
            }
            catch (Exception e)
            {
                Program.LogError("OcrWindow", e);
            }
        });
    }

    /// <summary>PNG 尺寸从 IHDR 头解析（宽高固定偏移 16/20）。</summary>
    private static (int W, int H) PngSize(byte[] png)
    {
        if (png.Length > 24 && png[0] == 0x89 && png[1] == (byte)'P')
            return ((png[16] << 24) | (png[17] << 16) | (png[18] << 8) | png[19],
                    (png[20] << 24) | (png[21] << 16) | (png[22] << 8) | png[23]);
        return (0, 0);
    }

    /// <summary>静默 TTS（Alt+Shift+G）：取选中文本（失败取剪贴板）→ 直接朗读，不弹窗。</summary>
    internal void BeginSilentTts()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var text = await CaptureSelectedTextAsync();
                if (string.IsNullOrWhiteSpace(text))
                    text = TryReadClipboardText();
                if (string.IsNullOrWhiteSpace(text))
                    return;

                var tts = _resolveTts();
                if (tts is null)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _showMainWindow(true);
                        _viewModel.ErrorMessage = "没有可用的 TTS 扩展";
                    });
                    return;
                }

                var audio = await tts.SpeakAsync(text, CancellationToken.None);
                await Task.Run(() => _playAudio(audio));
            }
            catch (Exception e)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _showMainWindow(true);
                    _viewModel.ErrorMessage = e.Message;
                });
            }
        });
    }

    /// <summary>剪贴板监听翻译的最后处理文本（避免自触发，跨类共享）。</summary>
    internal static string? LastProcessedClipboardText;

    /// <summary>截图触发（Alt+S）：窗口创建必须在 UI 线程。截图 → OCR → 主窗翻译。</summary>
    internal void BeginScreenshot()
    {
        if (_isMainWindowVisible())
            _showMainWindow(false); // 避免截到自身

        _ = Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                var desktop = App.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                var topLevel = desktop?.MainWindow as TopLevel;
                if (topLevel is null)
                    return;

                var result = await Services.Capture.ScreenshotSession.CaptureAsync(topLevel);
                _showMainWindow(true); // 截图完成回到主窗
                if (result is null)
                    return;

                var ocr = _resolveOcr();
                if (ocr is null)
                {
                    _viewModel.ErrorMessage = "没有可用的 OCR 扩展";
                    return;
                }
                DebugTrace.Log($"ocr resolved: {ocr.GetType().FullName}");

                await _viewModel.HandleScreenshotAsync(result.PngBytes, result.PixelSize.Width, result.PixelSize.Height, ocr);
            }
            catch (Exception e)
            {
                Program.LogError("Screenshot", e);
                _showMainWindow(true);
            }
        });
    }

    private Views.MouseSelectionIconWindow? _iconWindow;
    private DateTime _iconShownAt;

    private void ShowIconNear(int physicalX, int physicalY)
    {
        // 与上次显示间隔过短（双击连续触发）时忽略
        if ((DateTime.UtcNow - _iconShownAt).TotalMilliseconds < 300)
            return;
        _iconShownAt = DateTime.UtcNow;
        Dispatcher.UIThread.Post(() =>
        {
            HideIconWindow();
            var scaling = _getScaling();
            _iconWindow = Views.MouseSelectionIconWindow.Show(
                new PixelPoint(physicalX, physicalY),
                () => BeginCrosswordCapture(forceForeground: false),
                scaling);
            _iconWindow.Closed += (_, _) => _iconWindow = null;
        });
    }

    private void HideIconWindow()
    {
        try { _iconWindow?.Close(); } catch { }
        _iconWindow = null;
    }

    /// <summary>划词类触发：后台模拟复制取词，成功翻译，失败回退输入翻译并提示。</summary>
    internal void BeginCrosswordCapture(bool forceForeground)
    {
#if DEBUG
        DebugTrace.Log("BeginCrosswordCapture");
#endif
        _ = Task.Run(async () =>
        {
            var text = await CaptureSelectedTextAsync();
#if DEBUG
            DebugTrace.Log($"capture={(text is null ? "null" : $"len={text.Length}")}");
#endif
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    _viewModel.InputClear(); // WPF 行为：取词失败回退输入翻译
                    _viewModel.ShowLightMessage("未捕获到选中文本（取词失败），请手动输入");
                }
                else
                    _viewModel.ShowAndTranslate(text, forceForeground);
            });
        });
    }

    /// <summary>模拟 Ctrl+C，等待剪贴板变化并读取文本；超时返回 null。</summary>
    private static async Task<string?> CaptureSelectedTextAsync()
    {
        // 热键触发瞬间修饰键可能仍被按住（如 Alt+D 的 Alt），等释放后再模拟 Ctrl+C，
        // 否则目标应用收到的是 Ctrl+Alt+C，不会复制
        var waitSw = Stopwatch.StartNew();
        while (waitSw.ElapsedMilliseconds < 300)
        {
            var held = ((int)Win32Native.GetAsyncKeyState(0x10) | (int)Win32Native.GetAsyncKeyState(0x11) | (int)Win32Native.GetAsyncKeyState(0x12)) & 0x8000;
            if (held == 0)
                break;
            await Task.Delay(20);
        }

        var before = Win32Native.GetClipboardSequenceNumber();
        SendCtrlC();

        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < 500)
        {
            await Task.Delay(50);
            if (Win32Native.GetClipboardSequenceNumber() == before)
                continue;
            var text = TryReadClipboardText();
            if (text is not null)
            {
#if DEBUG
                DebugTrace.Log($"clipboard changed at {sw.ElapsedMilliseconds}ms");
#endif
                return text;
            }
        }
        return null;
    }

    private static void SendCtrlC()
    {
        // 序列：Ctrl↓ → C↓ → C↑ → Ctrl↑
        var inputs = new Win32Native.INPUT[4];
        Span<uint> vks = [Win32Native.VK_CONTROL, VkC, VkC, Win32Native.VK_CONTROL];
        for (var i = 0; i < inputs.Length; i++)
        {
            inputs[i].type = Win32Native.INPUT_KEYBOARD;
            inputs[i].u.ki.wVk = (ushort)vks[i];
            inputs[i].u.ki.dwFlags = i >= 2 ? Win32Native.KEYEVENTF_KEYUP : 0;
        }
        _ = Win32Native.SendInput((uint)inputs.Length, inputs, System.Runtime.InteropServices.Marshal.SizeOf<Win32Native.INPUT>());
    }

    internal static string? TryReadClipboardText()
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            if (Win32Native.OpenClipboard(IntPtr.Zero))
            {
                try
                {
                    var handle = Win32Native.GetClipboardData(Win32Native.CF_UNICODETEXT);
                    if (handle == IntPtr.Zero)
                        return null;
                    var pointer = Win32Native.GlobalLock(handle);
                    if (pointer == IntPtr.Zero)
                        return null;
                    try
                    {
                        return Marshal.PtrToStringUni(pointer);
                    }
                    finally
                    {
                        _ = Win32Native.GlobalUnlock(pointer);
                    }
                }
                finally
                {
                    _ = Win32Native.CloseClipboard();
                }
            }
            Thread.Sleep(20); // 剪贴板可能被占用，稍后重试
        }
        return null;
    }

    public void Dispose()
    {
        if (_keyboardHook != IntPtr.Zero)
            _ = Win32Native.UnhookWindowsHookEx(_keyboardHook);
        if (_mouseHook != IntPtr.Zero)
            _ = Win32Native.UnhookWindowsHookEx(_mouseHook);
        _pump.Dispose();
    }
}
