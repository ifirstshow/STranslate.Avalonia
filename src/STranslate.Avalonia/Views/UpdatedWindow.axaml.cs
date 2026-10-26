using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;

namespace STranslate.Avalonia.Views;

/// <summary>更新完成提示：新版本号 + Release 入口（对齐原版 UpdateChangelogDialog 的简化版）。</summary>
public partial class UpdatedWindow : Window
{
    public UpdatedWindow(string version)
    {
        InitializeComponent();
        VersionText.Text = $"已更新到 v{version}";
    }

    private void OnOpenReleaseClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(
                "https://github.com/ifirstshow/STranslate.Avalonia/releases") { UseShellExecute = true });
        }
        catch { /* 浏览器打开失败不影响主流程 */ }
        Close();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
