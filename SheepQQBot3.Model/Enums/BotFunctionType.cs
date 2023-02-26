using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BotFunctionType
    {
        Common_AlarmAide = 0,
        Common_AlarmAideSubmit = 1,
        Common_BlackList = 2,
        Private_AdminConfig = 1000,
        Group_RepeaterKiller = 2000,
        Group_CustomGroupAlarm = 2001,
        Group_RepeatRevokeMessage = 2002,
        Group_FundHelper = 2003,
        Group_RandomSetu = 2004,
        Group_LiveAlarm = 2005,
    }

    public static class BotFunctionTypeExtensions
    {
        public static string ToFunctionName(this BotFunctionType botFunctionType)
        {
            return botFunctionType switch
            {
                BotFunctionType.Common_AlarmAide => "闹钟助手",
                BotFunctionType.Common_AlarmAideSubmit => "闹钟助手投稿",
                BotFunctionType.Common_BlackList => "黑名单",
                BotFunctionType.Private_AdminConfig => "私聊配置",
                BotFunctionType.Group_RepeaterKiller => "复读机杀手",
                BotFunctionType.Group_CustomGroupAlarm => "群提醒",
                BotFunctionType.Group_RepeatRevokeMessage => "复读撤回消息",
                BotFunctionType.Group_FundHelper => "基金助手",
                BotFunctionType.Group_RandomSetu => "随机色图",
                BotFunctionType.Group_LiveAlarm => "直播提醒",
                _ => throw new System.NotImplementedException()
            };
        }

        public static HashSet<BotFunctionType> GetTabFunctions()
            => new()
            {
                BotFunctionType.Common_AlarmAide,
                BotFunctionType.Common_AlarmAideSubmit,
                BotFunctionType.Common_BlackList,
                BotFunctionType.Group_RepeaterKiller,
                BotFunctionType.Group_FundHelper,
                BotFunctionType.Group_LiveAlarm,
            };
    }
}