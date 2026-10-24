using System.Text.Json;
using STranslate.Sdk;

namespace STranslate.Avalonia.Services;

/// <summary>
/// worker 插件的宿主侧代理：序列化请求经 WorkerProcessManager 转发。
/// SettingItems 由 describe 提供（JSON 元数据，UI 另行渲染）。
/// </summary>
public abstract class WorkerProxyBase
{
    protected WorkerProcessManager Manager { get; }
    public string PluginId { get; }
    public string DisplayName { get; }
    public JsonElement SettingItemsMeta { get; }

    private static readonly JsonSerializerOptions ParseOpts = new() { PropertyNameCaseInsensitive = true };

    protected WorkerProxyBase(WorkerProcessManager manager, WorkerPluginDescription desc)
    {
        Manager = manager;
        PluginId = desc.Id;
        DisplayName = desc.Name;
        SettingItemsMeta = desc.SettingItems;
    }

    protected async Task<string> CallAsync(string method, object payload, CancellationToken ct) =>
        await Manager.CallAsync(method, JsonSerializer.Serialize(payload), ct);

    protected async Task<T> CallAndParseAsync<T>(string method, object payload, CancellationToken ct)
    {
        var json = await CallAsync(method, payload, ct);
        return JsonSerializer.Deserialize<T>(json, ParseOpts)
               ?? throw new InvalidOperationException("worker returned null");
    }
}

public sealed class WorkerTranslateProxy : WorkerProxyBase, ITranslatePlugin
{
    public WorkerTranslateProxy(WorkerProcessManager manager, WorkerPluginDescription desc)
        : base(manager, desc) { }

    public Task<TranslateResult> TranslateAsync(TranslateRequest request, IProgress<string>? deltas = null,
        CancellationToken cancellationToken = default)
        => CallAndParseAsync<TranslateResult>("translate", new
        {
            pluginId = PluginId,
            request = new
            {
                text = request.Text,
                sourceLang = request.SourceLang.ToString(),
                targetLang = request.TargetLang.ToString()
            }
        }, cancellationToken);

    public string? GetSourceLanguage(LangEnum lang) => lang.ToString();
    public string? GetTargetLanguage(LangEnum lang) => lang.ToString();
    public IReadOnlyList<SettingItem> SettingItems { get; } = [];
    public void Init(IPluginContext context) { }
    public void Dispose() { }
}

public sealed class WorkerOcrProxy : WorkerProxyBase, IOcrPlugin
{
    public WorkerOcrProxy(WorkerProcessManager manager, WorkerPluginDescription desc)
        : base(manager, desc) { }

    public Task<OcrResult> RecognizeAsync(OcrRequest request, CancellationToken cancellationToken = default)
        => CallAndParseAsync<OcrResult>("ocr", new
        {
            pluginId = PluginId,
            request = new
            {
                imageBase64 = Convert.ToBase64String(request.ImageData),
                imageFormat = request.ImageFormat,
                sourceLang = request.Language.ToString(),
                pixelWidth = request.PixelWidth,
                pixelHeight = request.PixelHeight
            }
        }, cancellationToken);

    public IReadOnlyList<SettingItem> SettingItems { get; } = [];
    public IEnumerable<LangEnum> SupportedLanguages => Enum.GetValues<LangEnum>();
    public void Init(IPluginContext context) { }
    public void Dispose() { }
}

public sealed class WorkerTtsProxy : WorkerProxyBase, ITtsPlugin
{
    public WorkerTtsProxy(WorkerProcessManager manager, WorkerPluginDescription desc)
        : base(manager, desc) { }

    public async Task<AudioData> SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        var json = await CallAndParseAsync<JsonElement>("tts", new { pluginId = PluginId, text }, cancellationToken);
        return new AudioData(
            Convert.FromBase64String(json.GetProperty("audioBase64").GetString() ?? ""),
            Enum.TryParse<AudioFormat>(json.GetProperty("format").GetString(), out var f) ? f : AudioFormat.Mp3);
    }

    public IReadOnlyList<SettingItem> SettingItems { get; } = [];
    public void Init(IPluginContext context) { }
    public void Dispose() { }
}

public sealed class WorkerVocabularyProxy : WorkerProxyBase, IVocabularyPlugin
{
    public WorkerVocabularyProxy(WorkerProcessManager manager, WorkerPluginDescription desc)
        : base(manager, desc) { }

    public Task<VocabularyResult> SaveAsync(string word, CancellationToken cancellationToken = default)
        => CallAndParseAsync<VocabularyResult>("vocabulary", new { pluginId = PluginId, text = word }, cancellationToken);

    public IReadOnlyList<SettingItem> SettingItems { get; } = [];
    public void Init(IPluginContext context) { }
    public void Dispose() { }
}
