using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Threading;
using STranslate.Avalonia.Services;
using STranslate.Avalonia.Services.Triggers;
using STranslate.Sdk;

namespace STranslate.Avalonia.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private CancellationTokenSource? _cts;
    private WindowsTriggerService? _triggers;
    private bool _suppressAutoTranslate;
    private bool _incDirty;
    private System.Threading.CancellationTokenSource? _autoTranslateDebounce;

    [ObservableProperty] private string _inputText = string.Empty;
    [ObservableProperty] private string _resultText = string.Empty;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private LangEnum _sourceLang = LangEnum.English;
    [ObservableProperty] private LangEnum _targetLang = LangEnum.ChineseSimplified;
    [ObservableProperty] private ExtensionService? _selectedService;
    [ObservableProperty] private ExtensionService? _selectedOcrService;
    [ObservableProperty] private bool _ctrlCcEnabled;
    [ObservableProperty] private bool _mouseSelectionEnabled;
    [ObservableProperty] private bool _isSpeaking;
    [ObservableProperty] private bool _autoStart;
    [ObservableProperty] private bool _isTopmost;
    [ObservableProperty] private bool _autoTranslate;
    [ObservableProperty] private string _theme = "Default";
    [ObservableProperty] private bool _clipboardMonitorEnabled;
    [ObservableProperty] private bool _mouseSelectionIconEnabled;
    [ObservableProperty] private string _incrementalTranslateKey = string.Empty;
    [ObservableProperty] private bool _isCheckingUpdate;
    [ObservableProperty] private string _updateStatusText = string.Empty;
    [ObservableProperty] private string? _lightMessage;

    private DispatcherTimer? _lightTimer;

    /// <summary>Snackbar 式轻提示：3 秒自动消失，新消息重置计时。</summary>
    public void ShowLightMessage(string message)
    {
        global::STranslate.Avalonia.Services.DebugTrace.Log($"light: {message}");
        LightMessage = message;
        _lightTimer ??= new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _lightTimer.Stop();
        _lightTimer.Tick -= OnLightTimerTick;
        _lightTimer.Tick += OnLightTimerTick;
        _lightTimer.Start();
    }

    private void OnLightTimerTick(object? sender, EventArgs e)
    {
        _lightTimer?.Stop();
        LightMessage = null;
    }

    public IReadOnlyList<LangEnum> Languages { get; } = [.. Enum.GetValues<LangEnum>()];
    public IReadOnlyList<ExtensionService> Services { get; }
    public IReadOnlyList<ExtensionService> OcrServices { get; }
    public IReadOnlyList<ExtensionService> TranslateServices { get; }

    public MainViewModel(IReadOnlyList<ExtensionService> services)
    {
        Services = services;
        OcrServices = services.Where(s => s.AsOcr is not null).ToList();
        TranslateServices = services.Where(s => s.AsTranslate is not null).ToList();

        // 从持久化设置初始化（字段直赋，避免触发带副作用的变更回调）
        var s = AppSettings.Current;
        if (Enum.TryParse<LangEnum>(s.SourceLang, out var src))
            _sourceLang = src;
        if (Enum.TryParse<LangEnum>(s.TargetLang, out var tgt))
            _targetLang = tgt;
        _ctrlCcEnabled = s.CtrlCcEnabled;
        _mouseSelectionEnabled = s.MouseSelectionEnabled;
        _autoStart = OperatingSystem.IsWindows() && s.AutoStart;
        _isTopmost = s.IsTopmost;
        _autoTranslate = s.AutoTranslate;
        _theme = s.Theme;
        _clipboardMonitorEnabled = s.ClipboardMonitorEnabled;
        _mouseSelectionIconEnabled = s.MouseSelectionIconEnabled;
        _incrementalTranslateKey = s.IncrementalTranslateKey;
        _selectedService = services.FirstOrDefault(x => x.Id == s.TranslateServiceId && x.AsTranslate is not null)
            ?? services.FirstOrDefault(x => x.AsTranslate is not null);
        _selectedOcrService = services.FirstOrDefault(x => x.Id == s.OcrServiceId && x.AsOcr is not null)
            ?? services.FirstOrDefault(x => x.AsOcr is not null);

        foreach (var svc in Services)
            svc.IsEnabled = !s.DisabledServiceIds.Contains(svc.Id);
    }

    /// <summary>触发系统构建后回接（仅 Windows 非空），并把当前设置值推送给触发系统。</summary>
    public void AttachTriggers(WindowsTriggerService? triggers)
    {
        _triggers = triggers;
        if (triggers is null)
            return;
        triggers.SetCtrlCcEnabled(CtrlCcEnabled);
        triggers.SetMouseSelectionEnabled(MouseSelectionEnabled);
        triggers.SetMouseSelectionIconEnabled(MouseSelectionIconEnabled);
        triggers.HotkeysApplied += conflicts =>
            global::Avalonia.Threading.Dispatcher.UIThread.Post(() => HotkeysApplied?.Invoke(conflicts));
        ClipboardMonitorToggled?.Invoke(ClipboardMonitorEnabled);
    }

    /// <summary>设置页保存：重注册热键。</summary>
    public void ApplyHotkeys() => _triggers?.ApplyHotkeys(AppSettings.Current.Hotkeys);

    /// <summary>热键（重）注册完成事件（已封送到 UI 线程），参数为冲突列表。</summary>
    public event Action<IReadOnlyList<string>>? HotkeysApplied;

    /// <summary>剪贴板监听开关变化（供窗口挂/摘 WM_CLIPBOARDUPDATE 监听）。</summary>
    public event Action<bool>? ClipboardMonitorToggled;

    /// <summary>应用启动完成后调用：以注册表实际状态校正自启开关显示。</summary>
    public void SyncAutoStart()
    {
        if (OperatingSystem.IsWindows())
            AutoStart = AutoStartService.IsEnabled();
    }

    /// <summary>配置导入/恢复后调用：把重读的设置推送到运行态（触发服务/组合框/热键）。</summary>
    public void ApplyRestoredSettings()
    {
        var s = AppSettings.Current;
        if (Enum.TryParse<LangEnum>(s.SourceLang, out var src))
            SourceLang = src;
        if (Enum.TryParse<LangEnum>(s.TargetLang, out var tgt))
            TargetLang = tgt;
        CtrlCcEnabled = s.CtrlCcEnabled;
        MouseSelectionEnabled = s.MouseSelectionEnabled;
        MouseSelectionIconEnabled = s.MouseSelectionIconEnabled;
        ClipboardMonitorEnabled = s.ClipboardMonitorEnabled;
        AutoStart = s.AutoStart;
        IncrementalTranslateKey = s.IncrementalTranslateKey;
        SelectedOcrService = Services.FirstOrDefault(x => x.Id == s.OcrServiceId && x.AsOcr is not null) ?? SelectedOcrService;
        SelectedService = Services.FirstOrDefault(x => x.Id == s.TranslateServiceId && x.AsTranslate is not null) ?? SelectedService;
        Theme = s.Theme;
        IsTopmost = s.IsTopmost;
        AutoTranslate = s.AutoTranslate;
        _triggers?.ApplyHotkeys(s.Hotkeys);
    }

    partial void OnSourceLangChanged(LangEnum value) => Save(p => p.SourceLang = value.ToString());
    partial void OnTargetLangChanged(LangEnum value) => Save(p => p.TargetLang = value.ToString());

    partial void OnCtrlCcEnabledChanged(bool value)
    {
        _triggers?.SetCtrlCcEnabled(value);
        Save(p => p.CtrlCcEnabled = value);
    }

    partial void OnMouseSelectionEnabledChanged(bool value)
    {
        _triggers?.SetMouseSelectionEnabled(value);
        Save(p => p.MouseSelectionEnabled = value);
    }

    partial void OnMouseSelectionIconEnabledChanged(bool value)
    {
        _triggers?.SetMouseSelectionIconEnabled(value);
        Save(p => p.MouseSelectionIconEnabled = value);
    }

    partial void OnAutoStartChanged(bool value)
    {
        if (OperatingSystem.IsWindows())
            AutoStartService.Set(value);
        Save(p => p.AutoStart = value);
    }

    partial void OnIsTopmostChanged(bool value) => Save(p => p.IsTopmost = value);
    partial void OnAutoTranslateChanged(bool value) => Save(p => p.AutoTranslate = value);
    partial void OnThemeChanged(string value) => Save(p => p.Theme = value);

    partial void OnClipboardMonitorEnabledChanged(bool value)
    {
        Save(p => p.ClipboardMonitorEnabled = value);
        ClipboardMonitorToggled?.Invoke(value);
    }

    partial void OnIncrementalTranslateKeyChanged(string value) => Save(p => p.IncrementalTranslateKey = value);

    // null 不落盘：ComboBox 初始化时可能回写 null，落盘会导致重启回退到第一个服务
    partial void OnSelectedServiceChanged(ExtensionService? value)
    {
        if (value is null)
            return;
        Save(p => p.TranslateServiceId = value.Id);
    }

    partial void OnSelectedOcrServiceChanged(ExtensionService? value)
    {
        if (value is null)
            return;
        Save(p => p.OcrServiceId = value.Id);
    }

    private void Save(Action<AppSettings> apply)
    {
        apply(AppSettings.Current);
        AppSettings.Current.Save();
    }

    /// <summary>多服务输出卡片（对齐原版 OutputControl：每启用服务一卡，并发翻译）。</summary>
    public System.Collections.ObjectModel.ObservableCollection<TranslateOutputItem> OutputItems { get; } = [];

    private CancellationTokenSource? _outputCts;

    [RelayCommand]
    private async Task TranslateAsync(CancellationToken cancellationToken)
    {
        // 翻译服务 ∪ 词典服务（词典卡仅在单词查询时出现）
        var dictionaryServices = IsDictionaryQuery(InputText)
            ? Services.Where(s => s.AsDictionary is not null && s.IsEnabled).ToList()
            : [];
        var services = Services
            .Where(s => s.IsEnabled && (s.AsTranslate is not null || s.AsDictionary is not null))
            .DistinctBy(s => s.Id)
            .ToList();
        if (services.Count == 0 || string.IsNullOrWhiteSpace(InputText))
            return;

        _cts?.Cancel();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _cts.Token;

        IsProcessing = true;
        ErrorMessage = null;
        ResultText = string.Empty;

        // 重建输出卡片（保持已有服务卡位置，新增/删除服务同步）
        var existing = OutputItems.ToDictionary(i => i.ServiceId);
        OutputItems.Clear();
        foreach (var svc in services)
        {
            var item = existing.TryGetValue(svc.Id, out var old)
                ? old
                : new TranslateOutputItem { ServiceId = svc.Id, ServiceName = svc.Name, IsExpanded = true };
            item.ResultText = string.Empty;
            item.ErrorMessage = null;
            item.IsProcessing = true;
            item.RetryHandler = RetryServiceAsync;
            if (dictionaryServices.Any(d => d.Id == svc.Id))
                item.Dictionary = new STranslate.Sdk.DictionaryResult();
            OutputItems.Add(item);
        }

        try
        {
            var tasks = OutputItems.Select(item => item.IsDictionary
                ? TranslateDictionaryItemAsync(item, token)
                : TranslateItemAsync(item, token));
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>单个服务卡翻译（TranslateAsync 与卡片重试共用）。</summary>
    private async Task TranslateItemAsync(TranslateOutputItem item, CancellationToken token)
    {
        var service = Services.FirstOrDefault(s => s.Id == item.ServiceId)?.AsTranslate;
        if (service is null)
            return;

        item.IsProcessing = true;
        item.ErrorMessage = null;
        item.ResultText = string.Empty;
        var start = DateTimeOffset.UtcNow;

        try
        {
            var result = await service.TranslateAsync(
                new TranslateRequest(InputText, SourceLang, TargetLang),
                null, token);
            item.ResultText = result.Text;
            item.ErrorMessage = result.IsSuccess ? null : result.ErrorMessage;
            item.Duration = DateTimeOffset.UtcNow - start;
            item.IsExpanded = !string.IsNullOrEmpty(result.Text);

            // 第一个成功结果同步到 ResultText 并写历史（历史/剪贴板监听兼容）
            if (result.IsSuccess && string.IsNullOrEmpty(ResultText))
            {
                ResultText = result.Text;
                try
                {
                    App.Services.GetRequiredService<HistoryService>().InsertOrUpdate(new HistoryItem
                    {
                        Time = DateTime.Now,
                        SourceLang = SourceLang.ToString(),
                        TargetLang = TargetLang.ToString(),
                        SourceText = InputText,
                        ResultText = result.Text
                    });
                }
                catch (Exception he)
                {
                    Program.LogError("History.Write", he);
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            item.ErrorMessage = e.Message;
        }
        finally
        {
            item.IsProcessing = false;
        }
    }

    /// <summary>单词判定：无内部空白且长度合理（词典查询触发条件）。</summary>
    private static bool IsDictionaryQuery(string text)
    {
        var t = text.Trim();
        return t.Length > 0 && t.Length <= 40 && !t.Contains(' ');
    }

    /// <summary>词典卡查询（IDictionaryPlugin 结构化结果）。</summary>
    private async Task TranslateDictionaryItemAsync(TranslateOutputItem item, CancellationToken token)
    {
        var plugin = Services.FirstOrDefault(s => s.Id == item.ServiceId)?.AsDictionary;
        if (plugin is null)
            return;

        item.IsProcessing = true;
        item.ErrorMessage = null;
        var start = DateTimeOffset.UtcNow;

        try
        {
            var result = await plugin.TranslateAsync(InputText.Trim(), token);
            item.Duration = DateTimeOffset.UtcNow - start;
            item.ErrorMessage = result.ResultType == DictionaryResultType.Success ? null
                : result.ResultType == DictionaryResultType.NoResult ? "未查询到该词" : (result.Text ?? "查询失败");
            if (result.ResultType == DictionaryResultType.Success)
            {
                item.Dictionary = result;
                item.RefreshDictionaryNotifiers();
                item.IsExpanded = true;
                // 复制/朗读的数据源（对齐原版：词头+音标+释义全文）
                var phonetics = string.Join("  ", result.Symbols
                    .Where(s => !string.IsNullOrEmpty(s.Phonetic))
                    .Select(s => $"{s.Label} [{s.Phonetic}]"));
                item.ResultText = string.Join(Environment.NewLine,
                    new[] { result.Text, phonetics }
                    .Concat(result.DictMeans.Select(m => $"{m.PartOfSpeech} {m.MeansText}"))
                    .Where(x => !string.IsNullOrWhiteSpace(x)));
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            item.ErrorMessage = e.Message;
        }
        finally
        {
            item.IsProcessing = false;
        }
    }

    /// <summary>输出卡片重试入口。</summary>
    private async Task RetryServiceAsync(TranslateOutputItem item, CancellationToken token)
    {
        try
        {
            if (item.IsDictionary)
                await TranslateDictionaryItemAsync(item, token);
            else
                await TranslateItemAsync(item, token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>供输出卡片朗读调用（TTS 参数化入口）。</summary>
    public Task SpeakTextPublicAsync(string text) => SpeakTextAsync(text);

    /// <summary>触发系统入口：置前主窗、填充文本并自动翻译。</summary>
    public void ShowAndTranslate(string text, bool forceForeground)
    {
        App.SetMainWindowVisible(true, forceForeground);
        _suppressAutoTranslate = true;
        try
        {
            InputText = text;
        }
        finally
        {
            _suppressAutoTranslate = false;
        }
        if (TranslateCommand.CanExecute(null))
            TranslateCommand.Execute(null);
    }

    /// <summary>截图翻译入口：OCR 识别截图 → 文本进主翻译链路。</summary>
    public async Task HandleScreenshotAsync(byte[] pngBytes, int pixelWidth, int pixelHeight, IOcrPlugin ocr)
    {
        App.SetMainWindowVisible(true);
        IsProcessing = true;
        ErrorMessage = null;

        try
        {
            var ocrResult = await ocr.RecognizeAsync(
                new OcrRequest(pngBytes, "image/png", SourceLang, pixelWidth, pixelHeight));
            var text = ocrResult.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                ErrorMessage = ocrResult.IsSuccess ? "OCR 未识别到文字" : ocrResult.ErrorMessage;
                return;
            }

            _suppressAutoTranslate = true;
            try
            {
                InputText = text;
            }
            finally
            {
                _suppressAutoTranslate = false;
            }
            if (TranslateCommand.CanExecute(null))
                await TranslateCommand.ExecuteAsync(null);
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

    /// <summary>输入翻译入口（WPF InputClear 语义）：显示主窗、清空输入并聚焦。</summary>
    public void InputClear()
    {
        App.SetMainWindowVisible(true);
        _suppressAutoTranslate = true;
        try
        {
            InputText = string.Empty;
        }
        finally
        {
            _suppressAutoTranslate = false;
        }
        ResultText = string.Empty;
        ErrorMessage = null;
        App.FocusInput();
    }

    /// <summary>图片翻译入口：OCR 坐标块 → 同行合并 → 并行翻译 → 原位贴图覆盖窗口。</summary>
    public async Task HandleImageTranslateAsync(
        STranslate.Avalonia.Views.ScreenshotResult shot,
        IOcrPlugin ocr,
        ITranslatePlugin? translate)
    {
        try
        {
            var items = await ImageTranslateCore(shot, ocr, translate);
            if (items is null)
                return;

            Views.ImageTranslateWindow.ShowOverlay(
                shot.PngBytes, shot.ScreenOrigin, shot.PixelSize, shot.Scaling, items,
                onRerun: () => _ = HandleImageTranslateAsync(shot, ocr, translate),
                onRescreenshot: () => _triggers?.BeginImageTranslate());
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }

    /// <summary>图片翻译核心：OCR → 同行贪心合并 → 并行翻译 → 覆盖项。</summary>
    private async Task<List<Views.ImageTranslateItem>?> ImageTranslateCore(
        STranslate.Avalonia.Views.ScreenshotResult shot,
        IOcrPlugin ocr,
        ITranslatePlugin? translate)
    {
        var ocrResult = await ocr.RecognizeAsync(
            new OcrRequest(shot.PngBytes, "image/png", SourceLang, shot.PixelSize.Width, shot.PixelSize.Height));
        var contents = ocrResult.OcrContents
            .Where(c => !string.IsNullOrWhiteSpace(c.Text) && c.BoxPoints.Count > 0)
            .Select(c =>
            {
                var x = c.BoxPoints.Min(p => p.X);
                var y = c.BoxPoints.Min(p => p.Y);
                return (
                    X: x, Y: y,
                    W: c.BoxPoints.Max(p => p.X) - x,
                    H: c.BoxPoints.Max(p => p.Y) - y,
                    Text: c.Text);
            })
            .Where(b => b.W > 0 && b.H > 0)
            .OrderBy(b => b.Y)
            .ThenBy(b => b.X)
            .ToList();

        if (contents.Count == 0)
        {
            ErrorMessage = ocrResult.IsSuccess ? "图片翻译：OCR 未识别到文字" : ocrResult.ErrorMessage;
            return null;
        }
        if (translate is null)
        {
            ErrorMessage = "图片翻译：没有可用的翻译扩展";
            return null;
        }

        // 合并模式（设置→通用）：smart=智能行合并（0.6 行高阈值）；strict=逐块直翻
        var mergeMode = AppSettings.Current.ImageTranslateMergeMode;
        var lines = new List<List<(float X, float Y, float W, float H, string Text)>>();
        if (mergeMode == "strict")
        {
            foreach (var b in contents)
                lines.Add([b]);
        }
        else
        {
            foreach (var b in contents)
            {
                var cy = b.Y + b.H / 2.0;
                var line = lines.FirstOrDefault(l =>
                {
                    var ly = l.Min(x => x.Y);
                    var lh = l.Max(x => x.Y + x.H) - ly;
                    var lcy = ly + lh / 2.0;
                    return Math.Abs(cy - lcy) < Math.Max(b.H, lh) * 0.6;
                });
                if (line is null)
                    lines.Add([b]);
                else
                    line.Add(b);
            }
        }

        var scaling = shot.Scaling;
        var translateTasks = new List<Task<Views.ImageTranslateItem>>();
        foreach (var line in lines)
        {
            var ordered = line.OrderBy(b => b.X).ToList();
            var x = ordered.Min(b => b.X);
            var y = ordered.Min(b => b.Y);
            var w = ordered.Max(b => b.X + b.W) - x;
            var h = ordered.Max(b => b.Y + b.H) - y;
            var text = string.Join(' ', ordered.Select(b => b.Text));
            translateTasks.Add(Task.Run(async () =>
            {
                var translated = await translate.TranslateAsync(
                    new TranslateRequest(text, SourceLang, TargetLang), null, CancellationToken.None);
                return new Views.ImageTranslateItem(
                    x / scaling, y / scaling, w / scaling, h / scaling,
                    translated.IsSuccess ? translated.Text : text);
            }));
        }
        var items = await Task.WhenAll(translateTasks);
        return [.. items];
    }

    partial void OnInputTextChanged(string value)
    {
        if (!AutoTranslate || _suppressAutoTranslate || string.IsNullOrWhiteSpace(value))
            return;
        _autoTranslateDebounce?.Cancel();
        _autoTranslateDebounce = new System.Threading.CancellationTokenSource();
        var debounceToken = _autoTranslateDebounce.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(600, debounceToken);
                global::Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    if (!string.IsNullOrWhiteSpace(InputText) && TranslateCommand.CanExecute(null))
                        TranslateCommand.Execute(null);
                });
            }
            catch (OperationCanceledException)
            {
            }
        });
    }

    /// <summary>增量翻译会话开始：清空输入框准备累积（对齐 WPF IncrementalClearInput 默认行为）。</summary>
    public void BeginIncrementalSession()
    {
        _suppressAutoTranslate = true;
        try
        {
            InputText = string.Empty;
        }
        finally
        {
            _suppressAutoTranslate = false;
        }
        _incDirty = false;
    }

    /// <summary>增量翻译会话：追加一段选中文本（会话开始时输入框已清空）。</summary>
    public void AppendIncrementalText(string text)
    {
        _suppressAutoTranslate = true;
        try
        {
            InputText = string.IsNullOrEmpty(InputText) ? text : InputText + Environment.NewLine + text;
            _incDirty = true;
        }
        finally
        {
            _suppressAutoTranslate = false;
        }
    }

    /// <summary>增量翻译会话结束（触发键松开）：有新内容则翻译。</summary>
    public void EndIncrementalSession()
    {
        if (!_incDirty || string.IsNullOrWhiteSpace(InputText))
            return;
        _incDirty = false;
        if (TranslateCommand.CanExecute(null))
            TranslateCommand.Execute(null);
    }

    /// <summary>朗读译文（TTS 扩展合成 + 宿主播放）。</summary>
    [RelayCommand]
    private async Task SpeakAsync()
    {
        await SpeakTextAsync(ResultText);
    }

    /// <summary>朗读输入文本。</summary>
    [RelayCommand]
    private async Task SpeakInputAsync()
    {
        await SpeakTextAsync(InputText);
    }

    private async Task SpeakTextAsync(string text)
    {
        if (IsSpeaking || string.IsNullOrWhiteSpace(text))
            return;
        var tts = App.ResolveExtension(s => s.AsTts, null);
        if (tts is null)
        {
            ErrorMessage = "没有可用的 TTS 扩展";
            return;
        }

        IsSpeaking = true;
        ErrorMessage = null;
        try
        {
            var audio = await tts.SpeakAsync(text);
            await Task.Run(() => App.Services.GetRequiredService<HostAudioPlayer>().Play(audio));
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
        finally
        {
            IsSpeaking = false;
        }
    }

    /// <summary>移除译文中的换行符。</summary>
    [RelayCommand]
    private void RemoveLineBreaks()
    {
        if (string.IsNullOrEmpty(ResultText))
            return;
        ResultText = ResultText.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');
    }

    /// <summary>移除译文中的空格。</summary>
    [RelayCommand]
    private void RemoveSpaces()
    {
        if (string.IsNullOrEmpty(ResultText))
            return;
        ResultText = ResultText.Replace(" ", string.Empty);
    }

    /// <summary>把源文本加入生词本。</summary>
    [RelayCommand]
    private async Task SaveVocabAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText))
            return;
        var vocab = App.ResolveExtension(s => s.AsVocabulary, null);
        if (vocab is null)
        {
            ErrorMessage = "没有可用的生词本扩展";
            return;
        }

        var result = await vocab.SaveAsync(InputText);
        global::STranslate.Avalonia.Services.DebugTrace.Log($"vocab: success={result.IsSuccess} err={result.ErrorMessage}");
        if (!result.IsSuccess)
            ErrorMessage = result.ErrorMessage;
        else
            ShowLightMessage("已加入生词本");
    }

    /// <summary>检查更新（Velopack）。</summary>
    [RelayCommand]
    private async Task CheckUpdateAsync()
    {
        if (IsCheckingUpdate)
            return;
        IsCheckingUpdate = true;
        try
        {
            var progress = new Progress<string>(text => UpdateStatusText = text);
            UpdateStatusText = await App.Services.GetRequiredService<UpdaterService>()
                .CheckAndApplyAsync(progress);
        }
        catch (Exception e)
        {
            UpdateStatusText = "更新失败：" + e.Message;
        }
        finally
        {
            IsCheckingUpdate = false;
        }
    }

    /// <summary>主题循环切换 Default → Light → Dark。</summary>
    public void ToggleTheme()
    {
        Theme = Theme switch
        {
            "Light" => "Dark",
            "Dark" => "Default",
            _ => "Light"
        };
    }

    /// <summary>语言对互换。</summary>
    [RelayCommand]
    private void SwapLanguages()
    {
        (SourceLang, TargetLang) = (TargetLang, SourceLang);
    }
}
