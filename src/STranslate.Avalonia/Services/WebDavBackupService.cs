using System.IO.Compression;
using System.Net;
using STranslate.Core;
using WebDav;

namespace STranslate.Avalonia.Services;

/// <summary>
/// WebDav 云备份：打包配置三件套上传/下载（对齐 WPF 备份能力的服务端部分）。
/// </summary>
public sealed class WebDavBackupService
{
    public static readonly string[] BackupFiles = ["settings.json", "extension-settings.json", "history.db"];

    private static string DataDir => Path.Combine(DataLocation.DataDirectory(), "Avalonia");

    private static WebDavClient CreateClient(BackupSettings backup) =>
        new(new WebDavClientParams
        {
            BaseAddress = new Uri(backup.Address.TrimEnd('/') + "/"),
            Credentials = new NetworkCredential(backup.Username, backup.Password)
        });

    /// <summary>打包并上传到 WebDav（Address/备份名.zip）。返回错误或 null=成功。</summary>
    public async Task<string?> UploadAsync(BackupSettings backup)
    {
        var zipPath = Path.Combine(Path.GetTempPath(), $"stranslate-backup-{DateTime.Now:yyyyMMdd-HHmmss}.zip");
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            foreach (var name in BackupFiles)
            {
                var path = Path.Combine(DataDir, name);
                if (!File.Exists(path))
                    continue;
                zip.CreateEntryFromFile(path, name);
            }
        }

        try
        {
            using var client = CreateClient(backup);
            var remote = $"{backup.Address.TrimEnd('/')}/{Constant.AppName}.zip";
            await using var fs = File.OpenRead(zipPath);
            var result = await client.PutFile(remote, fs);
            return result.IsSuccessful ? null : $"上传失败：{result.Description} ({result.StatusCode})";
        }
        finally
        {
            try { File.Delete(zipPath); } catch { }
        }
    }

    /// <summary>从 WebDav 下载并还原（覆盖本地配置，重启后完全生效）。返回错误或 null=成功。</summary>
    public async Task<string?> DownloadAndRestoreAsync(BackupSettings backup)
    {
        var zipPath = Path.Combine(Path.GetTempPath(), $"stranslate-restore-{DateTime.Now:yyyyMMdd-HHmmss}.zip");
        try
        {
            using var client = CreateClient(backup);
            var remote = $"{backup.Address.TrimEnd('/')}/{Constant.AppName}.zip";
            using var response = await client.GetRawFile(remote);
            if (!response.IsSuccessful)
                return $"下载失败：{response.Description} ({response.StatusCode})";
            await using var remoteStream = response.Stream;
            using var memory = new MemoryStream();
            await remoteStream.CopyToAsync(memory);

            memory.Position = 0;
            using var zip = new ZipArchive(memory, ZipArchiveMode.Read);
            var restored = 0;
            foreach (var entry in zip.Entries)
            {
                if (!BackupFiles.Contains(entry.Name))
                    continue;
                var target = Path.Combine(DataDir, entry.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                entry.ExtractToFile(target, overwrite: true);
                restored++;
            }
            return restored > 0 ? null : "备份中没有可识别的配置文件";
        }
        finally
        {
            try { File.Delete(zipPath); } catch { }
        }
    }
}
