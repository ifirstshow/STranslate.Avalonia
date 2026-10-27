using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using STranslate.Avalonia.Services;
using STranslate.Avalonia.Services.Triggers;

namespace STranslate.Avalonia.ViewModels;

/// <summary>单个翻译服务的输出卡片（原版 OutputControl 每服务一卡的等价物）。</summary>
public partial class TranslateOutputItem : ObservableObject
{
    [ObservableProperty] private string _resultText = string.Empty;
    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private TimeSpan _duration;

    public bool HasDuration => Duration.TotalSeconds >= 0.1;
    public string DurationText => $"{Duration.TotalSeconds:0.0}s";
    [ObservableProperty] private bool _isExpanded;

    /// <summary>词典查询结果（非空 = 词典卡：渲染音标/释义/变位，忽略 ResultText 文本块）。</summary>
    public STranslate.Sdk.DictionaryResult? Dictionary { get; set; }
    public bool IsDictionary => Dictionary is not null;
    public bool HasSymbols => Dictionary?.Symbols.Count > 0;

    /// <summary>变位汇总文本（复数/过去式/…），空 = 无变位信息。</summary>
    public string ExchangesText
    {
        get
        {
            var d = Dictionary;
            if (d is null)
                return string.Empty;
            var parts = new List<string>();
            void Add(string label, List<string> values)
            {
                if (values.Count > 0)
                    parts.Add($"{label} {string.Join("、", values)}");
            }
            Add("复数:", d.Plurals);
            Add("过去式:", d.PastTense);
            Add("过去分词:", d.PastParticiple);
            Add("现在分词:", d.PresentParticiple);
            Add("第三人称单数:", d.ThirdPersonSingular);
            Add("比较级:", d.Comparative);
            Add("最高级:", d.Superlative);
            return string.Join("  ", parts);
        }
    }

    /// <summary>Dictionary 赋值后调用：通知词典相关绑定。</summary>
    public void RefreshDictionaryNotifiers()
    {
        OnPropertyChanged(nameof(IsDictionary));
        OnPropertyChanged(nameof(HasSymbols));
        OnPropertyChanged(nameof(ExchangesText));
    }

    partial void OnDurationChanged(TimeSpan value)
    {
        OnPropertyChanged(nameof(HasDuration));
        OnPropertyChanged(nameof(DurationText));
    }

    public required string ServiceId { get; init; }
    public required string ServiceName { get; init; }

    /// <summary>重试回调（由 VM 注入，并发翻译单个服务）。</summary>
    public Func<TranslateOutputItem, CancellationToken, Task>? RetryHandler { get; set; }

    private CancellationTokenSource? _retryCts;

    /// <summary>重试本卡翻译。</summary>
    [RelayCommand]
    private async Task RetryAsync()
    {
        if (RetryHandler is null)
            return;
        _retryCts?.Cancel();
        _retryCts = new CancellationTokenSource();
        await RetryHandler(this, _retryCts.Token);
    }

    /// <summary>复制本卡结果。</summary>
    [RelayCommand]
    private async Task CopyAsync()
    {
        if (string.IsNullOrEmpty(ResultText))
            return;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is { } window &&
            TopLevel.GetTopLevel(window)?.Clipboard is { } clipboard)
        {
            var transfer = new DataTransfer();
            transfer.Add(DataTransferItem.CreateText(ResultText));
            WindowsTriggerService.LastProcessedClipboardText = ResultText;
            await clipboard.SetDataAsync(transfer);
        }
    }

    /// <summary>朗读本卡结果（TTS 扩展合成 + 宿主播放）。</summary>
    [RelayCommand]
    private async Task SpeakAsync()
    {
        var vm = App.Services.GetRequiredService<MainViewModel>();
        await vm.SpeakTextPublicAsync(ResultText);
    }
}
