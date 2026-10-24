using System.Diagnostics;

namespace STranslate.Avalonia.Services;

/// <summary>Debug 构建的诊断落盘（%TEMP%\m2trace.log），发布版编译消除。</summary>
public static class DebugTrace
{
    private const string Path = @"C:\Users\afly\AppData\Local\Temp\m2trace.log";

    [Conditional("DEBUG")]
    public static void Log(string message) =>
        File.AppendAllText(Path, $"{DateTime.Now:HH:mm:ss.fff} {message}\n");
}
