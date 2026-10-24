using System.IO.Compression;
using STranslate.Core;

namespace STranslate.Avalonia.Services;

/// <summary>一个已安装的社区插件包。</summary>
public sealed record InstalledPluginPackage(string DirectoryName, string InstallPath);

/// <summary>
/// 社区 .spkg 包管理（ADR-0004）：zip 解压安装到 Plugins/，卸载删除目录。
/// spkg = zip，内含插件程序集（dll）+ 依赖；worker 按目录扫描加载。
/// </summary>
public sealed class PluginPackageManager
{
    public static string PluginsRoot => Path.Combine(DataLocation.DataDirectory(), "Avalonia", "Plugins");

    public IReadOnlyList<InstalledPluginPackage> ListInstalled()
    {
        if (!Directory.Exists(PluginsRoot))
            return [];
        return Directory.EnumerateDirectories(PluginsRoot)
            .Select(d => new InstalledPluginPackage(Path.GetFileName(d), d))
            .ToList();
    }

    /// <summary>安装 .spkg（zip）。返回错误或 null=成功。</summary>
    public string? Install(string packagePath)
    {
        try
        {
            using var zip = ZipFile.OpenRead(packagePath);
            if (!zip.Entries.Any(e => e.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
                return "包内没有插件程序集";

            var name = Path.GetFileNameWithoutExtension(packagePath);
            var target = Path.Combine(PluginsRoot, name);
            if (Directory.Exists(target))
                Directory.Delete(target, recursive: true);
            Directory.CreateDirectory(target);
            zip.ExtractToDirectory(target);
            return null;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    /// <summary>卸载。返回错误或 null=成功。</summary>
    public string? Uninstall(string directoryName)
    {
        var target = Path.Combine(PluginsRoot, directoryName);
        if (!Directory.Exists(target))
            return null;
        // 进程退出后句柄释放有延迟，重试删除
        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                Directory.Delete(target, recursive: true);
                return null;
            }
            catch (IOException) when (attempt < 4)
            {
                Thread.Sleep(300);
            }
            catch (UnauthorizedAccessException) when (attempt < 4)
            {
                Thread.Sleep(300);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        return "目录被占用，请稍后重试";
    }
}
