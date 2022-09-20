namespace SheepQQBot3.Model.Enums
{
    /// <summary>
    /// 配置目标类型
    /// </summary>
    public enum BotConfigTargetType
    {
        /// <summary>
        /// 系统
        /// </summary>
        Common = 0,

        /// <summary>
        /// 群
        /// </summary>
        Group = 1,

        /// <summary>
        /// 个人
        /// </summary>
        Private = 2,
    }

    public static class TargetTypeExtensions
    {
        public static BotFunctionType[]? GetAllowFunctions(this BotConfigTargetType botConfigTargetType)
        {
            return botConfigTargetType switch
            {
                BotConfigTargetType.Common => new[] { BotFunctionType.Common_AlarmAide },
                BotConfigTargetType.Group => new[]
                {
                    BotFunctionType.Group_CustomGroupAlarm, BotFunctionType.Group_RepeaterKiller,
                    BotFunctionType.Common_AlarmAideSubmit
                },
                BotConfigTargetType.Private => new[] { BotFunctionType.Private_AdminConfig },
                _ => null
            };
        }
    }
}