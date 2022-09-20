namespace SheepQQBot3.Model.Enums
{
    public enum BotFunctionType
    {
        Common_AlarmAide = 0,
        Common_AlarmAideSubmit = 1,
        Private_AdminConfig = 1000,
        Group_RepeaterKiller = 2000,
        Group_CustomGroupAlarm = 2001,
        Group_RepeatRevokeMessage = 2002,
        Group_FundHelper = 2003,
        Group_RandomSetu = 2004,
    }

    public static class BotFunctionTypeExtensions
    {
        public static string ToFunctionName(this BotFunctionType botFunctionType)
        {
            return botFunctionType switch
            {
                BotFunctionType.Common_AlarmAide => "闹钟助手",
                BotFunctionType.Common_AlarmAideSubmit => "闹钟助手投稿",
                BotFunctionType.Private_AdminConfig => "私聊配置",
                BotFunctionType.Group_RepeaterKiller => "复读机杀手",
                BotFunctionType.Group_CustomGroupAlarm => "群提醒",
                BotFunctionType.Group_RepeatRevokeMessage => "复读撤回消息",
                BotFunctionType.Group_FundHelper => "基金助手",
                BotFunctionType.Group_RandomSetu => "随机色图",
                _ => throw new System.NotImplementedException()
            };
        }
    }
}