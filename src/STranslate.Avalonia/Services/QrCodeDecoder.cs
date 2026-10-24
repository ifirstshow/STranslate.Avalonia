using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing.ImageSharp;

namespace STranslate.Avalonia.Services;

/// <summary>二维码/条码解码（ZXing.Net + ImageSharp 绑定，AOT 兼容、无 System.Drawing）。</summary>
public static class QrCodeDecoder
{
    /// <summary>解码图片字节中的二维码/条码，未识别或解码失败返回 null。</summary>
    public static string? Decode(byte[] imageBytes)
    {
        try
        {
            using var image = Image.Load<Rgba32>(imageBytes);
            var reader = new ZXing.ImageSharp.BarcodeReader<Rgba32>();
            return reader.Decode(image)?.Text;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>从二维码文本提取可打开的网址。</summary>
    public static string? TryGetWebUri(string text) =>
        Uri.TryCreate(text.Trim(), UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? uri.ToString()
            : null;
}
