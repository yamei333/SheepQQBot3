using Masuit.Tools;
using SheepQQBot3.Enums;
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

    /// <summary>
    /// 黑名单-新增
    /// </summary>
    private void BlackList_OnAdd(object sender, RoutedEventArgs e)
    {
        var addNumberDialog = new AddNumberDialog(PublicVar.MWindow, sender, DialogMode.Add, "AI黑名单ID");
        if (addNumberDialog.ShowDialog() != true)
            return;

        var blackListMemberId = addNumberDialog.AddNumber.GetValueOrDefault();
        _vm.SelectedSetConfig.AIGroupConfig.BlackListIds = _vm.SelectedSetConfig.AIGroupConfig.BlackListIds
            .CopyAdd(blackListMemberId);
        _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));

        _vm.SelectedMemberId = blackListMemberId;
        ConfigExtensions.SaveConfig();
    }

    /// <summary>
    /// 黑名单-删除
    /// </summary>
    private void BlackList_OnDelete(object sender, RoutedEventArgs e)
    {
        if (!MainWindowUtil.ShowDeleteDialog())
            return;

        if (!_vm.SelectedMemberId.HasValue)
            return;

        _vm.SelectedSetConfig.AIGroupConfig.BlackListIds = _vm.SelectedSetConfig.AIGroupConfig.BlackListIds
            .CopyRemove(_vm.SelectedMemberId.Value);
        _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
        _vm.SelectedMemberId = null;
        ConfigExtensions.SaveConfig();
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