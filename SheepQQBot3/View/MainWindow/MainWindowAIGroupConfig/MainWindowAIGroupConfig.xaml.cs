using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Extension;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SheepQQBot3.View;

/// <summary>
/// MainWindowAIGroupConfig.xaml 的交互逻辑
/// </summary>
public partial class MainWindowAIGroupConfig
{
    private static readonly Regex _regex = RegexGenerator.Number();

    private static bool IsTextAllowed(string text) => !_regex.IsMatch(text);

    private static MainWindowAIGroupConfigModel _vm => PublicVar.Vm.MainWindowAIGroupConfigModel;

    public MainWindowAIGroupConfig()
    {
        InitializeComponent();
    }

    private void MainWindowAIGroupConfig_OnLoaded(object sender, RoutedEventArgs e)
    {
        DataContext = PublicVar.Vm.MainWindowAIGroupConfigModel;
        _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
    }

    private void TxtContent_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !IsTextAllowed(e.Text);
    }

    private void CheckBox_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigExtensions.SaveConfig();
    }

    private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        ConfigExtensions.SaveConfig();
    }
}