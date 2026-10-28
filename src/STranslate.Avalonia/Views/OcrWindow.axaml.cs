using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using STranslate.Avalonia.Services;
using STranslate.Avalonia.Services.Triggers;
using STranslate.Avalonia.ViewModels;
using STranslate.Sdk;

namespace STranslate.Avalonia.Views;

/// <summary>OCR 独立窗口：截图图片预览 + 识别文字（含识别块标注、导入/保存图片）。</summary>
public partial class OcrWindow : Window
{
    private readonly OcrWindowViewModel _viewModel = new();

    public OcrWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        _viewModel.BoxesChanged += DrawAnnotation;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(OcrWindowViewModel.ShowAnnotation))
                DrawAnnotation();
        };
        PreviewImage.SizeChanged += (_, _) => DrawAnnotation();
    }

    /// <summary>载入截图并执行识别。窗口不持有截图之外的资源。</summary>
    public async Task RunAsync(byte[] png, int pixelWidth, int pixelHeight, IOcrPlugin ocr, LangEnum sourceLang)
    {
        using var stream = new MemoryStream(png);
        PreviewImage.Source = new Bitmap(stream);

        // 基准显示尺寸：适配图片区视口（宽优先，超高则按高适配）
        var viewportW = PreviewScroll.Viewport.Width > 16 ? PreviewScroll.Viewport.Width : 800;
        var viewportH = 340;
        _zoom = 1;
        var ratio = (double)pixelWidth / Math.Max(1, pixelHeight);
        _baseW = Math.Min(viewportW - 16, Math.Min(pixelWidth, viewportH * ratio));
        _baseH = _baseW / ratio;
        if (_baseH > viewportH)
        {
            _baseH = viewportH;
            _baseW = _baseH * ratio;
        }
        ApplyZoom(1);
        PreviewScroll.Offset = new Vector(0, 0);

        await _viewModel.RecognizeAsync(png, pixelWidth, pixelHeight, ocr, sourceLang);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Close();
        base.OnKeyDown(e);
    }

    /// <summary>标注：Canvas 与 Image 同原点同尺寸，框坐标乘显示缩放比即可。</summary>
    private void DrawAnnotation()
    {
        AnnotationCanvas.Children.Clear();
        if (!_viewModel.ShowAnnotation || _viewModel.OcrBoxes.Count == 0 || PreviewImage.Source is not Bitmap source)
            return;
        if (PreviewImage.Width < 1 || PreviewImage.Height < 1)
            return;

        var scale = PreviewImage.Width / source.Size.Width;

        foreach (var box in _viewModel.OcrBoxes)
        {
            var rect = new Rect(box.X * scale, box.Y * scale,
                box.Width * scale, box.Height * scale);
            AnnotationCanvas.Children.Add(new Rectangle
            {
                Width = rect.Width,
                Height = rect.Height,
                Stroke = new SolidColorBrush(Color.FromRgb(255, 64, 64)),
                StrokeThickness = 1
            });
            Canvas.SetLeft(AnnotationCanvas.Children[^1], rect.Left);
            Canvas.SetTop(AnnotationCanvas.Children[^1], rect.Top);
        }
    }

    /// <summary>把识别全文送主窗翻译链路（图片翻译完整路径）。</summary>
    private void OnTranslateClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.ResultText.Length == 0)
            return;
        App.Services.GetRequiredService<MainViewModel>().ShowAndTranslate(_viewModel.ResultText, forceForeground: true);
        Close();
    }

    // ── 图片缩放与平移（布局式：改 Image 尺寸 + ScrollViewer 锚定滚动）──

    private double _zoom = 1;
    private double _baseW;   // zoom=1 时的显示宽（适配视口）
    private double _baseH;

    private void ApplyZoom(double zoom)
    {
        PreviewImage.Width = _baseW * zoom;
        PreviewImage.Height = _baseH * zoom;
        AnnotationCanvas.Width = PreviewImage.Width;
        AnnotationCanvas.Height = PreviewImage.Height;
    }

    private void OnPreviewWheel(object? sender, PointerWheelEventArgs e)
    {
        if (_baseW <= 0)
            return;
        var delta = e.Delta.Y > 0 ? 1.25 : 1 / 1.25;
        var newZoom = Math.Clamp(_zoom * delta, 0.2, 20);

        // 锚定：指针（相对内容原点）下的内容点在缩放后保持在同一视口位置
        var scroll = PreviewScroll;
        var pos = e.GetPosition(scroll);
        var off = scroll.Offset;
        var ratio = newZoom / _zoom;
        var margin = 8; // ZoomHost Margin
        var contentX = pos.X + off.X - margin;
        var contentY = pos.Y + off.Y - margin;

        _zoom = newZoom;
        ApplyZoom(newZoom);
        scroll.Offset = new Vector(contentX * ratio - (pos.X - margin), contentY * ratio - (pos.Y - margin));
        e.Handled = true;
    }

    private void OnPreviewReset(object? sender, RoutedEventArgs e)
    {
        _zoom = 1;
        ApplyZoom(1);
        PreviewScroll.Offset = new Vector(0, 0);
    }

    private void OnCopyClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard && _viewModel.ResultText.Length > 0)
        {
            var transfer = new DataTransfer();
            transfer.Add(DataTransferItem.CreateText(_viewModel.ResultText));
            clipboard.SetDataAsync(transfer);
        }
    }

    private void OnCopyQrClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard && _viewModel.QrCodeText.Length > 0)
        {
            var transfer = new DataTransfer();
            transfer.Add(DataTransferItem.CreateText(_viewModel.QrCodeText));
            clipboard.SetDataAsync(transfer);
        }
    }

    private void OnOpenQrLinkClick(object? sender, RoutedEventArgs e)
    {
        var uri = QrCodeDecoder.TryGetWebUri(_viewModel.QrCodeText);
        if (uri is null)
            return;
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(uri) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _viewModel.ErrorMessage = "打开链接失败：" + ex.Message;
        }
    }

    private async void OnSaveImageClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.PngBytes is null)
            return;
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "保存图片",
            SuggestedFileName = $"ocr-{DateTime.Now:yyyyMMdd-HHmmss}.png",
            DefaultExtension = "png"
        });
        if (file is null)
            return;
        await using var stream = new FileStream(file.Path.LocalPath, FileMode.Create, FileAccess.Write, FileShare.Read);
        await stream.WriteAsync(_viewModel.PngBytes);
    }

    private async void OnImportFileClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.Ocr is null)
            return;
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "导入图片",
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("图片") { Patterns = ["*.png", "*.jpg", "*.jpeg", "*.bmp"] }]
        });
        if (files is not [{ } file])
            return;

        try
        {
            var bytes = await File.ReadAllBytesAsync(file.Path.LocalPath);
            using var stream = new MemoryStream(bytes);
            var bitmap = new Bitmap(stream);
            PreviewImage.Source = bitmap;
            var viewportW = PreviewScroll.Viewport.Width > 16 ? PreviewScroll.Viewport.Width : 800;
            _zoom = 1;
            var r = (double)bitmap.PixelSize.Width / Math.Max(1, bitmap.PixelSize.Height);
            _baseW = Math.Min(viewportW - 16, Math.Min(bitmap.PixelSize.Width, 340 * r));
            _baseH = _baseW / r;
            if (_baseH > 340)
            {
                _baseH = 340;
                _baseW = _baseH * r;
            }
            ApplyZoom(1);
            PreviewScroll.Offset = new Vector(0, 0);
            await _viewModel.RecognizeAsync(bytes, bitmap.PixelSize.Width, bitmap.PixelSize.Height,
                _viewModel.Ocr, _viewModel.SourceLang);
        }
        catch (Exception ex)
        {
            _viewModel.ErrorMessage = "图片读取失败：" + ex.Message;
        }
    }
}
