using STranslate.Sdk;

namespace STranslate.Extensions.Vocabulary.Eudict;

/// <summary>欧路词典生词本。需要用户在欧路开放平台获取 Token 与单词本 ID。</summary>
[Extension(
    "d9537be74d23438ca581fd6d04e1d112",
    "Eudict",
    Description = "保存单词到欧路词典生词本",
    Author = "zggsong")]
public sealed class Main : IVocabularyPlugin
{
    private const string TokenId = "token";
    private const string BookIdId = "bookId";

    private IPluginContext Context { get; set; } = null!;

    public IReadOnlyList<SettingItem> SettingItems { get; } =
    [
        new PasswordSetting(TokenId, "Token", Tooltip: "欧路词典开放平台 API Token"),
        new TextSetting(BookIdId, "BookID", Tooltip: "要写入的单词本 ID")
    ];

    public void Init(IPluginContext context) => Context = context;

    public void Dispose() { }

    public async Task<VocabularyResult> SaveAsync(string word, CancellationToken cancellationToken = default)
    {
        var start = DateTimeOffset.UtcNow;
        var token = Context.Settings.Get(TokenId, string.Empty);
        var bookId = Context.Settings.Get(BookIdId, string.Empty);
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(bookId))
            return new VocabularyResult { Duration = DateTimeOffset.UtcNow - start }.Fail("欧路词典未配置 Token/BookID");

        try
        {
            var content = new { id = bookId, language = "en", words = new[] { word } };
            var options = new Options
            {
                Headers = new Dictionary<string, string> { ["Authorization"] = token }
            };
            var resp = await Context.Http.PostAsync("https://api.frdic.com/api/open/v1/studylist/words", content, options, cancellationToken);

            // 对齐 WPF 原版 GetResult：失败响应才带 status 字段；成功响应的 message 是"单词导入成功…"，不是失败
            var parsed = System.Text.Json.Nodes.JsonNode.Parse(resp);
            var status = parsed?["status"]?.ToString();
            return string.IsNullOrEmpty(status)
                ? new VocabularyResult { Duration = DateTimeOffset.UtcNow - start }
                : new VocabularyResult { Duration = DateTimeOffset.UtcNow - start }.Fail($"接口回复: {resp}");
        }
        catch (Exception e)
        {
            return new VocabularyResult { Duration = DateTimeOffset.UtcNow - start }.Fail(e.Message);
        }
    }
}
