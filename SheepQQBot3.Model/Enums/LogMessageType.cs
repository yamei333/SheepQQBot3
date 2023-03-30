using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LogMessageType
    {
        /// <summary>
        /// 系统日志_信息
        /// </summary>
        [Display(Name = "Bot消息")]
        System_Info,

        /// <summary>
        /// 系统日志_错误
        /// </summary>
        [Display(Name = "Bot错误")]
        System_Error,

        /// <summary>
        /// 系统日志_警告
        /// </summary>
        [Display(Name = "Bot警告")]
        System_Warning,

        /// <summary>
        /// 元事件类型
        /// </summary>
        [Display(Name = "元事件")]
        MetaData,

        /// <summary>
        /// 闹钟助手消息
        /// </summary>
        [Display(Name = "闹钟助手")]
        AlarmAide,

        /// <summary>
        /// 基金助手消息
        /// </summary>
        [Display(Name = "基金助手")]
        FundHelper,

        /// <summary>
        /// 直播提醒消息
        /// </summary>
        [Display(Name = "直播提醒")]
        LiveAlarm,

        /// <summary>
        /// 原神每日提醒消息
        /// </summary>
        [Display(Name = "原神每日提醒")]
        GenshinDailyNoteAlarm,

        /// <summary>
        /// 群消息
        /// </summary>
        [Display(Name = "群消息")]
        GroupMessage,

        /// <summary>
        /// 群消息撤回
        /// </summary>
        [Display(Name = "群消息撤回")]
        GroupRevokeMessage,

        /// <summary>
        /// 群戳一戳
        /// </summary>
        [Display(Name = "群戳一戳")]
        GroupPoke,

        /// <summary>
        /// 风控消息(发送消息被屏蔽)
        /// </summary>
        [Display(Name = "账号风控")]
        BlockedByServer,
    }
}