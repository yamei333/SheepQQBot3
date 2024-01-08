using System;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model
{
    [Serializable]
    public class DGPDailyNote
    {
        /// <summary>
        /// 周本减半剩余次数
        /// </summary>
        [JsonPropertyName("remain_resin_discount_num")]
        public int WeekDiscountCount { get; set; }

        /// <summary>
        /// 壶币剩余恢复时间(秒数)
        /// </summary>
        [JsonPropertyName("home_coin_recovery_time")]
        public int HomeCoinRecoveryTime { get; set; }

        /// <summary>
        /// 壶币恢复时间(日期)
        /// </summary>
        [JsonIgnore]
        public DateTime HomeCoinRecoveryDateTime => DateTime.Now.AddSeconds(HomeCoinRecoveryTime);

        /// <summary>
        /// 参量质变仪
        /// </summary>
        [JsonPropertyName("transformer")]
        public DGPDailyNoteTransformer Transformer { get; set; }

        /// <summary>
        /// 每日任务
        /// </summary>
        [JsonPropertyName("daily_task")]
        public DGPDailyNoteDailyTask DailyTask { get; set; }

        /// <summary>
        /// 当前宝钱数量
        /// </summary>
        [JsonPropertyName("current_home_coin")]
        public int CurrentHomeCoin { get; set; }

        /// <summary>
        /// 最大宝钱数量
        /// </summary>
        [JsonPropertyName("max_home_coin")]
        public int MaxHomeCoin { get; set; }

        /// <summary>
        /// 当前体力
        /// </summary>
        [JsonPropertyName("current_resin")]
        public int CurrentResin { get; set; }

        /// <summary>
        /// 最大体力
        /// </summary>
        [JsonPropertyName("max_resin")]
        public int MaxResin { get; set; }

        /// <summary>
        /// 体力恢复所需时间(秒数)
        /// </summary>
        [JsonPropertyName("resin_recovery_time")]
        public int ResinRecoveryTime { get; set; }

        /// <summary>
        /// 体力恢复时间(日期)
        /// </summary>
        [JsonIgnore]
        public DateTime ResinRecoveryDateTime => DateTime.Now.AddSeconds(ResinRecoveryTime);
    }

    [Serializable]
    public class DGPDailyNoteTransformer
    {
        /// <summary>
        /// 参量质变仪解锁
        /// </summary>
        [JsonPropertyName("obtained")]
        public bool Obtained { get; set; }

        /// <summary>
        /// 参量质变仪CD时间
        /// </summary>
        [JsonPropertyName("recovery_time")]
        public DGPDailyNoteTransformerRecoveryTime RecoveryTime { get; set; }
    }

    [Serializable]
    public class DGPDailyNoteTransformerRecoveryTime
    {
        /// <summary>
        /// 参量质变仪可用
        /// </summary>
        [JsonPropertyName("reached")]
        public bool Reached { get; set; }
    }

    [Serializable]
    public class DGPDailyNoteDailyTask
    {
        /// <summary>
        /// 是否领取每日奖励
        /// </summary>
        [JsonPropertyName("is_extra_task_reward_received")]
        public bool IsExtraTaskRewardReceived { get; set; }

        /// <summary>
        /// 最大任务数
        /// </summary>
        [JsonPropertyName("total_num")]
        public int TotalTaskNumber { get; set; }

        /// <summary>
        /// 已完成任务数
        /// </summary>
        [JsonPropertyName("finished_num")]
        public int FinishedTaskNumber { get; set; }
    }
}