using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using STranslate.Avalonia.Services;
using STranslate.Avalonia.Services.Capture;

namespace STranslate.Avalonia.Views;

/// <summary>
/// 单显示器截图遮罩：冻结画面 + 暗化 + 选区高亮。
/// 拖选松开即确认，Esc 取消。选区坐标以物理像素返回。
/// </summary>
public partial class ScreenshotWindow : Window
{
    private readonly MonitorSnapshot _snapshot;
    private readonly TaskCompletionSource<ScreenshotResult?> _tcs;
    private readonly List<ScreenshotWindow> _peers;
    private Point _dragStart;
    private bool _dragging;

    private double Scaling => _snapshot.PixelSize.Width / Bounds.Width;

    public ScreenshotWindow(MonitorSnapshot snapshot, TaskCompletionSource<ScreenshotResult?> tcs, List<ScreenshotWindow> peers)
    {
        _snapshot = snapshot;
        _tcs = tcs;
        _peers = peers;
        InitializeComponent();

        // 窗口 DIP 尺寸 = 物理像素 ÷ 本屏缩放；1:1 覆盖该屏
        Width = snapshot.PixelSize.Width / snapshot.Scaling;
        Height = snapshot.PixelSize.Height / snapshot.Scaling;
        Position = snapshot.ScreenOrigin;

        // 位图自然尺寸是像素值（2×），必须显式约束显示尺寸到窗口大小，否则按 2 倍溢出
        FrozenImage.Width = Width;
        FrozenImage.Height = Height;
        DarkMask.Width = Width;
        DarkMask.Height = Height;
        // 选区高亮层：全尺寸图层原位叠放 + 按选区矩形裁剪（亮区），不得缩放
        SelectionImage.Width = Width;
        SelectionImage.Height = Height;
        SelectionImage.Source = snapshot.Bitmap;
        FrozenImage.Source = snapshot.Bitmap;

        DebugTrace.Log($"overlay size={Width}x{Height} pos={Position} scaling={snapshot.Scaling}");

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        KeyDown += OnKeyDown;
    }

    private Rect _selectionRectDip; // 当前选区（DIP）

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _dragStart = e.GetPosition(Root);
        _dragging = true;
        SelectionBorder.IsVisible = true;
        SelectionImage.IsVisible = true;
        SizeHint.IsVisible = true;
        e.Pointer.Capture(Root);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_dragging)
            return;
        UpdateSelection(_dragStart, e.GetPosition(Root));
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_dragging)
            return;
        _dragging = false;
        UpdateSelection(_dragStart, e.GetPosition(Root));

        var physical = ToPhysical(_selectionRectDip);
        if (physical.Width < 2 || physical.Height < 2)
        {
            Complete(null); // 太小视为取消
            return;
        }
        Complete(CreateResult(physical));
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Complete(null);
        }
    }

    private void UpdateSelection(Point start, Point end)
    {
        var left = Math.Min(start.X, end.X);
        var top = Math.Min(start.Y, end.Y);
        var width = Math.Abs(end.X - start.X);
        var height = Math.Abs(end.Y - start.Y);
        _selectionRectDip = new Rect(left, top, width, height);

        Canvas.SetLeft(SelectionBorder, left);
        Canvas.SetTop(SelectionBorder, top);
        SelectionBorder.Width = width;
        SelectionBorder.Height = height;

        // 选区高亮层保持全窗原位，仅更新裁剪矩形
        SelectionImage.Clip = new RectangleGeometry(new Rect(left, top, width, height));

        Canvas.SetLeft(SizeHint, left);
        Canvas.SetTop(SizeHint, top + height + 6);
        var physical = ToPhysical(_selectionRectDip);
        SizeHint.Text = $"{physical.Width} × {physical.Height}";
    }

    private PixelRect ToPhysical(Rect dip)
    {
        var s = Scaling;
        return new PixelRect(
            _snapshot.ScreenOrigin.X + (int)Math.Round(dip.X * s),
            _snapshot.ScreenOrigin.Y + (int)Math.Round(dip.Y * s),
            (int)Math.Round(dip.Width * s),
            (int)Math.Round(dip.Height * s));
    }

    private ScreenshotResult CreateResult(PixelRect physical)
    {
        // 从冻结位图裁剪选区并编码 PNG
        var cropped = new RenderTargetBitmap(new PixelSize(physical.Width, physical.Height), new Vector(96, 96));
        using (var ctx = cropped.CreateDrawingContext())
        {
            // 源：整张快照；目的：按选区偏移绘制
            var srcRect = new Rect(
                physical.X - _snapshot.ScreenOrigin.X,
                physical.Y - _snapshot.ScreenOrigin.Y,
                physical.Width, physical.Height);
            ctx.DrawImage(_snapshot.Bitmap, srcRect, new Rect(0, 0, physical.Width, physical.Height));
        }

        using var stream = new MemoryStream();
        cropped.Save(stream, new PngBitmapEncoderOptions());
        return new ScreenshotResult(stream.ToArray(), new PixelSize(physical.Width, physical.Height), physical.Position, _snapshot.Scaling);
    }

    private void Complete(ScreenshotResult? result)
    {
        if (_tcs.Task.IsCompleted)
            return;
        _tcs.TrySetResult(result);
        foreach (var peer in _peers)
            peer.Close();
    }
}

/// <summary>一次截图的产物。</summary>
public sealed record ScreenshotResult(byte[] PngBytes, PixelSize PixelSize, PixelPoint ScreenOrigin, double Scaling);
