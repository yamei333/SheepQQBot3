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
    /// MainWindowGenshinHelper.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindowGenshinHelper
    {
        private static MainWindowGenshinHelperViewModel _vm => PublicVar.Vm.MainWindowGenshinHelperViewModel;

        /// <inheritdoc/>
        public MainWindowGenshinHelper()
        {
            InitializeComponent();
        }

        private void MainWindowGenshinHelper_OnLoaded(object sender, RoutedEventArgs e)
        {
            DataContext = PublicVar.Vm.MainWindowGenshinHelperViewModel;
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
        }

        /// <summary>
        /// 原神助手树脂提醒配置列表-新增
        /// </summary>
        private void ListView_OnAdd(object sender, RoutedEventArgs e)
        {
            var addGenshinDailyNoteAlarmDialog = new AddGenshinDailyNoteAlarmDialog(PublicVar.MWindow, sender, DialogMode.Add);
            if (addGenshinDailyNoteAlarmDialog.ShowDialog() != true)
                return;

            var configId = Guid.NewGuid();
            var vm = addGenshinDailyNoteAlarmDialog.Vm;
            var newGenshinResinAlarm = new GenshinResinAlarm(
                configId,
                vm.ConfigName,
                vm.Cookies,
                vm.BarkKey,
                vm.TargetId.GetValueOrDefault());
            var selectedSetConfig = _vm.SelectedSetConfig;
            selectedSetConfig.GenshinHelperConfig ??= new GenshinHelperConfig();
            selectedSetConfig.GenshinHelperConfig.GenshinResinAlarms = selectedSetConfig.GenshinHelperConfig.GenshinResinAlarms
                .CopyAdd(configId, newGenshinResinAlarm);
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
            ConfigExtensions.SaveConfig(GenshinResinAlarmList);
            _vm.SelectedGenshinResinAlarm = newGenshinResinAlarm;
        }

        /// <summary>
        /// 原神助手树脂提醒配置列表-修改
        /// </summary>
        private void ListView_OnEdit(object sender, RoutedEventArgs e)
        {
            var selectedGenshinResinAlarm = _vm.SelectedGenshinResinAlarm;
            var addGenshinDailyNoteAlarmDialog = new AddGenshinDailyNoteAlarmDialog(
                PublicVar.MWindow, sender, DialogMode.Edit,
                selectedGenshinResinAlarm.ConfigName,
                selectedGenshinResinAlarm.Cookies,
                selectedGenshinResinAlarm.BarkKey,
                selectedGenshinResinAlarm.TargetId);

            if (addGenshinDailyNoteAlarmDialog.ShowDialog() != true)
                return;

            var selectedSetConfig = _vm.SelectedSetConfig;
            var configId = selectedGenshinResinAlarm.Id;
            selectedSetConfig.GenshinHelperConfig.GenshinResinAlarms = selectedSetConfig.GenshinHelperConfig.GenshinResinAlarms
                .CopyEdit(configId, genshinResinAlarm =>
                {
                    var vm = addGenshinDailyNoteAlarmDialog.Vm;
                    genshinResinAlarm.ConfigName = vm.ConfigName;
                    genshinResinAlarm.Cookies = vm.Cookies;
                    genshinResinAlarm.BarkKey = vm.BarkKey;
                    genshinResinAlarm.TargetId = vm.TargetId.GetValueOrDefault();
                });
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
            ConfigExtensions.SaveConfig(GenshinResinAlarmList);
        }

        /// <summary>
        /// 原神助手树脂提醒配置列表-删除
        /// </summary>
        private void ListView_OnDelete(object sender, RoutedEventArgs e)
        {
            if (!MainWindowUtil.ShowDeleteDialog())
                return;

            var selectedSetConfig = _vm.SelectedSetConfig;
            selectedSetConfig.GenshinHelperConfig.GenshinResinAlarms = selectedSetConfig.GenshinHelperConfig.GenshinResinAlarms
                .CopyRemove(_vm.SelectedGenshinResinAlarm.Id);
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
            _vm.SelectedGenshinResinAlarm = default;
            ConfigExtensions.SaveConfig(GenshinResinAlarmList);
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GenshinResinAlarmList.SelectedItems.Count == 1)
                ListView_OnEdit(new MenuItem { Header = "修改" }, e);
        }

        /// <summary>
        /// 启用/非启用时保存
        /// </summary>
        private void List_OnEnable(object sender, RoutedEventArgs e)
        {
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
            ConfigExtensions.SaveConfig();
        }
    }
}