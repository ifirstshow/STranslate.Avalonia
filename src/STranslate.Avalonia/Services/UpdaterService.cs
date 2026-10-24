using STranslate.Core;
using Velopack;
using Velopack.Sources;

namespace STranslate.Avalonia.Services;

/// <summary>
/// Velopack 自更新。仅 vpk 打包安装的构建可用；开发模式（dotnet run）优雅降级。
/// </summary>
public sealed class UpdaterService
{
    private const string RepoUrl = "https://github.com/ifirstshow/STranslate.Avalonia";

    /// <summary>检查并应用更新（有更新则下载后自动重启）。返回给 UI 的提示文本。</summary>
    public async Task<string> CheckAndApplyAsync(IProgress<string>? progress = null)
    {
        try
        {
            // Velopack 全链路（构造/检查/下载）在未安装模式下都会抛 VelopackException，统一在此降级
            var manager = new UpdateManager(new GithubSource(RepoUrl, null, false));
            var update = await manager.CheckForUpdatesAsync();
            if (update is null)
                return $"已是最新版本（{Constant.Version}）";

            progress?.Report($"正在下载 {update.TargetFullRelease.Version}…");
            await manager.DownloadUpdatesAsync(update,
                p => progress?.Report($"正在下载 {update.TargetFullRelease.Version}… {p}%"));

            progress?.Report("下载完成，即将重启应用…");
            manager.ApplyUpdatesAndRestart(update);
            return "即将重启"; // 正常不会到达
        }
        catch (Velopack.Exceptions.NotInstalledException)
        {
            // vpk 安装版才会真实检查更新；开发模式/绿色解压版走此降级
            return "当前为非安装版运行，自动更新仅支持安装版";
        }
    }
}
