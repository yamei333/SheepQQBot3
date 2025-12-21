using Masuit.Tools;
using MessagePack;
using SheepQQBot3.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// Bot配置类
/// </summary>
public class BotConfig
{
    /// <summary>
    ///
    /// </summary>
    [JsonIgnore]
    [IgnoreMember]
    public SetConfig this[BotConfigTargetType targetType, string targetId]
        => SetConfigs.Values.FirstOrDefault(each => each.TargetType == targetType && each.TargetId == targetId);

    /// <summary>
    /// 群,个人等配置
    /// </summary>
    [JsonPropertyName(nameof(SetConfigs))]
    public Dictionary<Guid, SetConfig> SetConfigs { get; set; } = [];

    /// <summary>
    /// 用户配置(原神cookie, barkKey等)
    /// </summary>
    [JsonPropertyName(nameof(UserConfigs))]
    public Dictionary<string, Dictionary<UserConfigType, string>> UserConfigs { get; set; } = [];

    /// <summary>
    /// 保存自定义提醒内容
    /// </summary>
    [JsonPropertyName(nameof(CustomAlarms))]
    public Dictionary<Guid, CustomAlarm> CustomAlarms { get; set; } = [];

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public BotConfig()
    {
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