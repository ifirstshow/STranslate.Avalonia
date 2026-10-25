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
