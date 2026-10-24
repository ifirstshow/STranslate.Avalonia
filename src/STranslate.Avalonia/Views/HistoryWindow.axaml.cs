using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using STranslate.Avalonia.Services;
using STranslate.Avalonia.ViewModels;

namespace STranslate.Avalonia.Views;

/// <summary>历史条目的展示模型。</summary>
public sealed class HistoryRow
{
    public required HistoryItem Item { get; init; }
    public string TimeText => Item.Time.ToString("MM-dd HH:mm");
    public string SourceText => Item.SourceText;
    public string ResultText => Item.ResultText;
    public string StarText => Item.Favorite ? "★" : "☆";
    public string NoteText => Item.Note;
    public bool HasNote => !string.IsNullOrEmpty(Item.Note);
}

public partial class HistoryWindow : Window
{
    private static readonly JsonSerializerOptions ExportOpts = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly HistoryService _history;
    private readonly MainViewModel _viewModel;

    public HistoryWindow()
    {
        _history = App.Services.GetRequiredService<HistoryService>();
        _viewModel = App.Services.GetRequiredService<MainViewModel>();
        InitializeComponent();
        Reload("");
    }

    private void Reload(string keyword)
    {
        var items = _history.Search(keyword, FavOnlyToggle.IsChecked == true);
        HistoryList.ItemsSource = items.Select(i => new HistoryRow { Item = i }).ToList();
        FooterText.Text = $"{items.Count} 条 · 双击回填到主窗并翻译；★ 收藏 · ✎ 备注 · ✕ 删除";
    }

    private void OnSearchKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Reload(SearchBox.Text ?? "");
            e.Handled = true;
        }
    }

    private void OnSearchClick(object? sender, RoutedEventArgs e) => Reload(SearchBox.Text ?? "");

    private void OnFavOnlyClick(object? sender, RoutedEventArgs e) => Reload(SearchBox.Text ?? "");

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        _history.Clear();
        Reload("");
    }

    private HistoryRow? RowFrom(object? sender)
    {
        var container = sender as Control;
        var item = container?.DataContext as HistoryRow;
        return item;
    }

    private void OnStarClick(object? sender, RoutedEventArgs e)
    {
        if (RowFrom(sender) is not { } row)
            return;
        _history.SetFavorite(row.Item.Id, !row.Item.Favorite);
        Reload(SearchBox.Text ?? "");
    }

    private async void OnNoteClick(object? sender, RoutedEventArgs e)
    {
        if (RowFrom(sender) is not { } row)
            return;

        var input = new TextBox { Text = row.Item.Note, AcceptsReturn = true, MinHeight = 80, Width = 360 };
        var dialog = new FAContentDialog
        {
            Title = "编辑备注",
            Content = input,
            PrimaryButtonText = "保存",
            CloseButtonText = "取消"
        };
        dialog.PrimaryButtonClick += (_, args) =>
        {
            args.Cancel = false;
            _history.SetNote(row.Item.Id, input.Text ?? "");
        };
        await dialog.ShowAsync(this);
        Reload(SearchBox.Text ?? "");
    }

    private async void OnExportClick(object? sender, RoutedEventArgs e)
    {
        var items = _history.Search(SearchBox.Text ?? "", FavOnlyToggle.IsChecked == true, int.MaxValue);
        if (items.Count == 0)
            return;
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "导出历史",
            SuggestedFileName = $"stranslate-history-{DateTime.Now:yyyyMMdd-HHmmss}.json",
            DefaultExtension = "json"
        });
        if (file is null)
            return;
        await using var stream = new FileStream(file.Path.LocalPath, FileMode.Create, FileAccess.Write, FileShare.Read);
        await JsonSerializer.SerializeAsync(stream, items, ExportOpts);
    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (RowFrom(sender) is not { } row)
            return;
        _history.Delete(row.Item.Id);
        Reload(SearchBox.Text ?? "");
    }

    private void OnItemDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (HistoryList.SelectedItem is not HistoryRow row)
            return;
        _viewModel.ShowAndTranslate(row.Item.SourceText, forceForeground: false);
        Close();
    }
}
