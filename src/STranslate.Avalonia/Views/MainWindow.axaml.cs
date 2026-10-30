using Avalonia;
using STranslate.Sdk;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using STranslate.Avalonia.Services;
using STranslate.Avalonia.Services.Capture;
using STranslate.Avalonia.Services.Triggers;
using STranslate.Avalonia.ViewModels;

namespace STranslate.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Opened += (_, _) =>
        {
            // 位置/宽度记忆（-1 = 首次，交给系统默认居中）
            var s = AppSettings.Current;
            if (s.MainWindowWidth >= MinWidth)
                Width = s.MainWindowWidth;
            if (s.MainWindowLeft >= 0 && s.MainWindowTop >= 0)
                Position = new PixelPoint(s.MainWindowLeft, s.MainWindowTop);

            PositionChanged += (_, __) => DebounceSaveLayout();
            SizeChanged += (_, __) => DebounceSaveLayout();

            Topmost = viewModel.IsTopmost;
            ApplyTheme(viewModel.Theme);
            if (OperatingSystem.IsWindows())
            {
                var hwnd = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
                if (hwnd != IntPtr.Zero)
                {
                    Gdi32.AddClipboardFormatListener(hwnd);
                    Win32Properties.AddWndProcHookCallback(this, ClipboardWndProc);
                }
            }
            FocusInput();
        };
            Opened += (_, _) => InitLangCombos();
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.IsTopmost))
                Topmost = viewModel.IsTopmost;
            else if (e.PropertyName == nameof(MainViewModel.Theme))
                ApplyTheme(viewModel.Theme);
            else if (e.PropertyName == nameof(MainViewModel.SourceLang))
            {
                _langComboSyncing = true;
                SourceLangCombo.SelectedItem = viewModel.SourceLang;
                _langComboSyncing = false;
            }
            else if (e.PropertyName == nameof(MainViewModel.TargetLang))
            {
                _langComboSyncing = true;
                TargetLangCombo.SelectedItem = viewModel.TargetLang;
                _langComboSyncing = false;
            }
            else if (e.PropertyName == nameof(MainViewModel.ClipboardMonitorEnabled))
                SyncClipboardMonitor(viewModel.ClipboardMonitorEnabled);
        };
        viewModel.ClipboardMonitorToggled += SyncClipboardMonitor;
    }

    private bool _langComboSyncing = true;

    private System.Threading.CancellationTokenSource? _layoutSaveCts;

    /// <summary>位置/宽度防抖保存（拖动/缩放停止 800ms 后落盘）。</summary>
    private void DebounceSaveLayout()
    {
        _layoutSaveCts?.Cancel();
        _layoutSaveCts = new System.Threading.CancellationTokenSource();
        var token = _layoutSaveCts.Token;
        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                await System.Threading.Tasks.Task.Delay(800, token);
                global::Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var s = AppSettings.Current;
                    s.MainWindowLeft = Position.X;
                    s.MainWindowTop = Position.Y;
                    s.MainWindowWidth = Width;
                    s.Save();
                });
            }
            catch (OperationCanceledException) { }
        });
    }

    /// <summary>Combo 初始化（代码后台选择，绕开 ItemsSource 重置首项回写）。</summary>
    private void InitLangCombos()
    {
        var vm = (MainViewModel)DataContext;
        SourceLangCombo.ItemsSource = vm.Languages;
        TargetLangCombo.ItemsSource = vm.Languages;
        SourceLangCombo.SelectedItem = vm.SourceLang;
        TargetLangCombo.SelectedItem = vm.TargetLang;
        _langComboSyncing = false;
    }

    private void OnSourceLangChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_langComboSyncing || DataContext is not MainViewModel vm || SourceLangCombo.SelectedItem is not LangEnum lang)
            return;
        vm.SourceLang = lang;
    }

    private void OnTargetLangChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_langComboSyncing || DataContext is not MainViewModel vm || TargetLangCombo.SelectedItem is not LangEnum lang)
            return;
        vm.TargetLang = lang;
    }

    /// <summary>应用主题（Default=跟随系统）。</summary>
    private void ApplyTheme(string theme)
    {
        Application.Current!.RequestedThemeVariant = theme switch
        {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }

    private void OnToggleTheme(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.ToggleTheme();
    }

    private void OnSwapLanguages(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.SwapLanguagesCommand.Execute(null);
    }

    /// <summary>服务切换弹层：选中后收起。</summary>
    private void OnServiceSwitcherSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
            ServiceSwitcherButton.Flyout?.Hide();
    }

    private void OnResultDoubleTapped(object? sender, TappedEventArgs e)
    {
        // 双击结果区 = 复制
        if (DataContext is MainViewModel { ResultText.Length: > 0 })
            OnCopyClick(this, e);
    }

    /// <summary>词典发音：下载 AudioUrl 交宿主播放器。</summary>
    private async void OnPhoneticTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Control { DataContext: STranslate.Sdk.Symbol { AudioUrl.Length: > 0 } symbol })
            return;
        try
        {
            // iciba 返回协议相对地址（//dict-pk...），补全 https
            var url = symbol.AudioUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? symbol.AudioUrl
                : "https:" + symbol.AudioUrl;
            using var http = new System.Net.Http.HttpClient();
            var bytes = await http.GetByteArrayAsync(url);
            // 播放阻塞至结束，放后台避免卡 UI
            _ = Task.Run(() => App.Services.GetRequiredService<HostAudioPlayer>()
                .Play(new STranslate.Sdk.AudioData(bytes, STranslate.Sdk.AudioFormat.Auto)));
        }
        catch (Exception ex)
        {
            Program.LogError("PhoneticPlay", ex);
        }
    }

    private async void OnCopyInputClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel { InputText.Length: > 0 } vm &&
            TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
        {
            var transfer = new DataTransfer();
            transfer.Add(DataTransferItem.CreateText(vm.InputText));
            await clipboard.SetDataAsync(transfer);
        }
    }

    private void OnRemoveLineBreaksClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel { InputText.Length: > 0 } vm)
            vm.InputText = vm.InputText.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');
    }

    private void OnRemoveSpacesClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel { InputText.Length: > 0 } vm)
            vm.InputText = vm.InputText.Replace(" ", string.Empty);
    }

    private static HistoryWindow? _historyWindow;

    private void OnOpenHistory(object? sender, RoutedEventArgs e)
    {
        if (_historyWindow is { IsVisible: true })
        {
            _historyWindow.Activate();
            return;
        }
        _historyWindow = new HistoryWindow();
        _historyWindow.Show();
    }

    private void OnOpenSettings(object? sender, RoutedEventArgs e)
    {
        if (_settingsWindow is { IsVisible: true })
        {
            _settingsWindow.Activate();
            return;
        }
        _settingsWindow = new SettingsWindow();
        _settingsWindow.Show();
    }

    private SettingsWindow? _settingsWindow;

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            if (DataContext is MainViewModel vm && vm.TranslateCommand.CanExecute(null))
                vm.TranslateCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key is Key.R && e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            // WPF ToggleColorThemeHotkey：循环切换主题
            if (DataContext is MainViewModel vm)
                vm.ToggleTheme();
            e.Handled = true;
        }
    }

    // ── 剪贴板监听常驻（WM_CLIPBOARDUPDATE → 自动翻译）──

    private IntPtr ClipboardWndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        // Win32 消息回调，异常不能逃逸
        try
        {
            if (msg == 0x031D /* WM_CLIPBOARDUPDATE */
                && DataContext is MainViewModel { ClipboardMonitorEnabled: true } vm
                && !vm.IsProcessing)
            {
                var text = WindowsTriggerService.TryReadClipboardText();
                if (!string.IsNullOrWhiteSpace(text) && text != WindowsTriggerService.LastProcessedClipboardText)
                {
                    WindowsTriggerService.LastProcessedClipboardText = text;
                    vm.ShowAndTranslate(text, forceForeground: false);
                }
            }
        }
        catch (Exception e)
        {
            Program.LogError("ClipboardMonitor", e);
        }
        return IntPtr.Zero;
    }

    /// <summary>剪贴板监听开关变化（由 VM 通知）。</summary>
    public void SyncClipboardMonitor(bool enabled)
    {
        if (!OperatingSystem.IsWindows())
            return;
        var hwnd = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (hwnd == IntPtr.Zero)
            return;
        if (enabled)
            Gdi32.AddClipboardFormatListener(hwnd);
        else
            Gdi32.RemoveClipboardFormatListener(hwnd);
    }

    /// <summary>触发系统（输入翻译）入口：聚焦输入框。</summary>
    public void FocusInput() => InputTextBox.Focus();

    /// <summary>当前窗口 DPI 缩放。</summary>
    public double TryGetScaling() => ((TopLevel)this).RenderScaling;

    private async void OnCopyClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel { ResultText.Length: > 0 } vm &&
            TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
        {
            var transfer = new DataTransfer();
            transfer.Add(DataTransferItem.CreateText(vm.ResultText));
            WindowsTriggerService.LastProcessedClipboardText = vm.ResultText;
            await clipboard.SetDataAsync(transfer);
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // 托盘"退出"发起的关闭直接放行；点 X 仅隐藏（托盘常驻）
        if (App.ShuttingDown)
        {
            base.OnClosing(e);
            return;
        }
        e.Cancel = true;
        Hide();
        base.OnClosing(e);
    }
}
