using Microsoft.Win32;

namespace STranslate.Avalonia.Services;

/// <summary>
/// 开机自启（HKCU Run 注册表键，仅 Windows）。
/// </summary>
public static class AutoStartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "STranslate.Avalonia";

    private static string ExePath =>
        Environment.ProcessPath ?? throw new InvalidOperationException("无法获取主程序路径");

    public static bool IsEnabled()
    {
        if (!OperatingSystem.IsWindows())
            return false;
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
        var value = key?.GetValue(ValueName) as string;
        return value == ExePath; // 路径变了视为失效
    }

    public static void Set(bool enabled)
    {
        if (!OperatingSystem.IsWindows())
            return;
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(RunKeyPath);
        if (enabled)
            key.SetValue(ValueName, $"\"{ExePath}\"");
        else
            key.DeleteValue(ValueName, throwOnMissingValue: false);
    }
}
