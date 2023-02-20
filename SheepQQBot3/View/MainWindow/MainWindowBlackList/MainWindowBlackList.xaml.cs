using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;
using Yamei.Common;

namespace SheepQQBot3.View
{
    /// <summary>
    /// MainWindowBlackList.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindowBlackList : UserControl
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
            var alarmAideSubmitMemberDialog = new AddNumberDialog(PublicVar.MWindow, sender, DialogMode.Add);
            if (alarmAideSubmitMemberDialog.ShowDialog() != true)
                return;

            var alarmAideMemberId = alarmAideSubmitMemberDialog.AddNumber;
            _vm.SelectedSetConfig.BlackListIds = _vm.SelectedSetConfig.BlackListIds
                .CopyAdd(alarmAideMemberId);
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));

            _vm.SelectedMemberId = alarmAideMemberId;
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

            var alarmAideSubmitMemberIds = _vm.SelectedSetConfig.AlarmAideSubmitMemberIds;
            alarmAideSubmitMemberIds.Remove(_vm.SelectedMemberId.Value);
            _vm.SelectedSetConfig.AlarmAideSubmitMemberIds = new HashSet<long>(alarmAideSubmitMemberIds);
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
            _vm.SelectedMemberId = null;
            ConfigExtensions.SaveConfig();
        }
    }
}