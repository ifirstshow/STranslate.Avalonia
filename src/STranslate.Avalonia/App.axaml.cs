using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using STranslate.Avalonia.Services;
using STranslate.Avalonia.Services.Triggers;
using STranslate.Avalonia.ViewModels;
using STranslate.Avalonia.Views;

namespace STranslate.Avalonia;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    /// <summary>托盘"退出"发起的真正关闭（MainWindow 的关窗取消逻辑据此放行）。</summary>
    public static bool ShuttingDown;
    private static WindowsTriggerService? _triggers;
    private static TrayIcon? _trayIcon;

    /// <summary>按选择的服务 Id 解析扩展能力；未选或不存在时回退第一个可用。</summary>
    internal static T? ResolveExtension<T>(Func<ExtensionService, T?> selector, string? preferredId) where T : class
    {
        var services = Services.GetRequiredService<IReadOnlyList<ExtensionService>>();
        return services.FirstOrDefault(s => s.Id == preferredId) is { } preferred
            ? selector(preferred) ?? services.Select(selector).FirstOrDefault(x => x is not null)
            : services.Select(selector).FirstOrDefault(x => x is not null);
    }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddSingleton<HttpService>();
        services.AddSingleton<ExtensionSettingsStore>();
        services.AddSingleton<STranslate.Sdk.IHttpService>(sp => sp.GetRequiredService<HttpService>());
        services.AddSingleton<STranslate.Sdk.II18n, HostI18n>();
        services.AddSingleton<STranslate.Sdk.INotification, HostNotification>();
        services.AddSingleton<HistoryService>();
        services.AddSingleton(sp => new ExternalCallService(sp.GetRequiredService<MainViewModel>(), () => _triggers));
        services.AddSingleton<HostAudioPlayer>();
        services.AddSingleton<WebDavBackupService>();
        services.AddSingleton<UpdaterService>();
        services.AddSingleton<ExtensionHost>();
        services.AddSingleton(sp => sp.GetRequiredService<ExtensionHost>().LoadAll());
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        // UI 线程未处理异常落盘（不吞，交由默认策略，保证闪退可追因）
        Dispatcher.UIThread.UnhandledException += (_, e) =>
            Program.LogError("UIThread", e.Exception);

        // 托盘常驻：主窗可关，应用显式退出
        desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var singleInstance = Program.SingleInstance!;
        if (!singleInstance.IsFirstInstance)
        {
            // ponytail: OnFrameworkInitializationCompleted 期间 Shutdown() 不会终止进程，第二实例直接退出
            Environment.Exit(0);
            return;
        }
        singleInstance.SecondInstanceStarted += () => Dispatcher.UIThread.Post(() => SetMainWindowVisible(true));

        var mainWindow = Services.GetRequiredService<MainWindow>();
        desktop.MainWindow = mainWindow;

        SetupTrayIcon();

        // 触发系统按降级矩阵仅 Windows 启用（M2：Alt+G/Alt+A/Alt+D，Ctrl+C+C 与鼠标划词默认关、主窗开关控制）
        if (OperatingSystem.IsWindows())
        {
            var vm = Services.GetRequiredService<MainViewModel>();
            _triggers = new WindowsTriggerService(
                vm,
                () => mainWindow.IsVisible,
                show => SetMainWindowVisible(show),
                () => ResolveExtension(s => s.AsOcr, AppSettings.Current.OcrServiceId),
                () => ResolveExtension(s => s.AsTranslate, AppSettings.Current.TranslateServiceId),
                () => ResolveExtension(s => s.AsTts, null),
                audio => Services.GetRequiredService<HostAudioPlayer>().Play(audio),
                () => mainWindow.TryGetScaling());
            vm.AttachTriggers(_triggers);
            _triggers.Start();
            vm.SyncAutoStart();
            Services.GetRequiredService<ExternalCallService>().Start("http://127.0.0.1:51080/");

            DebugTrace.Log($"ocr resolver → {Services.GetRequiredService<IReadOnlyList<ExtensionService>>().Select(s => s.AsOcr).FirstOrDefault(o => o is not null)?.GetType().FullName ?? "none"}");

            if (_triggers.Conflicts.Count > 0 && _trayIcon is not null)
                _trayIcon.ToolTipText = $"STranslate（热键冲突：{string.Join("、", _triggers.Conflicts)}）";
        }

        base.OnFrameworkInitializationCompleted();
    }

    internal static void SetMainWindowVisible(bool show, bool forceForeground = false)
    {
        var window = Services.GetRequiredService<MainWindow>();
        if (!show)
        {
            window.Hide();
            return;
        }
        window.WindowState = WindowState.Normal;
        window.Show();
        if (forceForeground && OperatingSystem.IsWindows())
        {
            var hwnd = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            if (hwnd != IntPtr.Zero)
                ForceForeground(hwnd);
        }
        window.Activate();
    }

    internal static void FocusInput()
    {
        var window = Services.GetRequiredService<MainWindow>();
        if (window.IsVisible)
            window.FocusInput();
    }

    private static void ForceForeground(IntPtr hwnd)
    {
        var foreground = Win32Native.GetForegroundWindow();
        var foregroundThread = Win32Native.GetWindowThreadProcessId(foreground, out _);
        var currentThread = Win32Native.GetCurrentThreadId();
        if (foregroundThread == currentThread)
        {
            _ = Win32Native.SetForegroundWindow(hwnd);
            return;
        }
        _ = Win32Native.AttachThreadInput(foregroundThread, currentThread, true);
        _ = Win32Native.SetForegroundWindow(hwnd);
        _ = Win32Native.AttachThreadInput(foregroundThread, currentThread, false);
    }

    private void SetupTrayIcon()
    {
        var show = new NativeMenuItem("显示主窗口");
        show.Click += (_, _) => SetMainWindowVisible(true);

        var exit = new NativeMenuItem("退出");
        exit.Click += (_, _) =>
        {
            // 即时退出：隐藏窗口后直接 Shutdown。
            // 1) 不在此处移除托盘图标——菜单回调内修改图标集合会抛"集合已修改"中断退出；进程退出时图标自然消失
            // 2) 不做清理等待——进程终止时 OS 自动回收热键/钩子/后台线程
            ShuttingDown = true;
            try
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    // 快照：托盘菜单弹层窗口（TrayPopupRoot）此时正关闭并移出集合，直接枚举会炸
                    foreach (var w in desktop.Windows.ToList())
                        w.Hide();
                    // Shutdown 延后一拍，避开托盘菜单回调上下文
                    Dispatcher.UIThread.Post(() =>
                        (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown());
                }
            }
            catch (Exception e)
            {
                Program.LogError("Exit", e);
            }
        };

        var menu = new NativeMenu();
        menu.Add(show);
        menu.Add(exit);

        var icon = new WindowIcon(AssetLoader.Open(new Uri("avares://STranslate.Avalonia/Assets/app.ico")));
        _trayIcon = new TrayIcon { Icon = icon, ToolTipText = "STranslate", Menu = menu };
        TrayIcon.SetIcons(this, new TrayIcons { _trayIcon });
    }
}
