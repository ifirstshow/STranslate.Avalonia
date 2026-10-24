using STranslate.Sdk;
using System.Text.Json;

namespace STranslate.Extensions.Translate.MTranServer;

/// <summary>MTranServer（本地离线翻译服务）扩展。POST {from,to,text}，读取 result 字段。</summary>
[Extension(
    "9e44abfa040e443c9ab48205683082f4",
    "MTranServer",
    Description = "MTranServer 本地离线翻译",
    Author = "zggsong")]
public sealed class Main : ITranslatePlugin
{
    private const string UrlId = "url";
    private const string TokenId = "token";

    private IPluginContext Context { get; set; } = null!;

    public IReadOnlyList<SettingItem> SettingItems { get; } =
    [
        new TextSetting(UrlId, "Url", Placeholder: "http://localhost:8989/translate", Default: "http://localhost:8989/translate"),
        new PasswordSetting(TokenId, "Token", Tooltip: "服务端设置 MT_API_TOKEN 时必填；本地部署留空")
    ];

    public void Init(IPluginContext context) => Context = context;

    public void Dispose() { }

    public string? GetSourceLanguage(LangEnum lang) => MapLanguage(lang);
    public string? GetTargetLanguage(LangEnum lang) => MapLanguage(lang);

    public async Task<TranslateResult> TranslateAsync(TranslateRequest request, IProgress<string>? deltas = null, CancellationToken cancellationToken = default)
    {
        var start = DateTimeOffset.UtcNow;

        var source = GetSourceLanguage(request.SourceLang);
        if (source is null)
            return TranslateResult.Fail(Context.I18n.GetTranslation("UnsupportedSourceLang"), DateTimeOffset.UtcNow - start);

        var target = GetTargetLanguage(request.TargetLang);
        if (target is null)
            return TranslateResult.Fail(Context.I18n.GetTranslation("UnsupportedTargetLang"), DateTimeOffset.UtcNow - start);

        var url = Context.Settings.Get(UrlId, "http://localhost:8989/translate");
        var token = Context.Settings.Get(TokenId, string.Empty);

        Options? options = null;
        if (!string.IsNullOrEmpty(token))
        {
            options = new Options
            {
                Headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" }
            };
        }

        var content = new { from = source, to = target, text = request.Text };
        var response = await Context.Http.PostAsync(url, content, options, cancellationToken);

        var translated = JsonDocument.Parse(response).RootElement.GetProperty("result").GetString();
        return translated is null
            ? TranslateResult.Fail(response, DateTimeOffset.UtcNow - start)
            : TranslateResult.Ok(translated, DateTimeOffset.UtcNow - start);
    }

    private static string? MapLanguage(LangEnum lang) => lang switch
    {
        LangEnum.ChineseSimplified => "zh",
        LangEnum.English => "en",
        LangEnum.Japanese => "ja",
        LangEnum.Korean => "ko",
        LangEnum.French => "fr",
        LangEnum.Spanish => "es",
        LangEnum.Russian => "ru",
        LangEnum.German => "de",
        LangEnum.Italian => "it",
        LangEnum.Turkish => "tr",
        LangEnum.PortuguesePortugal => "pt",
        LangEnum.PortugueseBrazil => "pt",
        LangEnum.Vietnamese => "vi",
        LangEnum.Indonesian => "id",
        LangEnum.Malay => "ms",
        LangEnum.Arabic => "ar",
        LangEnum.Hindi => "hi",
        LangEnum.Persian => "fa",
        LangEnum.Swedish => "sv",
        LangEnum.Polish => "pl",
        LangEnum.Dutch => "nl",
        LangEnum.Ukrainian => "uk",
        LangEnum.Uzbek => "uz",
        _ => null
    };
}
