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
        ZoomHost.RenderTransform = new global::Avalonia.Media.MatrixTransform(Matrix.Identity);
    }

    /// <summary>载入截图并执行识别。窗口不持有截图之外的资源。</summary>
    public async Task RunAsync(byte[] png, int pixelWidth, int pixelHeight, IOcrPlugin ocr, LangEnum sourceLang)
    {
        using var stream = new MemoryStream(png);
        PreviewImage.Source = new Bitmap(stream);
        await _viewModel.RecognizeAsync(png, pixelWidth, pixelHeight, ocr, sourceLang);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Close();
        base.OnKeyDown(e);
    }

    /// <summary>
    /// 标注换算：Avalonia Image（Uniform）的 Bounds 就是内容适配后的矩形（非容器），
    /// Canvas 设为同尺寸并居中对齐 Image，框坐标只乘一个缩放比即可。
    /// </summary>
    private void DrawAnnotation()
    {
        AnnotationCanvas.Children.Clear();
        if (!_viewModel.ShowAnnotation || _viewModel.OcrBoxes.Count == 0 || PreviewImage.Source is not Bitmap source)
            return;
        if (PreviewImage.Bounds.Width < 1 || PreviewImage.Bounds.Height < 1)
            return;

        var dipW = source.Size.Width;
        var scale = PreviewImage.Bounds.Width / dipW;

        AnnotationCanvas.Width = PreviewImage.Bounds.Width;
        AnnotationCanvas.Height = PreviewImage.Bounds.Height;
        AnnotationCanvas.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center;
        AnnotationCanvas.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center;

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

    // ── 图片缩放与平移（Ctrl+滚轮 / 拖拽 / 双击复位）──

    private double _zoom = 1;
    private double _offsetX, _offsetY;
    private Point _dragStart;
    private double _startX, _startY;
    private bool _dragging;

    private void ApplyTransform() =>
        ZoomHost.RenderTransform = new global::Avalonia.Media.MatrixTransform(
            new global::Avalonia.Matrix(_zoom, 0, 0, _zoom, _offsetX, _offsetY));

    // ctor 内挂接：ZoomHost.RenderTransform = new TransformGroup { _zoomScale, _zoomTranslate }

    private void OnPreviewWheel(object? sender, PointerWheelEventArgs e)
    {
        var delta = e.Delta.Y > 0 ? 1.1 : 1 / 1.1;
        var newZoom = Math.Clamp(_zoom * delta, 0.2, 8);
        // 锚定必须用未变换的面板坐标系（ZoomHost 坐标已被当前缩放影响）
        var pos = e.GetPosition(PreviewPanel);

        // 围绕指针位置缩放
        var ratio = newZoom / _zoom;
        _offsetX = pos.X - (pos.X - _offsetX) * ratio;
        _offsetY = pos.Y - (pos.Y - _offsetY) * ratio;
        _zoom = newZoom;
        ApplyTransform();
        e.Handled = true;
    }

    private void OnPreviewPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && _zoom > 1)
        {
            _dragging = true;
            _dragStart = e.GetPosition(PreviewPanel);
            _startX = _offsetX;
            _startY = _offsetY;
        }
    }

    private void OnPreviewMoved(object? sender, PointerEventArgs e)
    {
        if (!_dragging)
            return;
        var pos = e.GetPosition(PreviewPanel);
        _offsetX = _startX + (pos.X - _dragStart.X);
        _offsetY = _startY + (pos.Y - _dragStart.Y);
        ApplyTransform();
    }

    private void OnPreviewReset(object? sender, RoutedEventArgs e)
    {
        _zoom = 1;
        _offsetX = _offsetY = 0;
        _dragging = false;
        ApplyTransform();
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
            await _viewModel.RecognizeAsync(bytes, bitmap.PixelSize.Width, bitmap.PixelSize.Height,
                _viewModel.Ocr, _viewModel.SourceLang);
        }
        catch (Exception ex)
        {
            _viewModel.ErrorMessage = "图片读取失败：" + ex.Message;
        }
    }
}
