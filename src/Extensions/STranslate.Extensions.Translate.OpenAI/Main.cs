using System.Text.Json;
using STranslate.Sdk;

namespace STranslate.Extensions.Translate.OpenAI;

/// <summary>
/// OpenAI 兼容翻译扩展（Chat Completions）。
/// v2 首版取舍：非流式 + 单条 Prompt（角色结构并入内容）；流式/Responses 模式按需补。
/// </summary>
[Extension(
    "76b14a8d707041c891a2dcd2f74be9c1",
    "OpenAI",
    Description = "OpenAI 兼容接口翻译（支持任意 OpenAI 格式服务）",
    Author = "zggsong")]
public sealed class Main : ITranslatePlugin
{
    private const string UrlId = "url";
    private const string ApiKeyId = "apiKey";
    private const string ModelId = "model";
    private const string TemperatureId = "temperature";
    private const string PromptsId = "prompts";

    private IPluginContext Context { get; set; } = null!;

    private static readonly PromptList DefaultPrompts = new()
    {
        Prompts =
        [
            new Prompt
            {
                Name = "翻译",
                Content = "You are a professional, authentic translation engine. Please translate the following text into $target (avoid explaining the original text):\r\n\r\n$content",
                IsEnabled = true
            },
            new Prompt
            {
                Name = "润色",
                Content = "You are a professional, authentic text polishing engine. Please polish the following text (avoid explaining the original text):\r\n\r\n$content"
            },
            new Prompt
            {
                Name = "总结",
                Content = "You are a professional, authentic text summarization engine. Please summarize the following text:\r\n\r\n$content"
            },
        ]
    };

    public IReadOnlyList<SettingItem> SettingItems { get; } =
    [
        new TextSetting(UrlId, "Url", Default: "https://api.openai.com/v1", Tooltip: "填到版本段即可，如 https://api.openai.com/v1 或 https://open.bigmodel.cn/api/paas/v4"),
        new PasswordSetting(ApiKeyId, "API Key"),
        new TextSetting(ModelId, "Model", Default: "gpt-4o-mini"),
        new NumberSetting(TemperatureId, "Temperature", 0, 2, 0.1, 0.7),
        new PromptListSetting(PromptsId, "Prompt", DefaultPrompts.Prompts),
    ];

    public void Init(IPluginContext context) => Context = context;

    public void Dispose() { }

    public string? GetSourceLanguage(LangEnum lang) => MapLanguage(lang);
    public string? GetTargetLanguage(LangEnum lang) => MapLanguage(lang);

    public async Task<TranslateResult> TranslateAsync(TranslateRequest request, IProgress<string>? deltas = null, CancellationToken cancellationToken = default)
    {
        var start = DateTimeOffset.UtcNow;
        var url = BuildChatUrl(Context.Settings.Get(UrlId, "https://api.openai.com/v1"));
        var apiKey = Context.Settings.Get(ApiKeyId, string.Empty);
        var model = Context.Settings.Get(ModelId, "gpt-4o-mini");
        var temperature = Context.Settings.Get(TemperatureId, 0.7);
        var prompts = Context.Settings.Get<PromptList>(PromptsId, DefaultPrompts);
        var prompt = prompts.Selected ?? DefaultPrompts.Selected;

        if (string.IsNullOrEmpty(apiKey))
            return TranslateResult.Fail("OpenAI 未配置 API Key", DateTimeOffset.UtcNow - start);
        if (prompt is null)
            return TranslateResult.Fail("请先在设置中启用一个 Prompt", DateTimeOffset.UtcNow - start);

        var source = GetSourceLanguage(request.SourceLang) ?? "auto";
        var target = GetTargetLanguage(request.TargetLang) ?? "English";
        var instruction = prompt.Content
            .Replace("$source", source)
            .Replace("$target", target)
            .Replace("$content", request.Text);

        var content = new
        {
            model,
            temperature,
            stream = false,
            messages = new object[] { new { role = "user", content = instruction } }
        };
        var options = new Options
        {
            Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {apiKey}" },
            Timeout = TimeSpan.FromMinutes(2)
        };

        var resp = await Context.Http.PostAsync(url, content, options, cancellationToken);
        var parsed = JsonDocument.Parse(resp).RootElement;
        var text = parsed.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        return text is null
            ? TranslateResult.Fail(resp, DateTimeOffset.UtcNow - start)
            : TranslateResult.Ok(text, DateTimeOffset.UtcNow - start);
    }

    /// <summary>
    /// 智能拼接 chat/completions 地址：
    /// 已含 chat/completions → 原样；末段是版本号（v1/v4…）→ 追加 /chat/completions；
    /// 否则补 /v1/chat/completions（兼容老默认值 https://api.openai.com/）。
    /// </summary>
    internal static string BuildChatUrl(string baseUrl)
    {
        var url = baseUrl.TrimEnd('/');
        if (url.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
            return url;
        var last = url.Split('/')[^1];
        return url + (last.StartsWith('v') && last.Length > 1 && last[1..].All(char.IsDigit)
            ? "/chat/completions"
            : "/v1/chat/completions");
    }

    private static string? MapLanguage(LangEnum lang) => lang switch
    {
        LangEnum.ChineseSimplified => "Simplified Chinese",
        LangEnum.ChineseTraditional => "Traditional Chinese",
        LangEnum.Cantonese => "Cantonese",
        LangEnum.English => "English",
        LangEnum.Japanese => "Japanese",
        LangEnum.Korean => "Korean",
        LangEnum.French => "French",
        LangEnum.Spanish => "Spanish",
        LangEnum.Russian => "Russian",
        LangEnum.German => "German",
        LangEnum.Italian => "Italian",
        LangEnum.Turkish => "Turkish",
        LangEnum.PortuguesePortugal => "Portuguese",
        LangEnum.PortugueseBrazil => "Portuguese",
        LangEnum.Vietnamese => "Vietnamese",
        LangEnum.Indonesian => "Indonesian",
        LangEnum.Thai => "Thai",
        LangEnum.Malay => "Malay",
        LangEnum.Arabic => "Arabic",
        LangEnum.Hindi => "Hindi",
        LangEnum.MongolianCyrillic => "Mongolian",
        LangEnum.MongolianTraditional => "Mongolian",
        LangEnum.Khmer => "Central Khmer",
        LangEnum.NorwegianBokmal => "Norwegian Bokmål",
        LangEnum.NorwegianNynorsk => "Norwegian Nynorsk",
        LangEnum.Persian => "Persian",
        LangEnum.Swedish => "Swedish",
        LangEnum.Polish => "Polish",
        LangEnum.Dutch => "Dutch",
        LangEnum.Ukrainian => "Ukrainian",
        LangEnum.Uzbek => "Uzbek",
        LangEnum.Uyghur => "Uyghur",
        _ => null
    };
}
