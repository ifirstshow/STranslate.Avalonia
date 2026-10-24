using Avalonia.Controls;
using STranslate.Avalonia.Views;

namespace STranslate.Avalonia.Services.Capture;

/// <summary>截图会话：冻结全部显示器 → 每屏一个遮罩窗 → 返回用户选区。</summary>
public static class ScreenshotSession
{
    public static async Task<ScreenshotResult?> CaptureAsync(TopLevel parent)
    {
        var snapshots = ScreenCaptureService.CaptureAllMonitors(parent);
        if (snapshots.Count == 0)
            return null;

        var tcs = new TaskCompletionSource<ScreenshotResult?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var peers = new List<ScreenshotWindow>();
        foreach (var snapshot in snapshots)
            peers.Add(new ScreenshotWindow(snapshot, tcs, peers));

        foreach (var window in peers)
            window.Show();

        var result = await tcs.Task;
        return result;
    }
}
