using Masuit.Tools;
using Microsoft.Win32;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.View;

/// <summary>
/// MainWindowAlarmAide.xaml 的交互逻辑
/// </summary>
public partial class MainWindowAlarmAide : UserControl
{
    private static MainWindowAlarmAideViewModel _vm => PublicVar.Vm.MainWindowAlarmAideViewModel;

    public MainWindowAlarmAide()
    {
        InitializeComponent();
    }

    private void MainWindowAlarmAide_OnLoaded(object sender, RoutedEventArgs e)
    {
        DataContext = PublicVar.Vm.MainWindowAlarmAideViewModel;
        _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
    }

    private void AlarmAideList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (AlarmAideList.SelectedItems.Count == 1)
            AlarmAideList_OnEdit(new MenuItem { Header = "修改" }, e);
    }

    /// <summary>
    /// 闹钟助手列表-取得Guid
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AlarmAideList_OnGetGuid(object sender, RoutedEventArgs e)
    {
        ClipboardExtensions.SetText(_vm.SelectedAlarmAideConfig?.Id.ToString());
        MessageBox.Show("已将Id复制到剪贴板");
    }

    /// <summary>
    /// 闹钟助手列表-新增
    /// </summary>
    private void AlarmAideList_OnAdd(object sender, RoutedEventArgs e)
    {
        var addAlarmAideDialog = new AddDateTimeConfigDialog(PublicVar.MWindow, sender, DialogMode.Add);
        if (addAlarmAideDialog.ShowDialog() != true)
            return;

        var selectedSetConfig = _vm.SelectedSetConfig;
        var alarmAideConfigs = selectedSetConfig.AlarmAideConfigs;
        var newAlarmAideConfig = new AlarmAideConfig(
            addAlarmAideDialog.AlarmName,
            addAlarmAideDialog.Condition);
        alarmAideConfigs.Add(newAlarmAideConfig.Id, newAlarmAideConfig);
        selectedSetConfig.AlarmAideConfigs = new Dictionary<Guid, AlarmAideConfig>(alarmAideConfigs);
        _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
        ConfigExtensions.SaveConfig();
        _vm.SelectedAlarmAideConfig = newAlarmAideConfig;
    }

    /// <summary>
    /// 闹钟助手列表-修改
    /// </summary>
    private void AlarmAideList_OnEdit(object sender, RoutedEventArgs e)
    {
        var selectedAlarmAideConfig = _vm.SelectedAlarmAideConfig;
        var addAlarmAideDialog = new AddDateTimeConfigDialog(PublicVar.MWindow, sender, DialogMode.Edit)
        {
            AlarmName = selectedAlarmAideConfig.AlarmName,
            Condition = selectedAlarmAideConfig.Condition,
        };
        if (addAlarmAideDialog.ShowDialog() != true)
            return;

        var selectedSetConfig = _vm.SelectedSetConfig;
        var alarmAideConfigs = selectedSetConfig.AlarmAideConfigs;
        var selectAlarmAideConfig = alarmAideConfigs[selectedAlarmAideConfig.Id];
        selectAlarmAideConfig.AlarmName = addAlarmAideDialog.AlarmName;
        selectAlarmAideConfig.Condition = addAlarmAideDialog.Condition;
        selectedSetConfig.AlarmAideConfigs = new Dictionary<Guid, AlarmAideConfig>(alarmAideConfigs);
        _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
        ConfigExtensions.SaveConfig();
    }

    /// <summary>
    /// 闹钟助手列表-删除
    /// </summary>
    private void AlarmAideList_OnDelete(object sender, RoutedEventArgs e)
    {
        if (!MainWindowUtil.ShowDeleteDialog())
            return;

        var selectedSetConfig = _vm.SelectedSetConfig;
        var alarmAideConfigs = selectedSetConfig.AlarmAideConfigs;
        alarmAideConfigs.Remove(_vm.SelectedAlarmAideConfig.Id);
        selectedSetConfig.AlarmAideConfigs = new Dictionary<Guid, AlarmAideConfig>(alarmAideConfigs);
        _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
        _vm.SelectedAlarmAideConfig = default; ;
        ConfigExtensions.SaveConfig();
    }

    /// <summary>
    /// 闹钟助手列表-测试
    /// </summary>
    private async void AlarmAideList_OnTest(object sender, RoutedEventArgs e)
    {
        var alarmTexts = _vm.SelectedAlarmAideConfig.AlarmTexts;
        if (!alarmTexts.Any())
            return;

        var alarmText = alarmTexts.Values
            .OrderBy(each => Guid.NewGuid())
            .First();

        var selectedSetConfig = _vm.SelectedSetConfig;
        switch (selectedSetConfig.TargetType)
        {
            case BotConfigTargetType.Group:
                await BotServer.SendGroupMessageAsync(selectedSetConfig.TargetId, alarmText, _vm.SetConfigs).ConfigureAwait(false);
                break;
            case BotConfigTargetType.Private:
                await BotServer.SendPrivateMessageAsync(selectedSetConfig.TargetId, alarmText).ConfigureAwait(false);
                break;
            case BotConfigTargetType.Common:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void AlarmAideTextList_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (_vm.SelectedAlarmAideConfig == null)
            e.Handled = true;
    }

    private void AlarmAideTextList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _vm.IsSingleSelectedAlarmAideText = AlarmAideTextList.SelectedItems.Count == 1;
    }

    private void AlarmAideTextList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (AlarmAideTextList.SelectedItems.Count == 1)
            AlarmAideTextList_OnEdit(new MenuItem { Header = "修改" }, e);
    }

    /// <summary>
    /// 闹钟助手内容列表-新增
    /// </summary>
    private void AlarmAideTextList_OnAdd(object sender, RoutedEventArgs e)
    {
        var addAlarmAideTextDialog = new AddAlarmAideTextDialog(PublicVar.MWindow, sender, DialogMode.Add);
        if (addAlarmAideTextDialog.ShowDialog() == true)
            _vm.OnAddAlarmAideTest(addAlarmAideTextDialog.AlarmText, AlarmAideTextList);
    }

    /// <summary>
    /// 闹钟助手内容列表-复制
    /// </summary>
    private void AlarmAideTextList_OnCopy(object sender, RoutedEventArgs e)
    {
        var addAlarmAideText = _vm.SelectedAlarmText.Value;
        var alarmTexts = _vm.SelectedAlarmAideConfig.AlarmTexts;
        var newAlarmAideTextId = alarmTexts.GetSequence();
        var newAlarmAideText = new KeyValuePair<int, string>(newAlarmAideTextId, addAlarmAideText);
        _vm.SelectedAlarmAideConfig.AlarmTexts = _vm.SelectedAlarmAideConfig.AlarmTexts
            .CopyAdd(newAlarmAideTextId, addAlarmAideText);
        _vm.SelectedAlarmText = newAlarmAideText;
        ConfigExtensions.SaveConfig(AlarmAideTextList);
    }

    /// <summary>
    /// 闹钟助手内容列表-编辑
    /// </summary>
    private void AlarmAideTextList_OnEdit(object sender, RoutedEventArgs e)
    {
        var addAlarmAideTextDialog = new AddAlarmAideTextDialog(PublicVar.MWindow, sender, DialogMode.Edit)
        {
            AlarmText = _vm.SelectedAlarmText.Value,
        };
        if (addAlarmAideTextDialog.ShowDialog() == true)
        {
            var alarmAideText = addAlarmAideTextDialog.AlarmText;
            var alarmAideTextId = ((KeyValuePair<int, string>)AlarmAideTextList.SelectedItem).Key;
            var alarmTexts = _vm.SelectedAlarmAideConfig.AlarmTexts;
            alarmTexts[alarmAideTextId] = addAlarmAideTextDialog.AlarmText;
            _vm.SelectedAlarmAideConfig.AlarmTexts = _vm.SelectedAlarmAideConfig.AlarmTexts
                .CopyEdit(alarmAideTextId, alarmText => alarmText = alarmAideText);
            _vm.SelectedAlarmText = new KeyValuePair<int, string>(alarmAideTextId, alarmAideText);
            ConfigExtensions.SaveConfig(AlarmAideTextList);
        }
    }

    /// <summary>
    /// 闹钟助手内容列表-删除
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AlarmAideTextList_OnDelete(object sender, RoutedEventArgs e)
    {
        if (!MainWindowUtil.ShowDeleteDialog())
            return;

        var alarmAideTextList = AlarmAideTextList;
        var alarmTexts = _vm.SelectedAlarmAideConfig.AlarmTexts;
        foreach (KeyValuePair<int, string> item in alarmAideTextList.SelectedItems)
            alarmTexts.TryRemove(item.Key, out _);

        _vm.SelectedAlarmAideConfig.AlarmTexts = new ConcurrentDictionary<int, string>(alarmTexts);
        ConfigExtensions.SaveConfig();
        AlarmAideTextList.Focus();
    }

    /// <summary>
    /// 闹钟助手内容列表-测试发送
    /// </summary>
    private async void AlarmAideTextList_OnTest(object sender, RoutedEventArgs e)
    {
        var alarmText = _vm.SelectedAlarmText.Value;
        var selectedSetConfig = _vm.SelectedSetConfig;
        switch (selectedSetConfig.TargetType)
        {
            case BotConfigTargetType.Group:
                await BotServer.SendGroupMessageAsync(selectedSetConfig.TargetId, alarmText, _vm.SetConfigs).ConfigureAwait(false);
                break;
            case BotConfigTargetType.Private:
                await BotServer.SendPrivateMessageAsync(selectedSetConfig.TargetId, alarmText).ConfigureAwait(false);
                break;
            case BotConfigTargetType.Common:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// 闹钟助手内容列表-导出
    /// </summary>
    private void AlarmAideTextList_OnExport(object sender, RoutedEventArgs e)
    {
        var saveFileDialog = new SaveFileDialog
        {
            Title = "导出配置",
            FileName = _vm.SelectedAlarmAideConfig.AlarmName,
            Filter = "文本文件(*.txt)|*.txt",
            OverwritePrompt = true,
            CheckPathExists = true,
            DefaultExt = "txt",
        };
        if (saveFileDialog.ShowDialog() == true)
            File.WriteAllLines(saveFileDialog.FileName, _vm.SelectedAlarmAideConfig.AlarmTexts.Values.ToArray());
    }

    /// <summary>
    /// 闹钟助手内容列表-导入
    /// </summary>
    private void AlarmAideTextList_OnImport(object sender, RoutedEventArgs e)
    {
        var opneFileDialog = new OpenFileDialog
        {
            Title = "导入配置",
            Filter = "文本文件(*.txt)|*.txt",
            CheckPathExists = true,
            CheckFileExists = true,
            DefaultExt = "txt",
        };
        if (opneFileDialog.ShowDialog() != true)
            return;

        try
        {
            var alarmTexts = _vm.SelectedAlarmAideConfig.AlarmTexts;
            var sr = new StreamReader(opneFileDialog.FileName);
            string line;
            var importFailedCount = 0;
            var importCount = 0;

            // 从文件读取并显示行，直到文件的末尾
            while ((line = sr.ReadLine()) != null)
            {
                importCount++;
                if (alarmTexts.Values.Any(each => each == line))
                {
                    importFailedCount++;
                    continue;
                }

                alarmTexts.TryAdd(alarmTexts.GetSequence(), line);
            }

            _vm.SelectedAlarmAideConfig.AlarmTexts = new ConcurrentDictionary<int, string>(alarmTexts);

            sr.Close();
            MessageBox.Show($"共导入 {importCount} 条记录!{(importFailedCount > 0 ? $"{ENTER}其中 {importFailedCount} 条为重复项目, 忽略导入" : string.Empty)}");
        }
        catch (Exception)
        {
            // 向用户显示出错消息
            MessageBox.Show("无法读取文件!");
        }
        File.WriteAllLines(opneFileDialog.FileName, _vm.SelectedAlarmAideConfig.AlarmTexts.Values.ToArray());
    }

    /// <summary>
    /// 闹钟助手内容列表-按文本内容排序
    /// </summary>
    private void AlarmAideTextList_OnSort(object sender, RoutedEventArgs e)
    {
        var index = 0;
        var alarmTexts = _vm.SelectedAlarmAideConfig.AlarmTexts
            .OrderBy(each => each.Value)
            .Select(each => new KeyValuePair<int, string>(index++, each.Value))
            .ToArray();
        _vm.SelectedAlarmAideConfig.AlarmTexts = new ConcurrentDictionary<int, string>(alarmTexts);
        _vm.SelectedAlarmText = new KeyValuePair<int, string>(0, string.Empty);
        ConfigExtensions.SaveConfig();
    }

    /// <summary>
    /// 启用/非启用时保存
    /// </summary>
    private void List_OnEnable(object sender, RoutedEventArgs e) => ConfigExtensions.SaveConfig();
}