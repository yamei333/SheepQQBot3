using System;
using System.Collections.Generic;
using System.Linq;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.SDK.Client;
using SheepQQBot3.SDK.Event;

namespace SheepQQBot3.View
{
    /// <summary>
    /// ViewModel
    /// </summary>
    public partial class MainWindowViewModel : NotifyPropertyChangedBase, IDisposable
    {
        /// <summary>
        /// 是否读取完成
        /// </summary>
        public bool IsLoadComplete { get; set; }

        /// <summary>
        /// 监听API用 <see cref="CQAPI"/>
        /// </summary>
        public CQAPI CqApi { get; set; }

        /// <summary>
        /// 监听Event用 <see cref="CQEvent"/>
        /// </summary>
        public CQEvent CqEvent { get; set; }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            CqApi?.Dispose();
            CqEvent?.Dispose();
            //_serviceHost?.Close();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public MainWindowViewModel()
        {
            PublicVar.InitPublicVar(this);
            Title = "助手哈莉 - Ver 0.6.4.2";
            IsLoadComplete = false;

            InitViewModel();
            AddRunLog(new RunLog_SystemInfo("助手哈莉 初始化..."));

#if (!debug)
            InitApi();
            InitEvent();
            //InitWcfService();
            InitBotFunctions();
#endif
        }

        /// <summary>
        /// 各种子ViewModel初始化
        /// </summary>
        public void InitViewModel()
        {
            MainWindowRunlogViewModel = new MainWindowRunlogViewModel();
            MainWindowAlarmAideViewModel = new MainWindowAlarmAideViewModel();
            MainWindowAlarmAideSubmitViewModel = new MainWindowAlarmAideSubmitViewModel();
            MainWindowFundHelperViewModel = new MainWindowFundHelperViewModel();
            MainWindowRepeaterKillerViewModel = new MainWindowRepeaterKillerViewModel();
            MainWindowBlackListViewModel = new MainWindowBlackListViewModel();
            MainWindowLiveAlarmViewModel = new MainWindowLiveAlarmViewModel();
            MainWindowGenshinHelperViewModel = new MainWindowGenshinHelperViewModel();
        }

        public MainWindowRunlogViewModel MainWindowRunlogViewModel { get; set; }
        public MainWindowAlarmAideViewModel MainWindowAlarmAideViewModel { get; set; }
        public MainWindowAlarmAideSubmitViewModel MainWindowAlarmAideSubmitViewModel { get; set; }
        public MainWindowFundHelperViewModel MainWindowFundHelperViewModel { get; set; }
        public MainWindowRepeaterKillerViewModel MainWindowRepeaterKillerViewModel { get; set; }
        public MainWindowBlackListViewModel MainWindowBlackListViewModel { get; set; }
        public MainWindowLiveAlarmViewModel MainWindowLiveAlarmViewModel { get; set; }
        public MainWindowGenshinHelperViewModel MainWindowGenshinHelperViewModel { get; set; }

        public Dictionary<(BotConfigTargetType, long), BotFunction[]> SetBotFunctions { get; set; }

        private SetConfig _selectedSetConfig;
        /// <summary>
        /// 当前选中的配置(群/个人)
        /// 后台->前台, 设定用
        /// </summary>
        public SetConfig SelectedSetConfig
        {
            get => _selectedSetConfig;
            set
            {
                _selectedSetConfig = value;
                OnPropertyChanged(nameof(SelectedSetConfig));
                MainWindowAlarmAideViewModel?.OnPropertyChanged(nameof(SelectedSetConfig));
                MainWindowAlarmAideSubmitViewModel?.OnPropertyChanged(nameof(SelectedSetConfig));
                MainWindowFundHelperViewModel?.OnPropertyChanged(nameof(SelectedSetConfig));
                MainWindowRepeaterKillerViewModel?.OnPropertyChanged(nameof(SelectedSetConfig));
                MainWindowBlackListViewModel?.OnPropertyChanged(nameof(SelectedSetConfig));
            }
        }

        private Dictionary<Guid, SetConfig> _setConfigs;
        /// <summary>
        /// 群名的列表
        /// </summary>
        public Dictionary<Guid, SetConfig> SetConfigs
        {
            get => _setConfigs;
            set
            {
                _setConfigs = value;
                OnPropertyChanged(nameof(SetConfigs));
                ConfigExtensions.SaveConfig();
            }
        }

        private string _title;

        /// <summary>
        /// 窗体标题
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        private Dictionary<BotFunctionType, BotFunction> _selectedSetBotFunctions;
        /// <summary>
        /// 选中群的功能配置
        /// </summary>
        public Dictionary<BotFunctionType, BotFunction> SelectedSetBotFunctions
        {
            get => _selectedSetBotFunctions;
            set
            {
                _selectedSetBotFunctions = value;
                OnPropertyChanged(nameof(SelectedSetBotFunctions));
                OnPropertyChanged(nameof(IsTabVisible));
            }
        }

        /// <summary>
        /// 是否有任意Tab页在显示中
        /// </summary>
        public bool IsTabVisible
        {
            get
            {
                var tabFunctions = BotFunctionTypeExtensions.GetTabFunctions();
                return SelectedSetBotFunctions?.Values
                    .Where(each => tabFunctions.Contains(each.BotFunctionType))
                    .Any(each => each.IsUsed) == true;
            }
        }
    }
}