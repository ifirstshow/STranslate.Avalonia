using Avalonia;
using Velopack;
using STranslate.Avalonia;
using STranslate.Avalonia.Services;

namespace STranslate.Avalonia;

internal static class Program
{
    internal static SingleInstanceService? SingleInstance { get; private set; }
    private static readonly string ErrorLogPath =
        Path.Combine(Path.GetTempPath(), "stranslate-avalonia-errors.log");

    internal static void LogError(string source, Exception e) =>
        File.AppendAllText(ErrorLogPath, $"{DateTime.Now:HH:mm:ss.fff} [{source}] {e}\n\n");

    [STAThread]
    public static void Main(string[] args)
    {
        // Velopack 钩子必须最先执行（安装/更新/卸载事件入口）
        Velopack.VelopackApp.Build().Run();
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            LogError("AppDomain", e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject?.ToString()));
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            LogError("UnobservedTask", e.Exception);
            e.SetObserved();
        };

        // 单实例判定放在 Avalonia 启动之前：第二实例不进 UI，直接退出
        SingleInstance = new SingleInstanceService();
        if (!SingleInstance.IsFirstInstance)
            return;

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
