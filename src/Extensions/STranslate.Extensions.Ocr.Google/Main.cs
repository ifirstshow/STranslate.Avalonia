using System.Text.Json.Nodes;
using STranslate.Sdk;

namespace STranslate.Extensions.Ocr.Google;

/// <summary>Google Cloud Vision OCR（TEXT_DETECTION）。需要用户自己的 API Key。</summary>
[Extension(
    "64f812516673408ab5b59e56720bd641",
    "Google OCR",
    Description = "Google Cloud Vision 文字识别",
    Author = "zggsong")]
public sealed class Main : IOcrPlugin
{
    private const string UrlId = "url";
    private const string ApiKeyId = "apiKey";

    private IPluginContext Context { get; set; } = null!;

    public IReadOnlyList<SettingItem> SettingItems { get; } =
    [
        new TextSetting(UrlId, "Url", Default: "https://vision.googleapis.com/"),
        new PasswordSetting(ApiKeyId, "API Key")
    ];

    public IEnumerable<LangEnum> SupportedLanguages => Enum.GetValues<LangEnum>();

    public bool SupportBoxPoints() => true;

    public void Init(IPluginContext context) => Context = context;

    public void Dispose() { }

    public async Task<OcrResult> RecognizeAsync(OcrRequest request, CancellationToken cancellationToken = default)
    {
        var start = DateTimeOffset.UtcNow;
        var baseUrl = Context.Settings.Get(UrlId, "https://vision.googleapis.com/").TrimEnd('/');
        var apiKey = Context.Settings.Get(ApiKeyId, string.Empty);
        if (string.IsNullOrEmpty(apiKey))
            return new OcrResult { Duration = DateTimeOffset.UtcNow - start }.Fail("Google OCR 未配置 API Key");

        var uri = $"{baseUrl}/v1/images:annotate?key={apiKey}";
        var content = new
        {
            requests = new[]
            {
                new
                {
                    features = new[] { new { type = "TEXT_DETECTION" } },
                    image = new { content = Convert.ToBase64String(request.ImageData) }
                }
            }
        };

        var resp = await Context.Http.PostAsync(uri, content, cancellationToken: cancellationToken);
        var parsed = JsonNode.Parse(resp) ?? throw new InvalidOperationException($"反序列化失败: {resp}");

        if (parsed["error"] is JsonNode error)
            return new OcrResult { Duration = DateTimeOffset.UtcNow - start }.Fail(error["message"]?.ToString() ?? "unknown error");

        var result = new OcrResult { Duration = DateTimeOffset.UtcNow - start };
        var annotations = parsed["responses"]?[0]?["textAnnotations"] as JsonArray;
        if (annotations is null || annotations.Count == 0)
            return result; // 无结果：空内容，宿主按失败提示

        // 跳过第 0 项整图聚合文本，只取词级结果
        for (var i = 1; i < annotations.Count; i++)
        {
            if (annotations[i] is not { } annotation)
                continue;

            var ocrContent = new OcrContent { Text = annotation["description"]?.ToString() ?? string.Empty };
            foreach (var point in ConvertVertices(annotation["boundingPoly"]?["vertices"] as JsonArray))
            {
                if (point.X != point.Y || point.X != 0)
                    ocrContent.BoxPoints.Add(point);
            }
            result.OcrContents.Add(ocrContent);
        }
        return result;
    }

    /// <summary>vertices 顺序：[0]左上 [1]右上 [2]右下 [3]左下；缺省返回 4 个 (0,0)。</summary>
    private static List<BoxPoint> ConvertVertices(JsonArray? vertices)
    {
        if (vertices is null || vertices.Count == 0)
            return [new BoxPoint(0, 0), new BoxPoint(0, 0), new BoxPoint(0, 0), new BoxPoint(0, 0)];

        return
        [
            new BoxPoint(vertices[0]?["x"]?.GetValue<float>() ?? 0, vertices[0]?["y"]?.GetValue<float>() ?? 0),
            new BoxPoint(vertices[1]?["x"]?.GetValue<float>() ?? 0, vertices[1]?["y"]?.GetValue<float>() ?? 0),
            new BoxPoint(vertices[2]?["x"]?.GetValue<float>() ?? 0, vertices[2]?["y"]?.GetValue<float>() ?? 0),
            new BoxPoint(vertices[3]?["x"]?.GetValue<float>() ?? 0, vertices[3]?["y"]?.GetValue<float>() ?? 0)
        ];
    }
}
