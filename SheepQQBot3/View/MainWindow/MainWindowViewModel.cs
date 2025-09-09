using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.BotService;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.SDK.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.View;

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
    /// 监听消息用 <see cref="SDK.Server.BotServer"/>
    /// </summary>
    public BotServer BotServer { get; set; }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        BotServer?.Dispose();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public MainWindowViewModel()
    {
        InitPublicVar(this);
        // 获取当前程序集的版本号
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        Title = $"{BOT_NAME} - Ver {version}";
        IsLoadComplete = false;
        IsBarkUsed = !string.IsNullOrEmpty(AppSettingExtensions.Get("bark"));

        ConfigExtensions.LoadConfig();
        ConfigExtensions.LoadAIConfig();
        ConfigExtensions.LoadAIData();
        ConfigExtensions.LoadAIAICharacter();
        InitViewModel();
        AddRunLog(new RunLog_SystemInfo($"{BOT_NAME} 初始化..."));

        JiebaDb.Dicts.Where(dict => dict.IsDefault == 0).ForEach(each => SegmenterExtensions.AddWord(each.Word));
        AddRunLog(new RunLog_SystemInfo("JiebaDb Dict 加载完成"));

#if (!debug)
        InitServer();
        //InitWcfService();
        WebApiProcess.InitWebApi();
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
        MainWindowAiConfigModel = new MainWindowAiConfigModel();
    }

    public MainWindowRunlogViewModel MainWindowRunlogViewModel { get; set; }
    public MainWindowAlarmAideViewModel MainWindowAlarmAideViewModel { get; set; }
    public MainWindowAlarmAideSubmitViewModel MainWindowAlarmAideSubmitViewModel { get; set; }
    public MainWindowFundHelperViewModel MainWindowFundHelperViewModel { get; set; }
    public MainWindowRepeaterKillerViewModel MainWindowRepeaterKillerViewModel { get; set; }
    public MainWindowBlackListViewModel MainWindowBlackListViewModel { get; set; }
    public MainWindowLiveAlarmViewModel MainWindowLiveAlarmViewModel { get; set; }
    public MainWindowAiConfigModel MainWindowAiConfigModel { get; set; }

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
            MainWindowLiveAlarmViewModel?.OnPropertyChanged(nameof(SelectedSetConfig));
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
    /// BarkServer是否启用
    /// </summary>
    public bool IsBarkUsed { get; }

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