using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using STranslate.Avalonia.Services;
using STranslate.Avalonia.Services.Capture;

namespace STranslate.Avalonia.Views;

/// <summary>覆盖单个文字块的译文卡片（窗口 DIP 坐标）。</summary>
public sealed record ImageTranslateItem(double X, double Y, double Width, double Height, string Text);

/// <summary>
/// 图片翻译贴图窗口：图片 1:1 钉在截图选区位置，译文白底卡片按坐标覆盖。
/// 按钮条优先放图片下方（窗口向下延伸），下方放不下放上方，都不行才叠加在图片内底部。
/// 支持拖动、重新翻译、重新截图；Esc 关闭。
/// </summary>
public partial class ImageTranslateWindow : Window
{
    private const double BarHeight = 44; // ponytail: 胶囊实测高（按钮32+内边距），改按钮样式时同步
    private const double Gap = 8;

    private readonly Action? _onRerun;
    private readonly Action? _onRescreenshot;
    private readonly double _imgW;
    private readonly double _imgH;

    public ImageTranslateWindow(byte[] png, PixelPoint screenOrigin, PixelSize physicalSize, double scaling,
        IEnumerable<ImageTranslateItem> items, Action? onRerun, Action? onRescreenshot)
    {
        _onRerun = onRerun;
        _onRescreenshot = onRescreenshot;
        InitializeComponent();

        _imgW = physicalSize.Width / scaling;
        _imgH = physicalSize.Height / scaling;
        Width = _imgW;
        Height = _imgH;
        Position = screenOrigin;

        // 图片固定尺寸钉在窗口原点，勿随窗口延伸拉伸
        SourceImage.Width = _imgW;
        SourceImage.Height = _imgH;
        SourceImage.HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left;
        SourceImage.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top;
        SourceImage.Stretch = Stretch.Fill;

        using var stream = new MemoryStream(png);
        SourceImage.Source = new Bitmap(stream);

        foreach (var item in items)
            OverlayCanvas.Children.Add(BuildCard(item));

        SourceImage.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        };

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
                Close();
        };

        // 失焦自动关闭：等真正激活并稳定 300ms 后再挂接，避免截图流程焦点交接瞬间误关
        var deactivateHooked = false;
        Activated += (_, _) =>
        {
            DebugTrace.Log("imwin: Activated");
            if (deactivateHooked)
                return;
            deactivateHooked = true;
            var timer = new global::Avalonia.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                Deactivated += (_, _) =>
                {
                    DebugTrace.Log("imwin: Deactivated close");
                    Close();
                };
            };
            timer.Start();
        };
    }

    /// <summary>创建并显示贴图窗口。</summary>
    public static void ShowOverlay(byte[] png, PixelPoint screenOrigin, PixelSize physicalSize, double scaling,
        IEnumerable<ImageTranslateItem> items, Action? onRerun, Action? onRescreenshot)
    {
        DebugTrace.Log($"imwin: ShowOverlay origin={screenOrigin} size={physicalSize} scaling={scaling} items={items.Count()}");
        new ImageTranslateWindow(png, screenOrigin, physicalSize, scaling, items, onRerun, onRescreenshot).Show();
    }

    private static Control BuildCard(ImageTranslateItem item)
    {
        var fontSize = Math.Clamp(item.Height * 0.72, 14, 42);
        var text = new TextBlock
        {
            Text = item.Text,
            TextWrapping = TextWrapping.Wrap,
            FontSize = fontSize,
            Foreground = Brushes.Black,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center
        };
        var card = new Border
        {
            Background = new SolidColorBrush(Colors.White, 0.92),
            Padding = new Thickness(2, 1),
            Child = text,
            Width = item.Width,
            MinHeight = item.Height
        };
        Canvas.SetLeft(card, item.X);
        Canvas.SetTop(card, item.Y);
        return card;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        // 深色半透明胶囊工具栏（对齐 WPF 原版 CompactWindow 样式）
        var bar = new StackPanel
        {
            Orientation = global::Avalonia.Layout.Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center
        };
        if (_onRerun is not null)
        {
            var rerun = new Button { Content = "重新翻译", Foreground = Brushes.Black };
            rerun.Click += (_, _) =>
            {
                Close();
                _onRerun();
            };
            bar.Children.Add(rerun);
        }
        if (_onRescreenshot is not null)
        {
            var rescreenshot = new Button { Content = "重新截图", Foreground = Brushes.Black };
            rescreenshot.Click += (_, _) =>
            {
                Close();
                _onRescreenshot();
            };
            bar.Children.Add(rescreenshot);
        }
        var close = new Button { Content = "关闭", Foreground = Brushes.Black };
        close.Click += (_, _) => Close();
        bar.Children.Add(close);

        var capsule = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#CC1F1F1F")),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(10, 6),
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
            Child = bar
        };
        ((Panel)Content).Children.Add(capsule);

        ApplyLayout(capsule);
    }

    /// <summary>按钮条避让：图片钉住不动，优先下方 → 上方 → 叠加图片内底部（对齐原版铁律）。</summary>
    private void ApplyLayout(Border capsule)
    {
        try
        {
        var workAreaBottom = double.MaxValue;
        var workAreaTop = double.MinValue;
        var screen = Screens.ScreenFromPoint(Position);
        if (screen is not null)
        {
            var wa = screen.WorkingArea;
            var scaling = screen.Scaling;
            workAreaBottom = (wa.Y + wa.Height) / scaling;
            workAreaTop = wa.Y / scaling;
        }

        var imgTop = Position.Y;
        var imgBottom = imgTop + _imgH;

        if (workAreaBottom - imgBottom >= BarHeight + Gap)
        {
            // 下方放得下：窗口向下延伸，胶囊贴底
            Height = _imgH + Gap + BarHeight;
            capsule.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Bottom;
            capsule.Margin = new Thickness(0, 0, 0, 2);
        }
        else if (imgTop - workAreaTop >= BarHeight + Gap)
        {
            // 下方放不下、上方放得下：窗口向上延伸，胶囊贴顶，图片压到窗口底部
            var ext = BarHeight + Gap;
            Position = new PixelPoint(Position.X, imgTop - (int)Math.Round(ext));
            Height = _imgH + ext;
            capsule.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top;
            capsule.Margin = new Thickness(0, 0, 0, 0);
            SourceImage.Margin = new Thickness(0, ext, 0, 0);
            OverlayCanvas.Margin = new Thickness(0, ext, 0, 0);
        }
        else
        {
            // 上下都放不下：叠加在图片内底部
            capsule.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Bottom;
            capsule.Margin = new Thickness(0, 0, 0, 6);
        }
        }
        catch (Exception ex)
        {
            DebugTrace.Log($"imwin: ApplyLayout failed: {ex}");
        }
    }
}
