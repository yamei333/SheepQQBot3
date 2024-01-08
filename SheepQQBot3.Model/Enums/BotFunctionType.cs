using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BotFunctionType
{
    [Display(Name = "闹钟助手")]
    Common_AlarmAide = 0,

    [Display(Name = "闹钟助手投稿")]
    Common_AlarmAideSubmit = 1,

    [Display(Name = "黑名单")]
    Common_BlackList = 2,

    [Display(Name = "自定义提醒")]
    Common_CustomAlarm = 3,

    [Display(Name = "KEY配置")]
    Common_KeyConfig = 4,

    [Display(Name = "私聊配置")]
    Private_AdminConfig = 1000,

    [Display(Name = "复读机杀手")]
    Group_RepeaterKiller = 2000,

    [Display(Name = "复读撤回消息")]
    Group_RepeatRevokeMessage = 2002,

    [Display(Name = "基金助手")]
    Group_FundHelper = 2003,

    [Display(Name = "随机色图")]
    Group_RandomSetu = 2004,

    [Display(Name = "直播提醒")]
    Group_LiveAlarm = 2005,

    [Display(Name = "图源搜索")]
    Group_SearchImageSource = 2006,

    [Display(Name = "ROLL点")]
    Group_Roll = 2007,

    [Display(Name = "群聊总结")]
    Group_ChatSummary = 2008,

    [Display(Name = "原神助手")]
    Group_GenshinHelper = 2200,
}

public static class BotFunctionTypeExtensions
{
    /// <summary>
    /// 所有显示TAB的功能
    /// </summary>
    /// <returns></returns>
    public static HashSet<BotFunctionType> GetTabFunctions()
        => new()
        {
            BotFunctionType.Common_AlarmAide,
            BotFunctionType.Common_AlarmAideSubmit,
            BotFunctionType.Common_BlackList,
            BotFunctionType.Group_RepeaterKiller,
            BotFunctionType.Group_FundHelper,
            BotFunctionType.Group_LiveAlarm,
            BotFunctionType.Group_GenshinHelper,
        };
}