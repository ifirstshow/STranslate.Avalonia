namespace STranslate.Sdk;

/// <summary>
/// OCR 扩展接口。
/// </summary>
public interface IOcrPlugin : IPlugin
{
    /// <summary>支持的识别语言。</summary>
    IEnumerable<LangEnum> SupportedLanguages { get; }

    /// <summary>
    /// 是否支持返回文本框像素坐标。图片翻译需要该能力，普通 OCR 可不实现（默认 false）。
    /// </summary>
    bool SupportBoxPoints() => false;

    /// <summary>识别图片中的文本。图像已由宿主按全局图片质量设置编码完成。</summary>
    Task<OcrResult> RecognizeAsync(OcrRequest request, CancellationToken cancellationToken = default);
}

/// <summary>OCR 请求。</summary>
/// <param name="ImageData">图像字节（已按宿主图片质量设置编码）。</param>
/// <param name="ImageFormat">图像 MIME 类型（如 image/png、image/jpeg）。</param>
/// <param name="Language">目标识别语言。</param>
/// <param name="PixelWidth">图像像素宽。</param>
/// <param name="PixelHeight">图像像素高。</param>
public sealed record OcrRequest(byte[] ImageData, string ImageFormat, LangEnum Language, int PixelWidth = 0, int PixelHeight = 0);

/// <summary>OCR 结果（纯数据，可 JSON 序列化）。两种形态二选一：<see cref="OcrContents"/> 扁平行，或 <see cref="Regions"/> 结构化区域。</summary>
public sealed class OcrResult
{
    /// <summary>纯文本结果（由内容/区域聚合）。</summary>
    public string Text => OcrContents.Count > 0
        ? string.Join(Environment.NewLine, OcrContents.Select(x => x.Text)).Trim()
        : string.Join(Environment.NewLine,
                Regions.SelectMany(r => r.Paragraphs).Select(JoinParagraphLines).Where(t => !string.IsNullOrWhiteSpace(t))).Trim();

    /// <summary>扁平行内容（含坐标）。</summary>
    public List<OcrContent> OcrContents { get; set; } = [];

    /// <summary>结构化区域。</summary>
    public List<OcrRegion> Regions { get; set; } = [];

    /// <summary>识别出的语言。</summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>耗时。</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>是否成功。</summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>错误信息（失败时）。</summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>标记为失败。</summary>
    public OcrResult Fail(string message)
    {
        IsSuccess = false;
        ErrorMessage = message;
        return this;
    }

    private static string JoinParagraphLines(OcrParagraph paragraph)
    {
        var lines = paragraph.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Text)).ToList();
        if (lines.Count == 0)
            return string.Empty;

        var text = lines[0].Text;
        for (var i = 1; i < lines.Count; i++)
        {
            if (NeedsSpace(text, lines[i].Text))
                text += " ";
            text += lines[i].Text;
        }
        return text;
    }

    private static bool NeedsSpace(string previous, string current)
    {
        if (string.IsNullOrWhiteSpace(previous) || string.IsNullOrWhiteSpace(current))
            return false;

        var left = previous[^1];
        var right = current[0];
        return !char.IsWhiteSpace(left) && !char.IsWhiteSpace(right) &&
               !char.IsPunctuation(left) && !char.IsPunctuation(right) &&
               !TextHelper.IsCjk(left) && !TextHelper.IsCjk(right);
    }
}

/// <summary>OCR 区域：段落集合 + 区域外接框。</summary>
public sealed class OcrRegion
{
    /// <summary>区域内的段落。</summary>
    public List<OcrParagraph> Paragraphs { get; set; } = [];

    /// <summary>区域外接坐标框。</summary>
    public List<BoxPoint> BoxPoints { get; set; } = [];
}

/// <summary>OCR 段落：按阅读顺序排列的文本行。</summary>
public sealed class OcrParagraph
{
    /// <summary>段落内的文本行。</summary>
    public List<OcrContent> Lines { get; set; } = [];

    /// <summary>段落外接坐标框。</summary>
    public List<BoxPoint> BoxPoints { get; set; } = [];
}

/// <summary>OCR 文本行及其包围盒。</summary>
public sealed class OcrContent
{
    /// <summary>文本内容。</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>包围盒顶点集合。</summary>
    public List<BoxPoint> BoxPoints { get; set; } = [];
}

/// <summary>二维坐标点（包围盒顶点）。</summary>
public sealed class BoxPoint(float x, float y)
{
    /// <summary>X 坐标。</summary>
    public float X { get; set; } = x;

    /// <summary>Y 坐标。</summary>
    public float Y { get; set; } = y;
}
