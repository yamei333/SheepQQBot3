using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Enums;

/// <summary>
/// 配置目标类型
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BotConfigTargetType
{
    /// <summary>
    /// 系统
    /// </summary>
    [Display(Name = "系统消息")]
    Common = 0,

    /// <summary>
    /// 群
    /// </summary>
    [Display(Name = "群消息")]
    Group = 1,

    /// <summary>
    /// 个人
    /// </summary>
    [Display(Name = "私聊消息")]
    Private = 2,
}

public static class TargetTypeExtensions
{
    /// <summary>
    /// 获得各种类型允许使用功能
    /// </summary>
    /// <param name="botConfigTargetType"><see cref="BotConfigTargetType"/></param>
    /// <returns>允许使用功能</returns>
    public static HashSet<BotFunctionType> GetAllowFunctions(this BotConfigTargetType botConfigTargetType)
    {
        return botConfigTargetType switch
        {
            BotConfigTargetType.Common => new HashSet<BotFunctionType>
            {
                BotFunctionType.Common_AlarmAide,
                BotFunctionType.Common_BlackList,
                BotFunctionType.Common_CustomAlarm,
                BotFunctionType.Common_KeyConfig,
            },
            BotConfigTargetType.Group => new HashSet<BotFunctionType>
            {
                BotFunctionType.Common_AlarmAide,
                BotFunctionType.Common_AlarmAideSubmit,
                BotFunctionType.Common_BlackList,
                BotFunctionType.Common_CustomAlarm,
                BotFunctionType.Group_RepeatRevokeMessage,
                BotFunctionType.Group_FundHelper,
                BotFunctionType.Group_RandomSetu,
                BotFunctionType.Group_RepeaterKiller,
                BotFunctionType.Group_LiveAlarm,
                BotFunctionType.Group_SearchImageSource,
                BotFunctionType.Group_Roll,
                BotFunctionType.Group_GenshinHelper,
            },
            BotConfigTargetType.Private => new HashSet<BotFunctionType>
            {
                BotFunctionType.Private_AdminConfig
            },
            _ => null
        };
    }
}