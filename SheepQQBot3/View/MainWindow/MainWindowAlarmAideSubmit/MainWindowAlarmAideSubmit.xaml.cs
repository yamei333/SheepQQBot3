using Masuit.Tools;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;
using System.Windows;

namespace SheepQQBot3.View;

/// <summary>
/// MainWindowAlarmAideSubmit.xaml 的交互逻辑
/// </summary>
public partial class MainWindowAlarmAideSubmit
{
    private static MainWindowAlarmAideSubmitViewModel _vm => PublicVar.Vm.MainWindowAlarmAideSubmitViewModel;

    public MainWindowAlarmAideSubmit()
    {
        InitializeComponent();
    }

    private void MainWindowAlarmAideSubmit_OnLoaded(object sender, RoutedEventArgs e)
    {
        DataContext = PublicVar.Vm.MainWindowAlarmAideSubmitViewModel;
        _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
    }

    /// <summary>
    /// 闹钟助手投稿配置内容列表-新增
    /// </summary>
    private void AlarmAideSubmitMemberList_OnAdd(object sender, RoutedEventArgs e)
    {
        var addNumberDialog = new AddNumberDialog(PublicVar.GlobalMainWindow, sender, DialogMode.Add, "闹钟助手投稿ID");
        if (addNumberDialog.ShowDialog() != true)
            return;

        var alarmAideMemberId = addNumberDialog.AddNumber;
        _vm.SelectedSetConfig.AlarmAideSubmitMemberIds = _vm.SelectedSetConfig.AlarmAideSubmitMemberIds
            .CopyAdd(alarmAideMemberId);
        _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
        _vm.SelectedMemberId = alarmAideMemberId;
        ConfigExtensions.SaveConfig();
    }

    /// <summary>
    /// 闹钟助手投稿配置内容列表-删除
    /// </summary>
    private void AlarmAideSubmitMemberList_OnDelete(object sender, RoutedEventArgs e)
    {
        if (!MainWindowUtil.ShowDeleteDialog())
            return;

        if (_vm.SelectedMemberId.IsNullOrEmpty())
            return;

        _vm.SelectedSetConfig.AlarmAideSubmitMemberIds = _vm.SelectedSetConfig.AlarmAideSubmitMemberIds
            .CopyRemove(_vm.SelectedMemberId);
        _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
        _vm.SelectedMemberId = null;
        ConfigExtensions.SaveConfig();
    }
}