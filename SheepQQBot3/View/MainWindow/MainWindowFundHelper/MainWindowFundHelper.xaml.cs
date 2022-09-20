using System;
using System.Collections.Generic;
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
    /// MainWindowFundHelper.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindowFundHelper : UserControl
    {
        private static MainWindowFundHelperViewModel _vm => PublicVar.Vm.MainWindowFundHelperViewModel;
        public MainWindowFundHelper()
        {
            InitializeComponent();
        }

        private void MainWindowFundHelper_OnLoaded(object sender, RoutedEventArgs e)
        {
            DataContext = PublicVar.Vm.MainWindowFundHelperViewModel;
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
        }

        /// <summary>
        /// 基金助手播报时间配置列表-新增
        /// </summary>
        private void FundAlarmConfigList_OnAdd(object sender, RoutedEventArgs e)
        {
            var addDateTimeConfigDialog = new AddDateTimeConfigDialog(PublicVar.MWindow, sender, DialogMode.Add);
            if (addDateTimeConfigDialog.ShowDialog() != true)
                return;

            var configId = Guid.NewGuid();
            var newFundAlarmConfig = new FundAlarmConfig(
                configId,
                addDateTimeConfigDialog.AlarmName,
                addDateTimeConfigDialog.Condition);
            var selectedSetConfig = _vm.SelectedSetConfig;
            selectedSetConfig.FundAlarmConfigs = selectedSetConfig.FundAlarmConfigs
                .CopyAdd(configId, newFundAlarmConfig);
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
            ConfigExtensions.SaveConfig(FundAlarmConfigList);
            _vm.SelectedFundAlarmConfig = newFundAlarmConfig;
        }

        /// <summary>
        /// 基金助手播报时间配置列表-修改
        /// </summary>
        private void FundAlarmConfigList_OnEdit(object sender, RoutedEventArgs e)
        {
            var selectedFundAlarmConfig = _vm.SelectedFundAlarmConfig;
            var addDateTimeConfigDialog = new AddDateTimeConfigDialog(PublicVar.MWindow, sender, DialogMode.Edit)
            {
                AlarmName = selectedFundAlarmConfig.AlarmName,
                Condition = selectedFundAlarmConfig.Condition,
            };

            if (addDateTimeConfigDialog.ShowDialog() != true)
                return;

            var selectedSetConfig = _vm.SelectedSetConfig;
            var configId = selectedFundAlarmConfig.ConfigId;
            selectedSetConfig.FundAlarmConfigs = selectedSetConfig.FundAlarmConfigs
                .CopyEdit(configId, fundAlarmConfig =>
                {
                    fundAlarmConfig.AlarmName = addDateTimeConfigDialog.AlarmName;
                    fundAlarmConfig.Condition = addDateTimeConfigDialog.Condition;
                });
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
            ConfigExtensions.SaveConfig(FundAlarmConfigList);
        }

        /// <summary>
        /// 基金助手播报时间配置列表-删除
        /// </summary>
        private void FundAlarmConfigList_OnDelete(object sender, RoutedEventArgs e)
        {
            if (!MainWindowUtil.ShowDeleteDialog())
                return;

            var selectedSetConfig = _vm.SelectedSetConfig;
            selectedSetConfig.FundAlarmConfigs = selectedSetConfig.FundAlarmConfigs
                .CopyRemove(_vm.SelectedSetConfig.Id);
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
            _vm.SelectedAlarmFundConfig = default;
            ConfigExtensions.SaveConfig(AlarmFundConfigList);
        }

        /// <summary>
        /// 基金助手播报时间配置列表-测试
        /// </summary>
        private void FundAlarmConfigList_OnTest(object sender, RoutedEventArgs e)
            => TaskProcess.SendFundAlarmMessage(_vm.SelectedSetConfig, _vm.SelectedFundAlarmConfig, DateTime.Now, true);

        /// <summary>
        /// 基金助手阈值观测时间配置列表-新增
        /// </summary>
        private void FundLimitObserveConfigList_OnAdd(object sender, RoutedEventArgs e)
        {
            var addDateTimeConfigDialog = new AddDateTimeConfigDialog(PublicVar.MWindow, sender, DialogMode.Add);
            if (addDateTimeConfigDialog.ShowDialog() != true)
                return;

            var configId = Guid.NewGuid();
            var newFundLimitObserveConfig = new FundLimitObserveConfig(
                configId,
                addDateTimeConfigDialog.AlarmName,
                addDateTimeConfigDialog.Condition);
            var selectedSetConfig = _vm.SelectedSetConfig;
            selectedSetConfig.FundLimitObserveConfigs = selectedSetConfig.FundLimitObserveConfigs
                .CopyAdd(configId, newFundLimitObserveConfig);
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
            ConfigExtensions.SaveConfig(FundLimitObserveConfigList);
            _vm.SelectedFundLimitObserveConfig = newFundLimitObserveConfig;
        }

        /// <summary>
        /// 基金助手阈值观测时间配置列表-修改
        /// </summary>
        private void FundLimitObserveConfigList_OnEdit(object sender, RoutedEventArgs e)
        {
            var selectedFundLimitObserveConfig = _vm.SelectedFundLimitObserveConfig;
            var addDateTimeConfigDialog = new AddDateTimeConfigDialog(PublicVar.MWindow, sender, DialogMode.Edit)
            {
                AlarmName = selectedFundLimitObserveConfig.LimitObserveName,
                Condition = selectedFundLimitObserveConfig.Condition,
            };

            if (addDateTimeConfigDialog.ShowDialog() != true)
                return;

            var selectedSetConfig = _vm.SelectedSetConfig;
            var configId = selectedFundLimitObserveConfig.ConfigId;
            selectedSetConfig.FundLimitObserveConfigs = selectedSetConfig.FundLimitObserveConfigs
                .CopyEdit(configId, fundLimitObserveConfig =>
                {
                    fundLimitObserveConfig.LimitObserveName = addDateTimeConfigDialog.AlarmName;
                    fundLimitObserveConfig.Condition = addDateTimeConfigDialog.Condition;
                });
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
            ConfigExtensions.SaveConfig(FundLimitObserveConfigList);
        }

        /// <summary>
        /// 基金助手阈值观测时间配置列表-删除
        /// </summary>
        private void FundLimitObserveConfigList_OnDelete(object sender, RoutedEventArgs e)
        {
            if (!MainWindowUtil.ShowDeleteDialog())
                return;

            var selectedSetConfig = _vm.SelectedSetConfig;
            selectedSetConfig.FundAlarmConfigs = selectedSetConfig.FundAlarmConfigs
                .CopyRemove(_vm.SelectedSetConfig.Id);
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
            _vm.SelectedAlarmFundConfig = default;
            ConfigExtensions.SaveConfig(FundLimitObserveConfigList);
        }

        /// <summary>
        /// 基金助手阈值观测时间配置列表-测试
        /// </summary>
        private void FundLimitObserveConfigList_OnTest(object sender, RoutedEventArgs e)
            => TaskProcess.SendFundLimitMessage(_vm.SelectedSetConfig, _vm.SelectedFundLimitObserveConfig, DateTime.Now, true);

        /// <summary>
        /// 基金助手播报列表-新增
        /// </summary>
        private void AlarmFundConfigList_OnAdd(object sender, RoutedEventArgs e)
        {
            var addFundAlarmDialog = new AddFundAlarmDialog(PublicVar.MWindow, sender, DialogMode.Add);
            if (addFundAlarmDialog.ShowDialog() != true)
                return;

            var selectedFundAlarmConfig = _vm.SelectedFundAlarmConfig;
            var alarmFundConfigs = selectedFundAlarmConfig.AlarmFundConfigs;
            var alarmFundConfigKey = alarmFundConfigs.GetSequence();
            var fundId = addFundAlarmDialog.FundId;
            var newAlarmFundConfig = new AlarmFundConfig(fundId, addFundAlarmDialog.FundRemark);
            var selectedNewAlarmFundConfig = new KeyValuePair<int, AlarmFundConfig>(
                alarmFundConfigKey, newAlarmFundConfig);
            selectedFundAlarmConfig.AlarmFundConfigs = selectedFundAlarmConfig.AlarmFundConfigs
                .CopyAdd(alarmFundConfigKey, newAlarmFundConfig);
            _vm.OnPropertyChanged(nameof(_vm.SelectedAlarmFundConfig));
            ConfigExtensions.SaveConfig(AlarmFundConfigList);
            _vm.SelectedAlarmFundConfig = selectedNewAlarmFundConfig;
        }

        /// <summary>
        /// 基金助手播报列表-修改
        /// </summary>
        private void AlarmFundConfigList_OnEdit(object sender, RoutedEventArgs e)
        {
            var selectedAlarmFundConfig = _vm.SelectedAlarmFundConfig.Value;
            var addFundAlarmDialog = new AddFundAlarmDialog(PublicVar.MWindow, sender, DialogMode.Edit)
            {
                FundId = selectedAlarmFundConfig.FundId,
                FundRemark = selectedAlarmFundConfig.FundRemark,
            };
            if (addFundAlarmDialog.ShowDialog() != true)
                return;

            var selectedFundAlarmConfig = _vm.SelectedFundAlarmConfig;
            var alarmAideTextId = ((KeyValuePair<int, AlarmFundConfig>)AlarmFundConfigList.SelectedValue).Key;
            selectedFundAlarmConfig.AlarmFundConfigs = selectedFundAlarmConfig.AlarmFundConfigs
                .CopyEdit(alarmAideTextId, fundAlarmConfig =>
                {
                    fundAlarmConfig.FundId = addFundAlarmDialog.FundId;
                    fundAlarmConfig.FundRemark = addFundAlarmDialog.FundRemark;
                });
            _vm.OnPropertyChanged(nameof(_vm.SelectedAlarmFundConfig));
            ConfigExtensions.SaveConfig(AlarmFundConfigList);
        }

        /// <summary>
        /// 基金助手播报列表-删除
        /// </summary>
        private void AlarmFundConfigList_OnDelete(object sender, RoutedEventArgs e)
        {
            if (!MainWindowUtil.ShowDeleteDialog())
                return;

            var selectedFundAlarmConfig = _vm.SelectedFundAlarmConfig;
            var alarmFundConfigId = ((KeyValuePair<int, AlarmFundConfig>)AlarmFundConfigList.SelectedItem).Key;
            selectedFundAlarmConfig.AlarmFundConfigs = selectedFundAlarmConfig.AlarmFundConfigs
                .CopyRemove(alarmFundConfigId);
            _vm.OnPropertyChanged(nameof(_vm.SelectedAlarmFundConfig));
            _vm.SelectedFundAlarmConfig = null;
            ConfigExtensions.SaveConfig(AlarmFundConfigList);
        }

        /// <summary>
        /// 基金助手阈值观测列表-新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LimitObserveFundConfigList_OnAdd(object sender, RoutedEventArgs e)
        {
            var addFundLimitDialog = new AddFundLimitObserveDialog(PublicVar.MWindow, sender, DialogMode.Add);
            if (addFundLimitDialog.ShowDialog() != true)
                return;

            var fundId = addFundLimitDialog.FundId;
            var fundObserveType = addFundLimitDialog.FundObserveType;
            var selectedFundLimitObserveConfig = _vm.SelectedFundLimitObserveConfig;
            var limitObserveFundConfigs = selectedFundLimitObserveConfig.LimitObserveFundConfigs;
            var newLimitObserveFundConfigId = limitObserveFundConfigs.GetSequence();
            var newLimitObserveFundConfig = new LimitObserveFundConfig(
                fundId, fundObserveType, addFundLimitDialog.AlertLimit);
            var selectedNewLimitObserveFundConfig = new KeyValuePair<int, LimitObserveFundConfig>(
                newLimitObserveFundConfigId, newLimitObserveFundConfig);
            selectedFundLimitObserveConfig.LimitObserveFundConfigs = selectedFundLimitObserveConfig.LimitObserveFundConfigs
                .CopyAdd(newLimitObserveFundConfigId, newLimitObserveFundConfig);
            _vm.OnPropertyChanged(nameof(_vm.SelectedFundLimitObserveConfig));
            ConfigExtensions.SaveConfig(LimitObserveFundConfigList);
            _vm.SelectedLimitObserveFundConfig = selectedNewLimitObserveFundConfig;
        }

        /// <summary>
        /// 基金助手阈值观测列表-修改
        /// </summary>
        private void LimitObserveFundConfigList_OnEdit(object sender, RoutedEventArgs e)
        {
            var selectedLimitObserveFundConfig = _vm.SelectedLimitObserveFundConfig.Value;
            var addFundLimitDialog = new AddFundLimitObserveDialog(PublicVar.MWindow, sender, DialogMode.Edit)
            {
                FundId = selectedLimitObserveFundConfig.FundId,
                FundObserveType = selectedLimitObserveFundConfig.FundObserveType,
                AlertLimit = selectedLimitObserveFundConfig.AlertLimit,
            };
            if (addFundLimitDialog.ShowDialog() != true)
                return;

            var limitObserveFundConfigId = ((KeyValuePair<int, LimitObserveFundConfig>)LimitObserveFundConfigList.SelectedItem).Key;
            var selectedFundLimitObserveConfig = _vm.SelectedFundLimitObserveConfig;
            selectedFundLimitObserveConfig.LimitObserveFundConfigs = selectedFundLimitObserveConfig.LimitObserveFundConfigs
                .CopyEdit(limitObserveFundConfigId, fundLimitObserveConfig =>
                {
                    fundLimitObserveConfig.FundId = addFundLimitDialog.FundId;
                    fundLimitObserveConfig.FundObserveType = addFundLimitDialog.FundObserveType;
                    fundLimitObserveConfig.AlertLimit = addFundLimitDialog.AlertLimit;
                });
            _vm.OnPropertyChanged(nameof(_vm.SelectedFundLimitObserveConfig));
            ConfigExtensions.SaveConfig(LimitObserveFundConfigList);
        }

        /// <summary>
        /// 基金助手阈值观测列表-删除
        /// </summary>
        private void LimitObserveFundConfigList_OnDelete(object sender, RoutedEventArgs e)
        {
            if (!MainWindowUtil.ShowDeleteDialog())
                return;

            var selectedFundLimitObserveConfig = _vm.SelectedFundLimitObserveConfig;
            var limitObserveFundConfigId = ((KeyValuePair<int, LimitObserveFundConfig>)LimitObserveFundConfigList.SelectedItem).Key;
            selectedFundLimitObserveConfig.LimitObserveFundConfigs = selectedFundLimitObserveConfig.LimitObserveFundConfigs
                .CopyRemove(limitObserveFundConfigId);
            _vm.OnPropertyChanged(nameof(_vm.SelectedFundLimitObserveConfig));
            _vm.SelectedLimitObserveFundConfig = default;
            ConfigExtensions.SaveConfig(LimitObserveFundConfigList);
        }

        private void FundAlarmConfigList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FundAlarmConfigList.SelectedItems.Count == 1)
                FundAlarmConfigList_OnEdit(new MenuItem { Header = "修改" }, e);
        }

        private void FundLimitObserveConfigList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FundLimitObserveConfigList.SelectedItems.Count == 1)
                FundLimitObserveConfigList_OnEdit(new MenuItem { Header = "修改" }, e);
        }

        private void AlarmFundConfigList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AlarmFundConfigList.SelectedItems.Count == 1)
                AlarmFundConfigList_OnEdit(new MenuItem { Header = "修改" }, e);
        }

        private void LimitObserveFundConfigList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LimitObserveFundConfigList.SelectedItems.Count == 1)
                LimitObserveFundConfigList_OnEdit(new MenuItem { Header = "修改" }, e);
        }
    }
}