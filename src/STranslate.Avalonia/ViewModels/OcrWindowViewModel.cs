using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Avalonia.Services;
using STranslate.Sdk;

namespace STranslate.Avalonia.ViewModels;

/// <summary>识别文字块包围盒（物理像素，窗口按预览比例绘制标注）。</summary>
public sealed record OcrBox(double X, double Y, double Width, double Height);

/// <summary>OCR 独立窗口 VM：图片预览 + 识别文字 + 复制/重识别/标注。</summary>
public partial class OcrWindowViewModel : ObservableObject
{
    [ObservableProperty] private byte[]? _pngBytes;
    [ObservableProperty] private string _resultText = string.Empty;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private bool _showAnnotation = true;
    [ObservableProperty] private string _qrCodeText = string.Empty;

    public bool HasQrCode => QrCodeText.Length > 0;

    partial void OnQrCodeTextChanged(string value) => OnPropertyChanged(nameof(HasQrCode));

    public List<OcrBox> OcrBoxes { get; private set; } = [];

    public IOcrPlugin? Ocr { get; private set; }
    public LangEnum SourceLang { get; private set; }

    public event Action? BoxesChanged;

    public async Task RecognizeAsync(byte[] png, int pixelWidth, int pixelHeight, IOcrPlugin ocr, LangEnum sourceLang)
    {
        Ocr = ocr;
        SourceLang = sourceLang;
        PngBytes = png;
        IsProcessing = true;
        ErrorMessage = null;
        ResultText = string.Empty;
        QrCodeText = string.Empty;
        OcrBoxes = [];

        try
        {
            var result = await ocr.RecognizeAsync(
                new OcrRequest(png, "image/png", sourceLang, pixelWidth, pixelHeight));
            ResultText = result.Text;
            OcrBoxes = [.. result.OcrContents
                .Where(c => !string.IsNullOrWhiteSpace(c.Text) && c.BoxPoints.Count > 0)
                .Select(c => new OcrBox(
                    c.BoxPoints.Min(p => (double)p.X),
                    c.BoxPoints.Min(p => (double)p.Y),
                    c.BoxPoints.Max(p => (double)p.X) - c.BoxPoints.Min(p => (double)p.X),
                    c.BoxPoints.Max(p => (double)p.Y) - c.BoxPoints.Min(p => (double)p.Y)))
                .Where(b => b.Width > 0 && b.Height > 0)];
            BoxesChanged?.Invoke();
            if (!result.IsSuccess && string.IsNullOrWhiteSpace(ResultText))
                ErrorMessage = result.ErrorMessage;

            // 二维码与 OCR 并行解码（同一张图可能两者都有）
            QrCodeText = await Task.Run(() => QrCodeDecoder.Decode(png)) ?? string.Empty;
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task ReRecognizeAsync()
    {
        if (PngBytes is null || Ocr is null || IsProcessing)
            return;
        var (w, h) = GetPngSize();
        await RecognizeAsync(PngBytes, w, h, Ocr, SourceLang);
    }

    private (int, int) GetPngSize()
    {
        // PNG 尺寸取自 IHDR（宽高固定偏移 16/20），避免解码整图
        if (PngBytes is { Length: > 24 } b && b[0] == 0x89 && b[1] == (byte)'P')
        {
            var w = (b[16] << 24) | (b[17] << 16) | (b[18] << 8) | b[19];
            var h = (b[20] << 24) | (b[21] << 16) | (b[22] << 8) | b[23];
            return (w, h);
        }
        return (0, 0);
    }
}
