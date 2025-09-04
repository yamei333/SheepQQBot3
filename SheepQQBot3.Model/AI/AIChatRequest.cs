using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    public class AIChatRequest
    {
        /// <summary>
        /// 消息发送者身份
        /// </summary>
        [JsonPropertyName("sender")]
        public AIChatSender Sender { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        [JsonPropertyName("date")]
        public string Date { get; set; }

        /// <summary>
        /// 响应内容
        /// </summary>
        [JsonPropertyName("text")]
        public string Message { get; set; }
    }

    public class AIChatSender
    {
        /// <summary>
        /// 姓名
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        [JsonPropertyName("gander")]
        public string Gander { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        [JsonPropertyName("bname")]
        public string BName { get; set; }

        /// <summary>
        /// QQ号
        /// </summary>
        [JsonPropertyName("qq")]
        public long QQId { get; set; }

        /// <summary>
        /// 身份
        /// </summary>
        [JsonPropertyName("sf")]
        public string Identity { get; set; }

        /// <summary>
        /// 消息来源 (Source: Group chat/Private chat)
        /// </summary>
        [JsonPropertyName("src")]
        public string Source { get; set; }
    }
}