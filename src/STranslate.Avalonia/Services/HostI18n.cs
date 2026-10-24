using System.Globalization;
using STranslate.Sdk;

namespace STranslate.Avalonia.Services;

/// <summary>
/// 宿主 i18n。当前 Avalonia UI 为中文硬编码，此处仅覆盖扩展消息所需 key；
/// UI 全面本地化时再迁移 WPF Languages 全量字典。
/// ponytail: 内置小表 + 系统语言判定；key 未收录时回退原文（英文 key 可读）。
/// </summary>
public sealed class HostI18n : II18n
{
    private static readonly Dictionary<string, string> Zh = new()
    {
        ["UnsupportedSourceLang"] = "不支持的源语言",
        ["UnsupportedTargetLang"] = "不支持的目标语言",
        ["AlreadyLatestVersion"] = "已是最新版本",
        ["TtsServiceNotFound"] = "未找到可用的 TTS 服务",
        ["TtsFailed"] = "语音播放失败",
    };

    private static readonly Dictionary<string, string> En = new()
    {
        ["UnsupportedSourceLang"] = "Unsupported source language",
        ["UnsupportedTargetLang"] = "Unsupported target language",
        ["AlreadyLatestVersion"] = "Already the latest version",
        ["TtsServiceNotFound"] = "No TTS service available",
        ["TtsFailed"] = "Audio playback failed",
    };

    private static bool IsChinese =>
        CultureInfo.CurrentUICulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase);

    public string GetTranslation(string key)
    {
        var table = IsChinese ? Zh : En;
        return table.TryGetValue(key, out var text) ? text : key;
    }
}

/// <summary>宿主系统通知（Windows Toast）。</summary>
public sealed class HostNotification : INotification
{
    public void Show(string title, string message)
    {
        // 通知失败绝不影响主流程（开发模式无 AUMID 时 Toolkit 会抛）
        try
        {
            new Microsoft.Toolkit.Uwp.Notifications.ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();
        }
        catch (Exception e)
        {
            Program.LogError("Toast", e);
        }
    }
}
