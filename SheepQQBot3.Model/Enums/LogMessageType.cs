namespace SheepQQBot3.Model.Enums
{
    public enum LogMessageType
    {
        /// <summary>
        /// 系统日志_信息
        /// </summary>
        System_Info,

        /// <summary>
        /// 系统日志_错误
        /// </summary>
        System_Error,

        /// <summary>
        /// 系统日志_警告
        /// </summary>
        System_Warning,

        /// <summary>
        /// 元事件类型
        /// </summary>
        MetaData,

        /// <summary>
        /// 闹钟助手消息
        /// </summary>
        AlarmAide,

        /// <summary>
        /// 基金助手消息
        /// </summary>
        FundHelper,

        /// <summary>
        /// 群消息
        /// </summary>
        GroupMessage,

        /// <summary>
        /// 群消息撤回
        /// </summary>
        GroupRevokeMessage,

        /// <summary>
        /// 群戳一戳
        /// </summary>
        GroupPoke,
    }
}