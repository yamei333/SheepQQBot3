using Masuit.Tools;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using System.Windows;

namespace SheepQQBot3.View;

/// <summary>
/// MainWindowBlackList.xaml 的交互逻辑
/// </summary>
public partial class MainWindowBlackList
{
    private static MainWindowBlackListViewModel _vm => PublicVar.Vm.MainWindowBlackListViewModel;

    public MainWindowBlackList()
    {
        InitializeComponent();
    }

    private void List_OnEnable(object sender, RoutedEventArgs e) => ConfigExtensions.SaveConfig();

    private void MainWindowBlackList_OnLoaded(object sender, RoutedEventArgs e)
    {
        DataContext = PublicVar.Vm.MainWindowBlackListViewModel;
        _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
    }

    /// <summary>
    /// 黑名单-新增
    /// </summary>
    private void ListView_OnAdd(object sender, RoutedEventArgs e)
    {
        var addNumberDialog = new AddNumberDialog(PublicVar.GlobalMainWindow, sender, DialogMode.Add, "黑名单QQ号");
        if (addNumberDialog.ShowDialog() != true)
            return;

        var targetId = addNumberDialog.AddNumber;
        var newBlackListUserConfig = new BlackListUserConfig(targetId);
        _vm.SelectedSetConfig.BlackListUserConfigs = _vm.SelectedSetConfig.BlackListUserConfigs
            .CopyAdd(targetId, newBlackListUserConfig);
        _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));

        _vm.SelectedBlackListUserConfig = newBlackListUserConfig;
        ConfigExtensions.SaveConfig();
    }

    /// <summary>
    /// 黑名单-删除
    /// </summary>
    private void ListView_OnDelete(object sender, RoutedEventArgs e)
    {
        if (!MainWindowUtil.ShowDeleteDialog())
            return;

        if (_vm.SelectedBlackListUserConfig is null)
            return;

        var selectedBlackListUserConfig = _vm.SelectedBlackListUserConfig;
        _vm.SelectedSetConfig.BlackListUserConfigs = _vm.SelectedSetConfig.BlackListUserConfigs
            .CopyRemove(selectedBlackListUserConfig.TargetId);
        _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
        _vm.SelectedBlackListUserConfig = null;
        ConfigExtensions.SaveConfig();
    }
}