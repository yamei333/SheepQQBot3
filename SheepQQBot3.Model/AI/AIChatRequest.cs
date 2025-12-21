using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    public class AIChatRequest
    {
        /// <summary>
        /// 消息发送者QQ号
        /// </summary>
        [JsonPropertyName("qq_id")]
        public string SenderId { get; set; }

        /// <summary>
        /// 消息发送者名字
        /// </summary>
        [JsonPropertyName("user_name")]
        public string NickName { get; set; }

        /// <summary>
        /// 响应内容
        /// </summary>
        [JsonPropertyName("text")]
        public string Message { get; set; }
    }

    public class AIChatSender
    {
        /// <summary>
        /// QQ号
        /// </summary>
        [JsonPropertyName("user_qq_id")]
        public string QQ { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [JsonPropertyName("user_name")]
        public string NickName { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        [JsonPropertyName("user_gender")]
        public string Gender { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        [JsonPropertyName("user_aliases")]
        public string Aliases { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        [JsonPropertyName("user_birthday")]
        public string Birthday { get; set; }

        /// <summary>
        /// 其他信息
        /// </summary>
        [JsonPropertyName("user_other_info")]
        public string OtherInfo { get; set; }

        /// <summary>
        /// 初始关系
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("relation")]
        public AIRelationData Relation { private get; set; }

        /// <summary>
        /// 允许行为
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("allowed_acts")]
        public string AllowedActs { private get; set; }

        public AIRelationData GetRelation() => Relation ?? new AIRelationData();

        public string GetAllowedActs() => AllowedActs;
    }
}