using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    public class AIData
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        [JsonPropertyName("userDatas")]
        public ConcurrentDictionary<long, AIUserData> UserDatas { get; set; } = [];

        /// <summary>
        /// 小助手状态
        /// </summary>
        [JsonPropertyName("aiStatus")]
        public AIStatusData AIStatusData { get; set; }
    }

    public class AIUserData
    {
        /// <summary>
        /// 好感度
        /// </summary>
        [JsonPropertyName("favorability")]
        public int Favorability { get; set; }

        /// <summary>
        /// 拉黑持续至(时间)
        /// </summary>
        [JsonPropertyName("blockUntil")]
        public long BlockUntil { get; set; }

        /// <summary>
        /// 禁止行为
        /// </summary>
        [JsonPropertyName("prohibitedActs")]
        [Description("Prohibited Acts")]
        public string ProhibitedActs { get; set; }
    }
}