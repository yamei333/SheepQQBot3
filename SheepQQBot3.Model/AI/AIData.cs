using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    /// <summary>
    /// AI信息(保存用)
    /// </summary>
    public class AIData
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        [JsonPropertyName("userDatas")]
        public ConcurrentDictionary<string, AIUserData> UserDatas { get; set; } = [];

        /// <summary>
        /// 小助手状态
        /// </summary>
        [JsonPropertyName("aiStatus")]
        public AIStatusData AIStatusData { get; set; }
    }

    /// <summary>
    /// 小助手状态(保存用)
    /// </summary>
    public class AIStatusData
    {
        /// <summary>
        /// 心情指数
        /// </summary>
        [JsonPropertyName("mood")]
        public int MoodIndexValue { get; set; }
    }

    public class AIUserData
    {
        /// <summary>
        /// 关系
        /// </summary>
        [JsonPropertyName("relation")]
        public AIRelationData Relation { get; set; }

        /// <summary>
        /// 拉黑持续至(时间)
        /// </summary>
        [JsonPropertyName("blockUntil")]
        public long BlockUntil { get; set; }

        /// <summary>
        /// 禁止行为
        /// </summary>
        [JsonPropertyName("allowed_acts")]
        [Description("Allowed Acts")]
        public string AllowedActs { get; set; }
    }

    public class AIRelationData
    {
        /// <summary>
        /// 亲密度 (Intimacy) —— 社交距离与防备心理
        /// </summary>
        [JsonPropertyName("intimacy")]
        public int Intimacy { get; set; }

        /// <summary>
        /// 认可度 (Respect) —— 地位感与意志服从
        /// </summary>
        [JsonPropertyName("respect")]
        public int Respect { get; set; }

        /// <summary>
        /// 好感度 (Affection) —— 情感底色与包容限度
        /// </summary>
        [JsonPropertyName("affection")]
        public int Affection { get; set; }
    }
}