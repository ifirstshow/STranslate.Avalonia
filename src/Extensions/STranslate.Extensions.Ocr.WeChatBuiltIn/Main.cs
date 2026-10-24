using System.IO;
using STranslate.Sdk;
using ImageOcr = WeChatOcr.ImageOcr;

namespace STranslate.Extensions.Ocr.WeChatBuiltIn;

/// <summary>
/// 微信内置 OCR（离线）。引擎二进制（wco_data）来自上游 WeChatOcr 包（仅供学习用途，
/// vendor 决策与许可注记见 docs/adr/0004）；C# 包装层已 vendor 进本工程并改用 System.Text.Json。
/// </summary>
[Extension(
    "3410e7de989340938301abd6fcf8cc4b",
    "微信OCR",
    Description = "微信本地离线文字识别（需要本机安装微信）",
    Author = "zggsong")]
public sealed class Main : IOcrPlugin
{
    private IPluginContext Context { get; set; } = null!;

    public IReadOnlyList<SettingItem> SettingItems { get; } = [];

    public IEnumerable<LangEnum> SupportedLanguages =>
    [
        LangEnum.Auto,
        LangEnum.ChineseSimplified,
        LangEnum.ChineseTraditional,
        LangEnum.English,
    ];

    public bool SupportBoxPoints() => true;

    public void Init(IPluginContext context)
    {
        Context = context;
        // 引擎数据目录：wco_data 位于宿主输出目录（内置直调形态）
        global::WeChatOcr.DataLocation.SetBaseDirectory(AppContext.BaseDirectory);
    }

    public void Dispose() { }

    public string? GetLanguage(LangEnum langEnum) => null;

    public Task<OcrResult> RecognizeAsync(OcrRequest request, CancellationToken cancellationToken = default)
    {
        var start = DateTimeOffset.UtcNow;
        var tcs = new TaskCompletionSource<OcrResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var ocr = new ImageOcr();
        var registered = false;
        ocr.Run(request.ImageData, (_, result) =>
        {
            if (registered)
                return;
            registered = true;

            var ocrResult = new global::STranslate.Sdk.OcrResult { Duration = DateTimeOffset.UtcNow - start };
            var list = result?.OcrResult?.SingleResult;
            if (list == null)
            {
                ocrResult.Fail("WeChatOCR 未返回结果（请确认本机已安装微信）");
                tcs.TrySetResult(ocrResult);
                return;
            }

            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item.SingleStrUtf8))
                    continue;

                var content = new OcrContent { Text = item.SingleStrUtf8 };
                var width = (int)(item.Right - item.Left);
                var height = (int)(item.Bottom - item.Top);
                foreach (var point in ConvertBox((int)item.Left, (int)item.Top, width, height))
                {
                    if (point.X != point.Y || point.X != 0)
                        content.BoxPoints.Add(point);
                }
                ocrResult.OcrContents.Add(content);
            }
            tcs.TrySetResult(ocrResult);
        });
        return tcs.Task;
    }

    /// <summary>由左上角坐标与宽高生成四角 BoxPoint（左上/右上/右下/左下）。</summary>
    private static List<BoxPoint> ConvertBox(int x, int y, int width, int height) =>
    [
        new(x, y),
        new(x + width, y),
        new(x + width, y + height),
        new(x, y + height)
    ];
}
