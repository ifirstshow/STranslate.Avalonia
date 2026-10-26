using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using STranslate.Avalonia.ViewModels;
using STranslate.Sdk;

namespace STranslate.Avalonia.Views;

/// <summary>首次运行向导（简化版）：语言对 + 主题 + 自启，完成写入配置并继续启动。</summary>
public partial class WelcomeWindow : Window
{
    private readonly MainViewModel _viewModel;

    public WelcomeWindow(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        SourceCombo.ItemsSource = _viewModel.Languages;
        TargetCombo.ItemsSource = _viewModel.Languages;
        SourceCombo.SelectedItem = _viewModel.SourceLang;
        TargetCombo.SelectedItem = _viewModel.TargetLang;
    }

    private void OnFinishClick(object? sender, RoutedEventArgs e)
    {
        if (SourceCombo.SelectedItem is LangEnum src)
            _viewModel.SourceLang = src;
        if (TargetCombo.SelectedItem is LangEnum tgt)
            _viewModel.TargetLang = tgt;
        _viewModel.Theme = ThemeCombo.SelectedIndex switch
        {
            1 => "Light",
            2 => "Dark",
            _ => "Default"
        };
        if (AutoStartCheck.IsChecked == true)
            _viewModel.AutoStart = true;

        Close();
    }
}
