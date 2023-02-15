using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.SDK.Client;
using SheepQQBot3.SDK.Event;

namespace SheepQQBot3.View
{
    /// <summary>
    /// ViewModel
    /// </summary>
    public partial class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        private List<BotFunction> _selectedSetBotFunctions;
        private Dictionary<BotFunctionType, bool> _isVisibleTabPages;

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
        /// 值变化时调用, 用于通知界面
        /// </summary>
        /// <param name="propertyName">属性名</param>
        public void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// 初始化
        /// </summary>
        public MainWindowViewModel()
        {
            PublicVar.InitPublicVar(this);
            Title = "助手哈莉 - Ver 0.4.3.0";
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
        }

        public MainWindowRunlogViewModel MainWindowRunlogViewModel { get; set; }
        public MainWindowAlarmAideViewModel MainWindowAlarmAideViewModel { get; set; }
        public MainWindowAlarmAideSubmitViewModel MainWindowAlarmAideSubmitViewModel { get; set; }
        public MainWindowFundHelperViewModel MainWindowFundHelperViewModel { get; set; }
        public MainWindowRepeaterKillerViewModel MainWindowRepeaterKillerViewModel { get; set; }

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

        public bool IsVisibleAlarmAideSubmit => GetTabVisible(BotFunctionType.Common_AlarmAideSubmit);

        public bool IsVisibleFundHelper => GetTabVisible(BotFunctionType.Group_FundHelper);

        public bool IsVisibleGroupGroupAlarm => GetTabVisible(BotFunctionType.Group_CustomGroupAlarm);

        public bool IsVisibleGroupRepeaterKiller => GetTabVisible(BotFunctionType.Group_RepeaterKiller);

        public Dictionary<BotFunctionType, bool> IsVisibleTabPages
        {
            get => _isVisibleTabPages;
            set
            {
                _isVisibleTabPages = value;
                OnPropertyChanged(nameof(IsVisibleGroupRepeaterKiller));
                OnPropertyChanged(nameof(IsVisibleGroupGroupAlarm));
                OnPropertyChanged(nameof(IsVisibleAlarmAideSubmit));
                OnPropertyChanged(nameof(IsVisibleFundHelper));
            }
        }

        /// <summary>
        /// 选中群的功能配置
        /// </summary>
        public List<BotFunction> SelectedSetBotFunctions
        {
            get => _selectedSetBotFunctions;
            set
            {
                _selectedSetBotFunctions = value;
                IsVisibleTabPages = _selectedSetBotFunctions
                    .ToDictionary(each => each.BotFunctionType, each => each.IsUsed);
                OnPropertyChanged(nameof(SelectedSetBotFunctions));
            }
        }

        private bool GetTabVisible(BotFunctionType botFunctionType)
        {
            var isVisibleTabPages = IsVisibleTabPages;
            if (isVisibleTabPages?.Any() == true)
                return isVisibleTabPages[botFunctionType] == true;

            return false;
        }
    }
}