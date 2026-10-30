using System.Text.Json;
using System.Text.Json.Serialization;
using STranslate.Core;

namespace STranslate.Avalonia.Services;

/// <summary>热键配置（键面字符串，如 "Alt+G"）。</summary>
public sealed class HotkeyEntry
{
    public string OpenWindow { get; set; } = "Alt+G";
    public string InputTranslate { get; set; } = "Alt+A";
    public string CrosswordTranslate { get; set; } = "Alt+D";
    public string ScreenshotTranslate { get; set; } = "Alt+S";
    public string OcrWindow { get; set; } = "Alt+Shift+S";
    public string SilentOcr { get; set; } = "Alt+Shift+F";
    public string SilentTts { get; set; } = "Alt+Shift+G";

    public string ImageTranslate { get; set; } = "Alt+Shift+X";
}

/// <summary>
/// 应用设置（settings.json，随改随存）。扩展设置另行存放于 extension-settings.json。
/// </summary>
public sealed class AppSettings
{
    private static readonly string FilePath =
        Path.Combine(DataLocation.DataDirectory(), "Avalonia", "settings.json");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    private static AppSettings? _current;

    /// <summary>当前实例（进程内单例，启动时 Load）。</summary>
    public static AppSettings Current => _current ??= Load();

    // ── 通用 ──
    public string SourceLang { get; set; } = "English";
    public string TargetLang { get; set; } = "ChineseSimplified";
    public bool CtrlCcEnabled { get; set; }
    public bool MouseSelectionEnabled { get; set; }

    /// <summary>划词后显示悬浮图标（点击才翻译）；与直接翻译互斥，直接翻译优先。</summary>
    public bool MouseSelectionIconEnabled { get; set; }
    public bool ClipboardMonitorEnabled { get; set; }

    // ── 主窗 ──
    /// <summary>增量翻译触发键（按住划词累积，松开翻译）；空 = 关闭。</summary>
    public string IncrementalTranslateKey { get; set; } = string.Empty;

    public bool IsTopmost { get; set; }
    public bool AutoTranslate { get; set; }
    public string Theme { get; set; } = "Default";

    /// <summary>图片翻译词块合并模式：smart=智能行合并；strict=逐块直翻。</summary>
    public string ImageTranslateMergeMode { get; set; } = "smart";

    /// <summary>主窗位置与宽度记忆（Height 由内容自适应）。</summary>
    public int MainWindowLeft { get; set; } = -1;
    public int MainWindowTop { get; set; } = -1;
    public double MainWindowWidth { get; set; } = 520;

    // ── 系统 ──
    public bool AutoStart { get; set; }

    // ── 服务选择（扩展 Id；空 = 取第一个可用）──
    public string? OcrServiceId { get; set; }
    public string? TranslateServiceId { get; set; }

    /// <summary>被禁用的服务 Id（黑名单式：默认全部启用，对齐原版服务启停）。</summary>
    public List<string> DisabledServiceIds { get; set; } = [];

    /// <summary>上次已提示更新说明的版本号（空 = 首次安装，不弹）。</summary>
    public string LastNoticeVersion { get; set; } = string.Empty;

    // ── 网络 ──
    public ProxySettings Proxy { get; set; } = new();

    // ── 备份 ──
    public BackupSettings Backup { get; set; } = new();

    // ── 热键 ──
    public HotkeyEntry Hotkeys { get; set; } = new();

    /// <summary>从磁盘重读并拷贝到当前单例（保留对象引用，绑定不失效）。导入/恢复后调用。</summary>
    public static void LoadIntoCurrent(string? sourcePath = null)
    {
        var path = sourcePath ?? FilePath;
        try
        {
            if (!File.Exists(path))
                return;
            var fresh = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), JsonOpts);
            if (fresh is null)
                return;
            var cur = Current;
            cur.SourceLang = fresh.SourceLang;
            cur.TargetLang = fresh.TargetLang;
            cur.CtrlCcEnabled = fresh.CtrlCcEnabled;
            cur.MouseSelectionEnabled = fresh.MouseSelectionEnabled;
            cur.MouseSelectionIconEnabled = fresh.MouseSelectionIconEnabled;
            cur.ClipboardMonitorEnabled = fresh.ClipboardMonitorEnabled;
            cur.IncrementalTranslateKey = fresh.IncrementalTranslateKey;
            cur.IsTopmost = fresh.IsTopmost;
            cur.AutoTranslate = fresh.AutoTranslate;
            cur.Theme = fresh.Theme;
            cur.MainWindowLeft = fresh.MainWindowLeft;
            cur.MainWindowTop = fresh.MainWindowTop;
            cur.MainWindowWidth = fresh.MainWindowWidth;
            cur.AutoStart = fresh.AutoStart;
            cur.OcrServiceId = fresh.OcrServiceId;
            cur.TranslateServiceId = fresh.TranslateServiceId;
            cur.DisabledServiceIds = fresh.DisabledServiceIds;
            cur.LastNoticeVersion = fresh.LastNoticeVersion;
            cur.Proxy = fresh.Proxy;
            cur.Backup = fresh.Backup;
            cur.Hotkeys.OpenWindow = fresh.Hotkeys.OpenWindow;
            cur.Hotkeys.InputTranslate = fresh.Hotkeys.InputTranslate;
            cur.Hotkeys.CrosswordTranslate = fresh.Hotkeys.CrosswordTranslate;
            cur.Hotkeys.ScreenshotTranslate = fresh.Hotkeys.ScreenshotTranslate;
            cur.Hotkeys.SilentOcr = fresh.Hotkeys.SilentOcr;
            cur.Hotkeys.SilentTts = fresh.Hotkeys.SilentTts;
            cur.Hotkeys.ImageTranslate = fresh.Hotkeys.ImageTranslate;
        }
        catch (Exception e)
        {
            Program.LogError("AppSettings.LoadIntoCurrent", e);
        }
    }

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath), JsonOpts) ?? new AppSettings();
        }
        catch (Exception e)
        {
            Program.LogError("AppSettings.Load", e);
        }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, JsonOpts));
        }
        catch (Exception e)
        {
            Program.LogError("AppSettings.Save", e);
        }
    }
}
