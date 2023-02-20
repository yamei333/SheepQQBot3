using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using Yamei.Common;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //private readonly BotFunction[] DefaultBotFunctions;

        public MainWindow()
        {
            BotExtensions.KillGocqexe();
            //DefaultBotFunctions = Enum.GetNames(typeof(BotFunctionType))
            //    .Select(each => new BotFunction((BotFunctionType)Enum.Parse(typeof(BotFunctionType), each), false))
            //    .ToArray();
            InitializeComponent();
        }

        private void MainWindow_OnInitialized(object sender, EventArgs e)
        {
            // MEMO : 界面初始化时读取配置
            ConfigExtensions.LoadConfig();
            MWindow = this;

            // MEMO : 获得节假日配置
            GetHolidayInfo();

            void GetHolidayInfo()
            {
                var nowYear = DateTime.Now.ToString("yyyy");
                var holidayInfoPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\{nowYear}.txt";
                string holidayInfoJson;
                if (File.Exists(holidayInfoPath))
                {
                    holidayInfoJson = File.ReadAllText(holidayInfoPath, Encoding.UTF8);
                }
                else
                {
                    holidayInfoJson = HttpExtensions.HttpGetString($"https://timor.tech/api/holiday/year/{nowYear}/");
                    File.WriteAllLines(holidayInfoPath, new[] { holidayInfoJson }, Encoding.UTF8);
                }

                var regHolidayInfo = RegexGenerator.HolidayInfo();
                var holidayInfo = new Dictionary<string, bool>();
                regHolidayInfo.Matches(holidayInfoJson).ForEach(each =>
                {
                    var holidayInfoData = JsonSerializer.Deserialize<HolidayInfoData>(each.Value);
                    if (holidayInfoData != null)
                        holidayInfo.Add(holidayInfoData.Date, holidayInfoData.Holiday);
                });

                PublicVar.HolidayInfo = holidayInfo;
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            GocqEmbedWindow = new GocqWindow();
            GocqEmbedWindow.Show();
            Vm.AddRunLog(new RunLog_SystemInfo("助手哈莉 初始化完成"));
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
            if (vm.SetBotFunctions.TryGetValue((selectedSetConfig.TargetType, selectedSetConfig.TargetId), out var botFunctions))
            {
                botFunctions = vm.SelectedSetBotFunctions;
            }
            else
            {
                vm.SetBotFunctions.Add((selectedSetConfig.TargetType, selectedSetConfig.TargetId), DefaultBotFunctions);
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
                TargetType = selectedSetConfig.TargetType
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

        /// <summary>
        /// 启用/非启用时保存
        /// </summary>
        private void List_OnEnable(object sender, RoutedEventArgs e) => ConfigExtensions.SaveConfig();

        //private void AlarmAideTextList_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (Vm.SelectedAlarmAideConfig == null)
        //        e.Handled = true;
        //}

        private void GroupList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GroupList.SelectedItems.Count == 1)
                GroupConfig_OnEdit(new MenuItem { Header = "修改" }, e);
        }

        private void OnNotifyIconDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
            this.Activate();
        }

        private void NotifyIcon_OnExit(object sender, RoutedEventArgs e)
        {
            BotExtensions.KillGocqexe();
            Application.Current.Shutdown();
        }

        private void NotifyIcon_OnShowMainWindow(object sender, RoutedEventArgs e)
            => OnNotifyIconDoubleClick(sender, e);

        private void NotifyIcon_OnShowGocqWindow(object sender, RoutedEventArgs e)
        {
            GocqEmbedWindow.Visibility = Visibility.Visible;
            GocqEmbedWindow.Activate();
            GocqEmbedWindow.GocqEmbedWindow.Focus();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            e.Cancel = true;
        }
    }
}