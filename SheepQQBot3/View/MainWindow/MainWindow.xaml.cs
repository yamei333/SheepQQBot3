using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.View;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// 默认构造函数
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MainWindow_OnInitialized(object sender, EventArgs e)
    {
        // MEMO : 界面初始化时读取配置
        //ConfigExtensions.LoadConfig();
        //ConfigExtensions.LoadAIConfig();
        //ConfigExtensions.LoadAIAICharacter();
        Vm.InitBotFunctions();
        MWindow = this;

        // MEMO : 获得节假日配置
        GetHolidayInfo();
        // MEMO : 初始化AI设定
        InitAIModel();

        async void GetHolidayInfo()
        {
            var nowYear = DateTime.Now.ToString("yyyy");
            var holidayInfoPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\{nowYear}.txt";
            string holidayInfoJson;
            if (File.Exists(holidayInfoPath))
            {
                holidayInfoJson = await File.ReadAllTextAsync(holidayInfoPath, Encoding.UTF8).ConfigureAwait(false);
            }
            else
            {
                holidayInfoJson = await HttpExtensions.CreateHttpClient()
                    .GetStringAsync($"https://timor.tech/api/holiday/year/{nowYear}/")
                    .ConfigureAwait(false);
                File.WriteAllLines(holidayInfoPath, [holidayInfoJson], Encoding.UTF8);
            }

            var regHolidayInfo = RegexGenerator.HolidayInfo();
            var holidayInfo = new Dictionary<string, bool>();
            regHolidayInfo.Matches(holidayInfoJson).ForEach(each =>
            {
                var holidayInfoData = each.Value.JsonDeserialize<HolidayInfoData>();
                if (holidayInfoData != null)
                    holidayInfo.Add(holidayInfoData.Date, holidayInfoData.Holiday);
            });

            HolidayInfo = holidayInfo;
        }

        void InitAIModel()
        {
            if (PublicVar.AIConfig.ApiKeys?.Any() != true)
            {
                LogExtensions.AddRunLog(new RunLog_SystemWarning("AI配置 未配置"));
                return;
            }

            LogExtensions.AddRunLog(new RunLog_SystemInfo("AI配置 初始化完成"));
            PublicVar.AIControl = new AIControl(PublicVar.AIConfig, PublicVar.AICharacter);
        }
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        this.Left -= int.MaxValue;
        this.Visibility = Visibility.Collapsed;
        // MEMO : NapCat
        PublicVar.NapCatWindow = new NapCatWindow
        {
            Visibility = Visibility.Collapsed,
            WindowStyle = WindowStyle.None,
        };
        PublicVar.NapCatWindow.Show();
        // MEMO : Bark
        PublicVar.BarkWindow = new BarkWindow
        {
            Visibility = Visibility.Collapsed,
            WindowStyle = WindowStyle.None,
        };
        PublicVar.BarkWindow.Show();
        Vm.AddRunLog(new RunLog_SystemInfo($"{BOT_NAME} 初始化完成"));
    }

    private void MainWindow_OnClosed(object sender, EventArgs e)
    {
        Vm.Dispose();
    }

    private void GroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        /*
        var selectionSetConfig = GroupList.SelectedItem as SetConfig;
        vm.SelectionSetConfig = selectionSetConfig;
        vm.SelectedSetBotFunctions = selectionSetConfig.BotFunctions.Select(each => each.Value).ToArray();*/
        if (Vm.SelectedSetConfig == null)
        {
            Vm.SelectedSetBotFunctions = new Dictionary<BotFunctionType, BotFunction>();
        }
        else
        {
            Vm.SelectedSetBotFunctions = Vm.SelectedSetConfig.BotFunctions
                .ToDictionary(each => each.BotFunctionType, each => each);
        }

        /*
        var groupList = Vm.GroupConfigList;
        groupList.Add(new GroupConfig(new Random().Next(1, 100), "testzap"));
        Vm.GroupConfigList = new List<GroupConfig>(groupList);*/
    }

    private void BotFunctionCheckBox_CheckedChange(object sender, RoutedEventArgs e)
    {
        ConfigExtensions.SaveConfig();
        Vm.OnPropertyChanged(nameof(Vm.IsTabVisible));

        /*
        var selectionSetConfig = vm.SelectionSetConfig;
        foreach (BotFunction botFunction in UsedFunctionList.Items)
            selectionSetConfig.BotFunctions[botFunction.BotFunctionType].IsUsed = botFunction.IsUsed;

        vm.SelectedSetConfig = selectionSetConfig;*/

        /*
        var selectedSetConfig = vm.SelectedSetConfig;
        if (vm.SetBotFunctions.TryGetValue((selectedSetConfig.MessageTargetType, selectedSetConfig.TargetId), out var botFunctions))
        {
            botFunctions = vm.SelectedSetBotFunctions;
        }
        else
        {
            vm.SetBotFunctions.Add((selectedSetConfig.MessageTargetType, selectedSetConfig.TargetId), DefaultBotFunctions);
        }*/
    }

    private void TabItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var tabItem = (TabItem)sender;
        var tabItems = TabBotFunctions.Items;
        var visibleItemNum = 0;
        TabItem visibleTabItem = null;
        foreach (TabItem item in tabItems)
        {
            if (item.Visibility != Visibility.Visible)
                continue;

            visibleTabItem = item;
            visibleItemNum++;
        }

        if (visibleItemNum == 1)
        {
            visibleTabItem.IsSelected = true;
            return;
        }

        if (tabItem.Visibility != Visibility.Visible)
        {
            foreach (TabItem item in tabItems)
            {
                if (item.Visibility == Visibility.Visible)
                {
                    item.IsSelected = true;
                    return;
                }
            }
        }
    }

    //private void AlarmAideTextAdd_OnClick(object sender, RoutedEventArgs e)
    //{
    //    var addAlarmAideText = Vm.AddAlarmAideText;
    //    if (Vm.SelectedAlarmAideConfig.AlarmTexts.Contains(addAlarmAideText))
    //    {
    //        MessageBox.Show("已存在相同内容!");
    //        return;
    //    }

    //    var newAlarmTexts = new List<string>(Vm.SelectedAlarmAideConfig.AlarmTexts) { addAlarmAideText };
    //    Vm.SelectedAlarmAideConfig.AlarmTexts = newAlarmTexts;
    //    Vm.AddAlarmAideText = string.Empty;
    //}

    /// <summary>
    /// 群列表-新增
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GroupConfig_OnAdd(object sender, RoutedEventArgs e)
    {
        var addGroupDialog = new AddGroupDialog(this, sender, DialogMode.Add);
        if (addGroupDialog.ShowDialog() == true)
        {
            var setConfigs = Vm.SetConfigs;
            var targetId = addGroupDialog.TargetId;
            var newId = Guid.NewGuid();
            setConfigs.Add(newId, new SetConfig(newId, addGroupDialog.TargetType, targetId, addGroupDialog.TargetName));
            Vm.SetConfigs = new Dictionary<Guid, SetConfig>(setConfigs);
        }
    }

    /// <summary>
    /// 群列表-修改
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GroupConfig_OnEdit(object sender, RoutedEventArgs e)
    {
        var selectedSetConfig = Vm.SelectedSetConfig;
        var addGroupDialog = new AddGroupDialog(this, sender, DialogMode.Edit)
        {
            TargetName = selectedSetConfig.TargetName,
            TargetId = selectedSetConfig.TargetId,
            TargetType = selectedSetConfig.TargetType,
        };
        if (addGroupDialog.ShowDialog() == true)
        {
            var setConfigs = Vm.SetConfigs;
            var newSelectedSetConfig = setConfigs[selectedSetConfig.Id];
            newSelectedSetConfig.TargetType = addGroupDialog.TargetType;
            newSelectedSetConfig.TargetId = addGroupDialog.TargetId;
            newSelectedSetConfig.TargetName = addGroupDialog.TargetName;
            Vm.SetConfigs = new Dictionary<Guid, SetConfig>(setConfigs);
            Vm.SelectedSetConfig = newSelectedSetConfig;
        }
    }

    /// <summary>
    /// 群列表-删除
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GroupConfig_OnDelete(object sender, RoutedEventArgs e)
    {
        if (!MainWindowUtil.ShowDeleteDialog())
            return;

        var setConfigs = Vm.SetConfigs;
        setConfigs.Remove(Vm.SelectedSetConfig.Id);
        Vm.SetConfigs = new Dictionary<Guid, SetConfig>(setConfigs);
    }

    private void GroupList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (GroupList.SelectedItems.Count == 1)
            GroupConfig_OnEdit(new MenuItem { Header = "修改" }, e);
    }

    private void OnNotifyIconDoubleClick(object sender, RoutedEventArgs e)
    {
        if (this.Width <= 100)
        {
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            Width = 1000;
            Height = 532;
            // 获取 DPI 缩放比例
            var matrix = PresentationSource.FromVisual(this)!.CompositionTarget!.TransformToDevice;
            var dpiFactor = 1 / matrix.M11;
            // 计算居中位置,设置窗口位置
            this.Left = (SystemParameters.PrimaryScreenWidth - this.Width * dpiFactor) / 2;
            this.Top = (SystemParameters.PrimaryScreenHeight - this.Height * dpiFactor) / 2;
        }
        this.ShowInTaskbar = true;
        this.Visibility = Visibility.Visible;
        this.Show();
    }

    private void NotifyIcon_OnExit(object sender, RoutedEventArgs e)
    {
        // MEMO : 结束时不再关闭Server, 以处理历史消息
        // MEMO : debug时还是关闭(为了保持Server进程关闭)
        // MEMO : 0.14.0.1 结束时还是关闭, 暂时不处理中断时的历史消息
        BotExtensions.KillServerExe();
        BotExtensions.KillBarkExe();
        ConfigExtensions.SaveConfig();
        Environment.Exit(0);
    }

    private void NotifyIcon_OnShowMainWindow(object sender, RoutedEventArgs e)
        => OnNotifyIconDoubleClick(sender, e);

    private void NotifyIcon_OnShowNapCatWindow(object sender, RoutedEventArgs e)
    {
        PublicVar.NapCatWindow.Visibility = Visibility.Visible;
        PublicVar.NapCatWindow.Activate();
    }

    private void NotifyIcon_OnShowBarkWindow(object sender, RoutedEventArgs e)
    {
        PublicVar.BarkWindow.Visibility = Visibility.Visible;
        PublicVar.BarkWindow.Activate();
    }

    private void MainWindow_OnClosing(object sender, CancelEventArgs e)
    {
        this.Visibility = Visibility.Collapsed;
        e.Cancel = true;
    }
}