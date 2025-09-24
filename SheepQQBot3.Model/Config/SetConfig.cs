using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Model.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// 配置类
/// </summary>
public class SetConfig
{
    /// <summary>
    /// 默认BotFunction
    /// </summary>
    [JsonIgnore]
    public static readonly List<BotFunction> DefaultBotFunctions = Enum.GetNames(typeof(BotFunctionType))
        .Select(each => new BotFunction((BotFunctionType)Enum.Parse(typeof(BotFunctionType), each), false))
        .ToList();

    /// <summary>
    /// 配置中的功能
    /// </summary>
    [JsonPropertyName(nameof(BotFunctions))]
    public List<BotFunction> BotFunctions { get; set; }

    /// <summary>
    /// 配置ID
    /// </summary>
    [JsonPropertyName(nameof(Id))]
    public Guid Id { get; set; }

    /// <summary>
    /// 配置显示的图标
    /// 头像大图链接 https://q1.qlogo.cn/g?b=qq&nk={QQ号}252961222}&s=640
    /// </summary>
    [JsonIgnore]
    public BitmapFrame Icon => TargetType switch
    {
        BotConfigTargetType.Common => QQExtensions.GetQQImage(AppSettingExtensions.Get("selfId", 0L)),
        BotConfigTargetType.Group => QQExtensions.GetQQGroupImage(TargetId),
        BotConfigTargetType.Private => QQExtensions.GetQQImage(TargetId),
        _ => QQExtensions.GetQQImage(10000),
    };

    /// <summary>
    /// 对象类型
    /// </summary>
    [JsonPropertyName(nameof(TargetType))]
    public BotConfigTargetType TargetType { get; set; }

    /// <summary>
    /// 闹钟助手配置
    /// </summary>
    [JsonPropertyName(nameof(AlarmAideConfigs))]
    public ConcurrentDictionary<Guid, AlarmAideConfig> AlarmAideConfigs { get; set; }

    /// <summary>
    /// 闹钟助手允许投稿成员ID配置
    /// </summary>
    [JsonPropertyName(nameof(AlarmAideSubmitMemberIds))]
    public HashSet<long> AlarmAideSubmitMemberIds { get; set; }

    /// <summary>
    /// 黑名单ID配置
    /// </summary>
    [JsonPropertyName(nameof(BlackListIds))]
    public HashSet<long> BlackListIds { get; set; }

    /// <summary>
    /// 基金播报配置
    /// </summary>
    [JsonPropertyName(nameof(FundAlarmConfigs))]
    public ConcurrentDictionary<Guid, FundAlarmConfig> FundAlarmConfigs { get; set; }

    /// <summary>
    /// 基金阈值观测配置
    /// </summary>
    [JsonPropertyName(nameof(FundLimitObserveConfigs))]
    public ConcurrentDictionary<Guid, FundLimitObserveConfig> FundLimitObserveConfigs { get; set; }

    /// <summary>
    /// 复读机杀手配置
    /// </summary>
    [JsonPropertyName(nameof(RepeaterKillerConfigs))]
    public ConcurrentDictionary<Guid, RepeaterKillerConfig> RepeaterKillerConfigs { get; set; }

    /// <summary>
    /// 直播提醒配置
    /// </summary>
    [JsonPropertyName(nameof(LiveAlarmConfigs))]
    public ConcurrentDictionary<Guid, LiveAlarmConfig> LiveAlarmConfigs { get; set; }

    /// <summary>
    /// AI群响应配置
    /// </summary>
    [JsonPropertyName(nameof(AIGroupConfig))]
    public AIGroupConfig AIGroupConfig { get; set; }

    /// <summary>
    /// 显示文字
    /// </summary>
    [JsonIgnore]
    public string DisplayId =>
        TargetType switch
        {
            BotConfigTargetType.Common => $"系统-{TargetId}",
            BotConfigTargetType.Group => $"群-{TargetId}",
            BotConfigTargetType.Private => $"{TargetId}",
            _ => throw new NotImplementedException(),
        };

    /// <summary>
    /// 对象ID(群号/个人QQ号)
    /// </summary>
    [JsonPropertyName(nameof(TargetId))]
    public long TargetId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [JsonPropertyName(nameof(TargetName))]
    public string TargetName { get; set; }

    public SetConfig(Guid id, BotConfigTargetType targetType, long targetId, string targetName)
    {
        Id = id;
        TargetType = targetType;
        TargetId = targetId;
        TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
        BotFunctions = DefaultBotFunctions.DeepClone();
        AlarmAideConfigs = [];
        BlackListIds = [];
        AlarmAideSubmitMemberIds = [];
        FundAlarmConfigs = [];
        FundLimitObserveConfigs = [];
        RepeaterKillerConfigs = [];
        LiveAlarmConfigs = [];
        AIGroupConfig = new AIGroupConfig();
        InitBotFunctionIsEnabled();
#if (!debug)
#else
            //AlarmAideConfigs = new List<AlarmAideConfig>()
            //{
            //    new AlarmAideConfig()
            //    {
            //        AlarmName = "zap",
            //        LoopDays = "12345",
            //        Condition = "111",
            //        Id =  Guid.NewGuid(),
            //        IsActive = true,
            //        AlarmTexts = new List<string> {"zap1","zap2"},
            //    },
            //    new AlarmAideConfig()
            //    {
            //        AlarmName = "zap",
            //        LoopDays = "12345",
            //        Condition = "111",
            //        Id =  Guid.NewGuid(),
            //        IsActive = false,
            //        AlarmTexts = new List<string> {"zap1","zap2"},
            //    }
            //};
#endif
    }

    /// <summary>
    /// 初始化BotFuntion可用状态
    /// </summary>
    internal void InitBotFunctionIsEnabled()
    {
        var allowFunctions = TargetType.GetAllowFunctions();
        BotFunctions.ForEach(each => each.IsEnabled = allowFunctions.Contains(each.BotFunctionType));
    }
}