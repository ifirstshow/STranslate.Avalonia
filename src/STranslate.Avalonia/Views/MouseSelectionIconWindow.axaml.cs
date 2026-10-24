using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using STranslate.Avalonia.Services.Triggers;

namespace STranslate.Avalonia.Views;

/// <summary>
/// 划词悬浮图标：出现在选区附近，点击后取词翻译。
/// 5 秒无操作或点击其他位置自动消失（对齐 WPF 悬浮球行为）。
/// </summary>
public partial class MouseSelectionIconWindow : Window
{
    private readonly Action _onTrigger;
    private readonly DispatcherTimer _autoHide;

    public MouseSelectionIconWindow(Action onTrigger)
    {
        _onTrigger = onTrigger;
        InitializeComponent();
        GoButton.Click += (_, _) =>
        {
            Hide();
            _onTrigger();
        };
        Deactivated += (_, _) => Close();
        LostFocus += (_, _) => Close();

        // 5 秒自动消失
        _autoHide = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _autoHide.Tick += (_, _) => Close();
        _autoHide.Start();
    }

    /// <summary>在物理坐标附近显示悬浮图标（x/y 为屏幕物理像素）。</summary>
    public static MouseSelectionIconWindow Show(PixelPoint screenPos, Action onTrigger, double scaling)
    {
        var window = new MouseSelectionIconWindow(onTrigger)
        {
            Position = new PixelPoint(screenPos.X + (int)(14 * scaling), screenPos.Y + (int)(14 * scaling))
        };
        window.Show();
        return window;
    }
}
