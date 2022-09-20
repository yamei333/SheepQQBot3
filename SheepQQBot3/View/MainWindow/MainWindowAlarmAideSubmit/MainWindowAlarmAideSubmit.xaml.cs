using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;

namespace SheepQQBot3.View
{
    /// <summary>
    /// MainWindowAlarmAideSubmit.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindowAlarmAideSubmit : UserControl
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
            var alarmAideSubmitMemberDialog = new AddAlarmAideSubmitMemberDialog(PublicVar.MWindow, sender, DialogMode.Add);
            if (alarmAideSubmitMemberDialog.ShowDialog() != true)
                return;

            var alarmAideMemberId = alarmAideSubmitMemberDialog.AlarmAideMemberId;
            var alarmAideSubmitMemberIds = _vm.SelectedSetConfig.AlarmAideSubmitMemberIds;
            _vm.SelectedSetConfig.AlarmAideSubmitMemberIds = alarmAideSubmitMemberIds == null
                ? new HashSet<long> { alarmAideMemberId }
                : new HashSet<long>(alarmAideSubmitMemberIds) { alarmAideMemberId };
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

            if (!_vm.SelectedMemberId.HasValue)
                return;

            var alarmAideSubmitMemberIds = _vm.SelectedSetConfig.AlarmAideSubmitMemberIds;
            alarmAideSubmitMemberIds.Remove(_vm.SelectedMemberId.Value);
            _vm.SelectedSetConfig.AlarmAideSubmitMemberIds = new HashSet<long>(alarmAideSubmitMemberIds);
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
            _vm.SelectedMemberId = null;
            ConfigExtensions.SaveConfig();
        }
    }
}