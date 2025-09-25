using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    public class AIChatRequest
    {
        /// <summary>
        /// 消息发送者QQID
        /// </summary>
        [JsonPropertyName("qq")]
        public long SenderId { get; set; }

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
        public long QQ { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        [JsonPropertyName("birthday")]
        public string Birthday { get; set; }

        /// <summary>
        /// 其他信息
        /// </summary>
        [JsonPropertyName("other")]
        public string Other { get; set; }
    }
}