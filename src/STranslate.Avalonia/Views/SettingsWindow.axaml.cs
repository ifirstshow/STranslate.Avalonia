using System.IO.Compression;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using STranslate.Avalonia.Services;
using STranslate.Core;
using STranslate.Avalonia.ViewModels;
using STranslate.Sdk;

namespace STranslate.Avalonia.Views;

public partial class SettingsWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly ExtensionSettingsStore _settingsStore;

    public SettingsWindow()
    {
        _viewModel = App.Services.GetRequiredService<MainViewModel>();
        _settingsStore = App.Services.GetRequiredService<ExtensionSettingsStore>();
        InitializeComponent();
        DataContext = _viewModel;
        HotkeysPanel.DataContext = AppSettings.Current.Hotkeys;
        _viewModel.HotkeysApplied += ShowHotkeyStatus;
        Closed += (_, _) => _viewModel.HotkeysApplied -= ShowHotkeyStatus;
        BuildExtensionSettingsForms();
        ShowDataLocation();
        InitCombos();
        ReloadSpkgList();
        InitNetworkPage();
        IncKeyBox.Text = _viewModel.IncrementalTranslateKey;
        var b = AppSettings.Current.Backup;
        WebDavAddress.Text = b.Address;
        WebDavUser.Text = b.Username;
        WebDavPass.Text = b.Password;
    }

    // ── Combo 初始化（代码后台选择，绕开 ItemsSource 重置时的首项回写）──

    private bool _comboSyncing = true;

    private void InitCombos()
    {
        OcrServiceCombo.ItemsSource = _viewModel.OcrServices;
        SourceLangCombo.ItemsSource = _viewModel.Languages;
        TargetLangCombo.ItemsSource = _viewModel.Languages;

        OcrServiceCombo.SelectedItem = _viewModel.SelectedOcrService;
        SourceLangCombo.SelectedItem = _viewModel.SourceLang;
        TargetLangCombo.SelectedItem = _viewModel.TargetLang;
        _comboSyncing = false;
    }

    private void OnOcrServiceChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_comboSyncing)
            return;
        _viewModel.SelectedOcrService = OcrServiceCombo.SelectedItem as ExtensionService;
    }

    private void OnSourceLangChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_comboSyncing)
            return;
        if (SourceLangCombo.SelectedItem is LangEnum lang)
            _viewModel.SourceLang = lang;
    }

    private void OnTargetLangChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_comboSyncing)
            return;
        if (TargetLangCombo.SelectedItem is LangEnum lang)
            _viewModel.TargetLang = lang;
    }

    // ── WebDav 云备份 ──

    private void ReadWebDavFieldsInto(BackupSettings backup)
    {
        backup.Address = WebDavAddress.Text.Trim();
        backup.Username = WebDavUser.Text.Trim();
        backup.Password = WebDavPass.Text;
    }

    private async void OnWebDavUpload(object? sender, RoutedEventArgs e)
    {
        ReadWebDavFieldsInto(AppSettings.Current.Backup);
        AppSettings.Current.Save();
        if (string.IsNullOrWhiteSpace(AppSettings.Current.Backup.Address))
        {
            WebDavStatusText.Text = "请先填写服务地址";
            return;
        }
        WebDavStatusText.Text = "上传中…";
        var error = await App.Services.GetRequiredService<WebDavBackupService>()
            .UploadAsync(AppSettings.Current.Backup);
        WebDavStatusText.Text = error ?? "上传成功";
    }

    private async void OnWebDavRestore(object? sender, RoutedEventArgs e)
    {
        ReadWebDavFieldsInto(AppSettings.Current.Backup);
        AppSettings.Current.Save();
        if (string.IsNullOrWhiteSpace(AppSettings.Current.Backup.Address))
        {
            WebDavStatusText.Text = "请先填写服务地址";
            return;
        }
        WebDavStatusText.Text = "恢复中…";
        try
        {
            var error = await App.Services.GetRequiredService<WebDavBackupService>()
                .DownloadAndRestoreAsync(AppSettings.Current.Backup);
            if (error is null)
            {
                AppSettings.LoadIntoCurrent();
                _viewModel.ApplyRestoredSettings();
                RefreshAllFromVm();
                WebDavStatusText.Text = "恢复成功，已即时生效";
            }
            else
            {
                WebDavStatusText.Text = error;
            }
        }
        catch (Exception ex)
        {
            WebDavStatusText.Text = "恢复失败：" + ex.Message;
        }
    }

    /// <summary>从 VM 回读刷新本窗口全部控件（导入/恢复后调用）。</summary>
    private void RefreshAllFromVm()
    {
        _comboSyncing = true;
        OcrServiceCombo.SelectedItem = _viewModel.SelectedOcrService;
        SourceLangCombo.SelectedItem = _viewModel.SourceLang;
        TargetLangCombo.SelectedItem = _viewModel.TargetLang;
        IncKeyBox.Text = _viewModel.IncrementalTranslateKey;
        // 热键框绑定的是 HotkeyEntry（无 INPC 的 CLR 属性），必须显式回读
        var hk = AppSettings.Current.Hotkeys;
        HkOpenWindow.Text = hk.OpenWindow;
        HkInputTranslate.Text = hk.InputTranslate;
        HkCrosswordTranslate.Text = hk.CrosswordTranslate;
        HkScreenshotTranslate.Text = hk.ScreenshotTranslate;
        HkSilentOcr.Text = hk.SilentOcr;
        HkSilentTts.Text = hk.SilentTts;
        HkImageTranslate.Text = hk.ImageTranslate;
        _comboSyncing = false;
        var b = AppSettings.Current.Backup;
        WebDavAddress.Text = b.Address;
        WebDavUser.Text = b.Username;
        WebDavPass.Text = b.Password;
    }

    // ── 热键录制（点击框 → 按组合键 → 自动写入并应用）──

    private TextBox? _recordingBox;

    private void OnHotkeyGotFocus(object? sender, FocusChangedEventArgs e)
    {
        if (sender is TextBox box)
        {
            _recordingBox = box;
            box.Text = "按下组合键…（Esc 取消）";
        }
    }

    private void OnHotkeyKeyDown(object? sender, KeyEventArgs e)
    {
        if (_recordingBox is null || sender is not TextBox box)
            return;

        e.Handled = true;

        // Esc 取消：恢复原值
        if (e.Key == Key.Escape)
        {
            box.Text = ReadHotkeyForBox(box);
            _recordingBox = null;
            return;
        }

        // 修饰键单独按下不算完成
        if (e.Key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin
           )
            return;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        var mods = e.KeyModifiers;

        var parts = new List<string>();
        if (mods.HasFlag(KeyModifiers.Control)) parts.Add("Ctrl");
        if (mods.HasFlag(KeyModifiers.Alt)) parts.Add("Alt");
        if (mods.HasFlag(KeyModifiers.Shift)) parts.Add("Shift");
        if (mods.HasFlag(KeyModifiers.Meta)) parts.Add("Win");

        // 纯主键（无修饰）仅接受 F1-F24，避免劫持普通输入
        if (parts.Count == 0 && !(key is Key.F1 or Key.F2 or Key.F3 or Key.F4 or Key.F5 or Key.F6
            or Key.F7 or Key.F8 or Key.F9 or Key.F10 or Key.F11 or Key.F12
            or Key.F13 or Key.F14 or Key.F15 or Key.F16 or Key.F17 or Key.F18
            or Key.F19 or Key.F20 or Key.F21 or Key.F22 or Key.F23 or Key.F24))
            return;

        parts.Add(key.ToString());
        box.Text = HotkeyCodec.Normalize(string.Join('+', parts));
        _recordingBox = null;

        // 录制完自动应用（对齐原版即录即生效）
        OnSaveHotkeys(sender, new RoutedEventArgs());
    }

    private string ReadHotkeyForBox(TextBox box) => box.Name switch
    {
        var n when n == HkOpenWindow.Name => AppSettings.Current.Hotkeys.OpenWindow,
        var n when n == HkInputTranslate.Name => AppSettings.Current.Hotkeys.InputTranslate,
        var n when n == HkCrosswordTranslate.Name => AppSettings.Current.Hotkeys.CrosswordTranslate,
        var n when n == HkScreenshotTranslate.Name => AppSettings.Current.Hotkeys.ScreenshotTranslate,
        var n when n == HkSilentOcr.Name => AppSettings.Current.Hotkeys.SilentOcr,
        var n when n == HkSilentTts.Name => AppSettings.Current.Hotkeys.SilentTts,
        var n when n == HkImageTranslate.Name => AppSettings.Current.Hotkeys.ImageTranslate,
        _ => string.Empty
    };

    private void OnSaveIncrementalKey()
    {
        // 增量键与热键同管：存设置 + 应用（重解析 _incVk）
        _viewModel.IncrementalTranslateKey = HotkeyCodec.Normalize(IncKeyBox.Text.Trim());
    }

    private void ShowDataLocation()
    {
        DataDirText.Text = DataLocation.DataDirectory();
        PortableText.Text = DataLocation.PortableDataLocationInUse()
            ? "当前为便携模式（exe 旁存在 PortableConfig 文件夹）"
            : "当前为常规模式；在 exe 旁创建 PortableConfig 文件夹可切换为便携模式";
    }

    // ── 网络代理 ──

    private void InitNetworkPage()
    {
        var proxy = AppSettings.Current.Proxy;
        ProxyEnabledCheck.IsChecked = proxy.IsEnabled;
        ProxyCustomPanel.IsVisible = proxy.IsEnabled;
        ProxyEnabledCheck.IsCheckedChanged += (_, _) =>
        {
            proxy.IsEnabled = ProxyEnabledCheck.IsChecked == true;
            ProxyCustomPanel.IsVisible = proxy.IsEnabled;
            AppSettings.Current.Save();
            App.Services.GetRequiredService<HttpService>().ReloadProxy();
            ProxyStatusText.Text = "已生效";
        };
        ProxyTypeCombo.SelectedIndex = 0; // v1 仅自定义 HTTP 代理
        ProxyAddress.Text = proxy.ProxyAddress;
        ProxyPort.Text = proxy.ProxyPort.ToString();
    }

    private void SaveProxyFromUi()
    {
        var proxy = AppSettings.Current.Proxy;
        proxy.IsEnabled = ProxyEnabledCheck.IsChecked == true;
        proxy.ProxyType = ProxyType.Http;
        proxy.ProxyAddress = ProxyAddress.Text.Trim();
        _ = int.TryParse(ProxyPort.Text.Trim(), out var port);
        proxy.ProxyPort = port;
        AppSettings.Current.Save();
        App.Services.GetRequiredService<HttpService>().ReloadProxy();
    }

    private async void OnTestProxyClick(object? sender, RoutedEventArgs e)
    {
        SaveProxyFromUi();
        ProxyStatusText.Text = "测试中…";
        var http = App.Services.GetRequiredService<HttpService>();
        var ok = await http.TestProxyAsync();
        ProxyStatusText.Text = ok ? $"连通（出口 IP: {http.LastTestIp}）" : "连接失败";
    }

    // ── 社区插件（.spkg，ADR-0004 worker 加载）──

    private readonly PluginPackageManager _spkgManager = new();

    private void ReloadSpkgList()
    {
        SpkgList.ItemsSource = _spkgManager.ListInstalled()
            .Select(p => p.DirectoryName)
            .ToList();
    }

    private async void OnInstallSpkgClick(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "安装社区插件",
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("社区插件包") { Patterns = ["*.spkg", "*.zip"] }]
        });
        if (files is not [{ } file])
            return;

        var error = _spkgManager.Install(file.Path.LocalPath);
        ExtensionStatusText.Text = error is null
            ? $"已安装 {file.Name}，重启程序后生效。"
            : "安装失败：" + error;
        ReloadSpkgList();
        InitNetworkPage();
    }

    private void OnUninstallSpkgClick(object? sender, RoutedEventArgs e)
    {
        if (SpkgList.SelectedItem is not string name)
        {
            ExtensionStatusText.Text = "先在列表中选择要卸载的插件。";
            return;
        }
        // 先停 worker 释放 dll 文件锁，再删目录
        App.Services.GetRequiredService<ExtensionHost>().StopWorker();
        DebugTrace.Log($"spkg: uninstalling {name}");
        var error = _spkgManager.Uninstall(name);
        DebugTrace.Log($"spkg: uninstall result={error ?? "ok"}");
        ExtensionStatusText.Text = error is null
            ? $"已卸载 {name}，重启程序后生效。"
            : "卸载失败：" + error;
        ReloadSpkgList();
        InitNetworkPage();
    }

    // ── 配置备份：导出/导入 settings.json + extension-settings.json + history.db ──

    private static readonly string DataDir = Path.Combine(DataLocation.DataDirectory(), "Avalonia");
    private static readonly string[] BackupFiles = ["settings.json", "extension-settings.json", "history.db"];

    private async void OnExportConfig(object? sender, RoutedEventArgs e)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "导出配置",
            SuggestedFileName = $"stranslate-config-{DateTime.Now:yyyyMMdd-HHmmss}.zip",
            DefaultExtension = "zip"
        });
        if (file is null)
            return;

        try
        {
            // FileMode.Create 截断旧文件：OpenOrCreate 在覆盖更长旧文件时会留下尾部垃圾
            await using var stream = new FileStream(file.Path.LocalPath, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var zip = new ZipArchive(stream, ZipArchiveMode.Create);
            foreach (var name in BackupFiles)
            {
                var path = Path.Combine(DataDir, name);
                if (!File.Exists(path))
                    continue;
                var entry = zip.CreateEntry(name);
                await using var source = File.OpenRead(path);
                await using var entryStream = entry.Open();
                await source.CopyToAsync(entryStream);
            }
            DataStatusText.Text = "已导出";
        }
        catch (Exception ex)
        {
            DataStatusText.Text = "导出失败：" + ex.Message;
        }
    }

    private async void OnImportConfig(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "导入配置",
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("配置备份") { Patterns = ["*.zip"] }]
        });
        if (files is not [{ } file])
        {
            DebugTrace.Log("import: no file picked");
            return;
        }
        DebugTrace.Log($"import: file={file.Name}");

        try
        {
            await using var stream = await file.OpenReadAsync();
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
            DebugTrace.Log($"import: entries={zip.Entries.Count} [{string.Join(',', zip.Entries.Select(x => x.Name))}]");
            var restored = 0;
            foreach (var entry in zip.Entries)
            {
                if (!BackupFiles.Contains(entry.Name))
                    continue;
                var target = Path.Combine(DataDir, entry.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                await using var source = entry.Open();
                await using var output = File.Create(target);
                await source.CopyToAsync(output);
                restored++;
            }
            DebugTrace.Log($"import restored={restored}");
            if (restored > 0)
            {
                AppSettings.LoadIntoCurrent();
#if DEBUG
                DebugTrace.Log($"import restored={restored} file: cc={AppSettings.Current.CtrlCcEnabled} mouse={AppSettings.Current.MouseSelectionEnabled} lang={AppSettings.Current.SourceLang}");
                DebugTrace.Log($"import vm: cc={_viewModel.CtrlCcEnabled} mouse={_viewModel.MouseSelectionEnabled} lang={_viewModel.SourceLang}");
#endif
                _viewModel.ApplyRestoredSettings();
                RefreshAllFromVm();
                DataStatusText.Text = $"已导入 {restored} 项并即时生效";
            }
            else
            {
                DataStatusText.Text = "备份中没有可识别的配置文件";
            }
        }
        catch (Exception ex)
        {
            DataStatusText.Text = "导入失败：" + ex.Message;
        }
    }

    private void OnSaveHotkeys(object? sender, RoutedEventArgs e)
    {
        ReadWebDavFieldsInto(AppSettings.Current.Backup);
        // 规范化输入（"alt+g" → "Alt+G"）
        var h = AppSettings.Current.Hotkeys;
        h.OpenWindow = HotkeyCodec.Normalize(h.OpenWindow);
        h.InputTranslate = HotkeyCodec.Normalize(h.InputTranslate);
        h.CrosswordTranslate = HotkeyCodec.Normalize(h.CrosswordTranslate);
        h.ScreenshotTranslate = HotkeyCodec.Normalize(h.ScreenshotTranslate);
        h.SilentOcr = HotkeyCodec.Normalize(h.SilentOcr);
        h.ImageTranslate = HotkeyCodec.Normalize(h.ImageTranslate);
        AppSettings.Current.Save();

        OnSaveIncrementalKey();
        _viewModel.ApplyHotkeys();
    }

    /// <summary>由 MainViewModel 订阅转发（避免窗口重复订阅热键事件）。</summary>
    public void ShowHotkeyStatus(IReadOnlyList<string> conflicts) =>
        StatusText.Text = conflicts.Count == 0 ? "热键已生效" : $"热键冲突：{string.Join("、", conflicts)}";

    // ── 扩展设置动态表单（按 SettingItems 元数据生成，值即改即存） ──

    private void BuildExtensionSettingsForms()
    {
        foreach (var svc in _viewModel.Services)
        {
            ServiceCardsPanel.Children.Add(BuildServiceCard(svc));
        }
    }

    /// <summary>单张服务卡：启停开关 + 服务名 + 内嵌设置表单（对齐原版 ServicePanel）。</summary>
    private Border BuildServiceCard(ExtensionService svc)
    {
        var card = new Border
        {
            Background = new global::Avalonia.Media.SolidColorBrush(global::Avalonia.Media.Color.Parse("#14808080")),
            BorderBrush = new global::Avalonia.Media.SolidColorBrush(global::Avalonia.Media.Color.Parse("#22808080")),
            BorderThickness = new global::Avalonia.Thickness(1),
            CornerRadius = new global::Avalonia.CornerRadius(6),
            Padding = new global::Avalonia.Thickness(10, 6, 10, 8)
        };
        var body = new StackPanel { Spacing = 8 };

        // 卡头：启停开关 + 服务名
        var header = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("Auto,Auto,*") };
        var toggle = new ToggleSwitch { IsChecked = svc.IsEnabled, Padding = new global::Avalonia.Thickness(2, 0), MinWidth = 0 };
        toggle.IsCheckedChanged += (_, _) =>
        {
            svc.IsEnabled = toggle.IsChecked == true;
            var disabled = AppSettings.Current.DisabledServiceIds;
            if (svc.IsEnabled)
                disabled.Remove(svc.Id);
            else if (!disabled.Contains(svc.Id))
                disabled.Add(svc.Id);
            AppSettings.Current.Save();
        };
        header.Children.Add(toggle);
        var nameText = new TextBlock
        {
            Text = svc.Name,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(nameText, 1);
        header.Children.Add(nameText);
        body.Children.Add(header);

        // 内嵌设置表单
        var section = new StackPanel { Spacing = 8 };
        foreach (var item in svc.Plugin.SettingItems)
        {
            var control = BuildItemControl(svc.Id, item);
            if (control is null)
            {
                section.Children.Add(new TextBlock
                {
                    Text = $"{item.Label}：该设置项类型暂不支持编辑",
                    Opacity = 0.5,
                    FontSize = 12
                });
                continue;
            }

            var row = new Grid
            {
                ColumnDefinitions = ColumnDefinitions.Parse("180,*"),
                ColumnSpacing = 8
            };
            row.Children.Add(new TextBlock { Text = item.Label, VerticalAlignment = VerticalAlignment.Center });
            Grid.SetColumn(control, 1);
            row.Children.Add(control);
            section.Children.Add(row);
        }
        if (svc.Plugin.SettingItems.Count > 0)
            body.Children.Add(section);
        card.Child = body;
        return card;
    }

    private Control? BuildItemControl(string extensionId, SettingItem item)
    {
        var raw = _settingsStore.GetRaw(extensionId);
        raw.TryGetValue(item.Id, out var current);

        switch (item)
        {
            case TextSetting t:
            {
                var box = new TextBox
                {
                    PlaceholderText = t.Placeholder,
                    Text = current.ValueKind == JsonValueKind.String ? current.GetString() : t.Default
                };
                BindSave(extensionId, item, box, () => (object?)box.Text, v => (object?)v);
                return box;
            }
            case PasswordSetting p:
            {
                var box = new TextBox { PasswordChar = '•', PlaceholderText = p.Placeholder };
                if (current.ValueKind == JsonValueKind.String)
                    box.Text = current.GetString();
                BindSave(extensionId, item, box, () => (object?)box.Text, v => (object?)v);
                return box;
            }
            case NumberSetting n:
            {
                var box = new NumericUpDown
                {
                    Minimum = (decimal)n.Min,
                    Maximum = (decimal)n.Max,
                    Increment = (decimal)n.Step
                };
                if (current.ValueKind == JsonValueKind.Number)
                    box.Value = (decimal)current.GetDouble();
                else if (n.Default is { } d)
                    box.Value = (decimal)d;
                BindSave(extensionId, item, box, () => (object)(box.Value ?? (decimal)(n.Default ?? 0)), v => (object?)Convert.ToDouble(v));
                return box;
            }
            case ToggleSetting t:
            {
                var check = new CheckBox { IsChecked = current.ValueKind == JsonValueKind.True || (current.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null && t.Default) };
                BindSave(extensionId, item, check, () => (object?)(check.IsChecked == true), v => (object?)v);
                return check;
            }
            case SelectSetting s:
            {
                var combo = new ComboBox { ItemsSource = s.Options };
                var selected = s.Options.FirstOrDefault(o =>
                    current.ValueKind == JsonValueKind.String && o.Value == current.GetString());
                if (selected is not null)
                    combo.SelectedItem = selected;
                else
                    combo.SelectedItem = s.Options.FirstOrDefault(o => o.Value == s.Default) ?? s.Options.FirstOrDefault();
                BindSave(extensionId, item, combo, () => (object?)((combo.SelectedItem as SelectOption)?.Value ?? ""), v => (object?)v);
                return combo;
            }
            default:
                return null;
        }
    }

    private void BindSave(string extensionId, SettingItem item, Control control,
        Func<object?> getValue, Func<object, object> toElement)
    {
        control.LostFocus += (_, _) => SaveValue(extensionId, item.Id, getValue(), toElement);
        if (control is ComboBox combo)
            combo.SelectionChanged += (_, _) => SaveValue(extensionId, item.Id, getValue(), toElement);
        if (control is CheckBox check)
            check.Click += (_, _) => SaveValue(extensionId, item.Id, getValue(), toElement);
    }

    private void SaveValue(string extensionId, string itemId, object? value, Func<object, object> toElement)
    {
        var element = JsonSerializer.SerializeToElement(toElement(value!));
        _settingsStore.Update(extensionId, itemId, element);
        ExtensionStatusText.Text = "已保存";
    }
    private void OnOpenRepoClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                "https://github.com/ZGGSONG/STranslate") { UseShellExecute = true });
        }
        catch { /* 浏览器打开失败不影响主流程 */ }
    }
}
