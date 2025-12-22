using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// 黑名单用户配置
    /// </summary>
    public class BlackListUserConfig(string targetId)
    {
        [JsonPropertyName(nameof(TargetId))]
        public string TargetId { get; set; } = targetId;

        /// <summary>
        /// 禁止色图功能
        /// </summary>
        [JsonPropertyName(nameof(BanedSetu))]
        public bool BanedSetu { get; set; }

        /// <summary>
        /// 禁止AI收集消息
        /// </summary>
        [JsonPropertyName(nameof(BanedAICollect))]
        public bool BanedAICollect { get; set; }

        /// <summary>
        /// 禁止AIAt功能
        /// </summary>
        [JsonPropertyName(nameof(BanedAIAt))]
        public bool BanedAIAt { get; set; }

        /// <summary>
        /// 禁止群聊统计功能
        /// </summary>
        [JsonPropertyName(nameof(BanedChatSummary))]
        public bool BanedChatSummary { get; set; }

        /// <summary>
        /// 禁止群聊统计收集
        /// </summary>
        [JsonPropertyName(nameof(BanedChatSummaryCollect))]
        public bool BanedChatSummaryCollect { get; set; }
    }
}