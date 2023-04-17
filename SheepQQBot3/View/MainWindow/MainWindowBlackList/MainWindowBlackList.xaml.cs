using System.Windows;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;
using Yamei.Common;

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

    private void MainWindowBlackList_OnLoaded(object sender, RoutedEventArgs e)
    {
        DataContext = PublicVar.Vm.MainWindowBlackListViewModel;
        _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
    }

    /// <summary>
    /// 黑名单-新增
    /// </summary>
    private void BlackList_OnAdd(object sender, RoutedEventArgs e)
    {
        var addNumberDialog = new AddNumberDialog(PublicVar.MWindow, sender, DialogMode.Add, "黑名单ID");
        if (addNumberDialog.ShowDialog() != true)
            return;

        var blackListMemberId = addNumberDialog.AddNumber.GetValueOrDefault();
        _vm.SelectedSetConfig.BlackListIds = _vm.SelectedSetConfig.BlackListIds
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

        _vm.SelectedSetConfig.BlackListIds = _vm.SelectedSetConfig.BlackListIds
            .CopyRemove(_vm.SelectedMemberId.Value);
        _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
        _vm.SelectedMemberId = null;
        ConfigExtensions.SaveConfig();
    }
}