using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MessagePack;
using SheepQQBot3.Model.Enums;
using Yamei.Common;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// Bot配置类
/// </summary>
[MessagePackObject]
public class BotConfig
{
    [JsonIgnore]
    [IgnoreMember]
    private Dictionary<Guid, SetConfig> _setConfigs;

    /// <summary>
    /// 群,个人等配置
    /// </summary>
    [Key(nameof(SetConfigs))]
    public Dictionary<Guid, SetConfig> SetConfigs
    {
        get => _setConfigs ??= new Dictionary<Guid, SetConfig>();
        set => _setConfigs = value;
    }

    [JsonIgnore]
    [IgnoreMember]
    private Dictionary<long, Dictionary<UserConfigType, string>> _userConfigs;

    /// <summary>
    /// 用户配置(原神cookie, barkKey等)
    /// </summary>
    [Key(nameof(UserConfigs))]
    public Dictionary<long, Dictionary<UserConfigType, string>> UserConfigs
    {
        get => _userConfigs ??= new Dictionary<long, Dictionary<UserConfigType, string>>();
        set => _userConfigs = value;
    }

    [JsonIgnore]
    [IgnoreMember]
    private Dictionary<Guid, CustomAlarm> _customAlarms;

    /// <summary>
    /// 保存自定义提醒内容
    /// </summary>
    [Key(nameof(CustomAlarms))]
    public Dictionary<Guid, CustomAlarm> CustomAlarms
    {
        get => _customAlarms ??= new Dictionary<Guid, CustomAlarm>();
        set => _customAlarms = value;
    }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public BotConfig()
    {
        CustomAlarms = new Dictionary<Guid, CustomAlarm>();
    }

    /// <summary>
    /// 初始化BotFuntion可用状态, 顺便排序
    /// </summary>
    public void InitBotFunctionIsEnabled()
    {
        SetConfigs.Values.ForEach(each =>
        {
            each.InitBotFunctionIsEnabled();
            each.BotFunctions = each.BotFunctions.OrderBy(botFunc => (int)botFunc.BotFunctionType).ToList();
        });
    }
}