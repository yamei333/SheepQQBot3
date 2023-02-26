using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using Yamei.Common;

namespace SheepQQBot3.View
{
    /// <summary>
    /// MainWindowLiveAlarm.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindowLiveAlarm
    {
        private static MainWindowLiveAlarmViewModel _vm => PublicVar.Vm.MainWindowLiveAlarmViewModel;

        public MainWindowLiveAlarm()
        {
            InitializeComponent();
        }

        private void MainWindowLiveAlarm_OnLoaded(object sender, RoutedEventArgs e)
        {
            DataContext = PublicVar.Vm.MainWindowLiveAlarmViewModel;
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
        }

        /// <summary>
        /// 直播提醒配置列表-新增
        /// </summary>
        private void ListView_OnAdd(object sender, RoutedEventArgs e)
        {
            var addLiveAlarmDialog = new AddLiveAlarmDialog(PublicVar.MWindow, sender, DialogMode.Add);
            if (addLiveAlarmDialog.ShowDialog() != true)
                return;

            var configId = Guid.NewGuid();
            var newLiveAlarmConfig = new LiveAlarmConfig(
                configId,
                addLiveAlarmDialog.LiveType,
                addLiveAlarmDialog.LiveRoomId);
            var selectedSetConfig = _vm.SelectedSetConfig;
            selectedSetConfig.LiveAlarmConfigs = selectedSetConfig.LiveAlarmConfigs
                .CopyAdd(configId, newLiveAlarmConfig);
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
            ConfigExtensions.SaveConfig(LiveAlarmConfigList);
            _vm.SelectedLiveAlarmConfig = newLiveAlarmConfig;
        }

        /// <summary>
        /// 直播提醒配置列表-修改
        /// </summary>
        private void ListView_OnEdit(object sender, RoutedEventArgs e)
        {
            var selectedLiveAlarmConfig = _vm.SelectedLiveAlarmConfig;
            var addLiveAlarmDialog = new AddLiveAlarmDialog(PublicVar.MWindow, sender, DialogMode.Edit)
            {
                LiveType = selectedLiveAlarmConfig.LiveType,
                LiveRoomId = selectedLiveAlarmConfig.LiveRoomId,
            };

            if (addLiveAlarmDialog.ShowDialog() != true)
                return;

            var selectedSetConfig = _vm.SelectedSetConfig;
            var configId = selectedLiveAlarmConfig.Id;
            selectedSetConfig.LiveAlarmConfigs = selectedSetConfig.LiveAlarmConfigs
                .CopyEdit(configId, liveAlarmConfig =>
                {
                    liveAlarmConfig.LiveType = addLiveAlarmDialog.LiveType;
                    liveAlarmConfig.LiveRoomId = addLiveAlarmDialog.LiveRoomId;
                });
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
            ConfigExtensions.SaveConfig(LiveAlarmConfigList);
        }

        /// <summary>
        /// 直播提醒配置列表-删除
        /// </summary>
        private void ListView_OnDelete(object sender, RoutedEventArgs e)
        {
            if (!MainWindowUtil.ShowDeleteDialog())
                return;

            var selectedSetConfig = _vm.SelectedSetConfig;
            selectedSetConfig.LiveAlarmConfigs = selectedSetConfig.LiveAlarmConfigs
                .CopyRemove(_vm.SelectedLiveAlarmConfig.Id);
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
            _vm.SelectedLiveAlarmConfig = default;
            ConfigExtensions.SaveConfig(LiveAlarmConfigList);
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LiveAlarmConfigList.SelectedItems.Count == 1)
                ListView_OnEdit(new MenuItem { Header = "修改" }, e);
        }
    }
}