using Masuit.Tools;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    [Description("response")]
    public class AIChatResponse
    {
        /// <summary>
        /// 消息回复时间
        /// </summary>
        [Description("response date")]
        [JsonPropertyName("date")]
        public string Date { get; set; }

        /// <summary>
        /// 日志的值类型
        /// </summary>
        [JsonIgnore]
        public DateTime DateValue => Date.ToDateTime();

        /// <summary>
        /// 回复信息内容
        /// </summary>
        [Description("response contents")]
        [JsonPropertyName("contents")]
        public AIChatResponseContent[] Contents { get; set; }

        /// <summary>
        /// 好感度变化信息
        /// </summary>
        [Description("favorability change info")]
        [JsonPropertyName("favorabilityChangeInfos")]
        public AIFavorabilityChangeInfo[] FavorabilityChangeInfos { get; set; }

        /// <summary>
        /// 拉黑用户信息
        /// </summary>
        [Description("block user info")]
        [JsonPropertyName("blockUserInfos")]
        public AIBlockUserInfo[] BlockUserInfos { get; set; }

        /// <summary>
        /// AI知识笔记
        /// </summary>
        [Description("knowledge Note")]
        [JsonPropertyName("knowledgeNote")]
        public AIKnowledgeNote KnowledgeNote { get; set; }

        /// <summary>
        /// AI灵感笔记
        /// </summary>
        [Description("inspiration Note")]
        [JsonPropertyName("inspirationNote")]
        public AIInspirationNote InspirationNote { get; set; }

        /// <summary>
        /// AI状态变化信息
        /// </summary>
        [Description("other status change info")]
        [JsonPropertyName("statusChangeInfo")]
        public AIStatusChangeInfo StatusChangeInfo { get; set; }
    }

    public class AIChatResponseContent
    {
        /// <summary>
        /// 思考内容
        /// </summary>
        [Description("thinking")]
        [JsonPropertyName("think")]
        public string Think { get; set; }

        /// <summary>
        /// 肢体语言
        /// </summary>
        [Description("body language, The cat ears' movements also fall under this category.")]
        [JsonPropertyName("bodyLanguage")]
        public string Body { get; set; }

        /// <summary>
        /// 感官
        /// </summary>
        [Description("sight, hearing, smell, taste, touch")]
        [JsonPropertyName("sensory")]
        public string Sensory { get; set; }

        /// <summary>
        /// 心理描写
        /// </summary>
        [Description("psychological description")]
        [JsonPropertyName("psychologicalDesc")]
        public string Mind { get; set; }

        /// <summary>
        /// 神情
        /// </summary>
        [Description("expression")]
        [JsonPropertyName("expression")]
        public string Face { get; set; }

        /// <summary>
        /// 回复消息
        /// </summary>
        [Description("response message info")]
        [JsonPropertyName("chatMessageInfo")]
        [NotNull]
        public AIChatMessage ChatMessageInfo { get; set; }
    }

    public class AIChatMessage
    {
        /// <summary>
        /// 表情包代码
        /// </summary>
        [Description("emoji code")]
        [JsonPropertyName("emoji")]
        public string Emoji { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        [Description("message text")]
        [JsonPropertyName("text")]
        [NotNull]
        public string Text { get; set; }

        /// <summary>
        /// 距离下条消息间隔(毫秒)
        /// </summary>
        [Description("next message interval")]
        [JsonPropertyName("msgInterval")]
        public int? Delay { get; set; }
    }

    public class AIFavorabilityChangeInfo
    {
        /// <summary>
        /// 对象QQ号
        /// </summary>
        [Description("favorability change target qq id")]
        [JsonPropertyName("favoChangeTarget")]
        public long TargetId { get; set; }

        /// <summary>
        /// 好感度变化数值
        /// </summary>
        [Description("favorability change value, value range is -6 to 5")]
        [JsonPropertyName("favoChangeValue")]
        public int Value { get; set; }
    }

    public class AIBlockUserInfo
    {
        /// <summary>
        /// 拉黑对象QQ号
        /// </summary>
        [Description("block target qq id")]
        [JsonPropertyName("blockTarget")]
        public long TargetId { get; set; }

        /// <summary>
        /// 拉黑持续时间(分钟)
        /// </summary>
        [Description("block target minutes")]
        [JsonPropertyName("blockMinutes")]
        public int Value { get; set; }
    }

    /// <summary>
    /// AI状态信息
    /// </summary>
    public class AIStatusChangeInfo
    {
        /// <summary>
        /// 心情指数变化
        /// </summary>
        [Description("mood index change value")]
        [JsonPropertyName("moodChangeValue")]
        public int MoodIndexChange { get; set; }
    }

    /// <summary>
    /// AI知识笔记
    /// </summary>
    public class AIKnowledgeNote
    {
        /// <summary>
        /// 标题
        /// </summary>
        [Description("note title")]
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Description("note content")]
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    /// <summary>
    /// AI灵感笔记
    /// </summary>
    public class AIInspirationNote
    {
        /// <summary>
        /// 标题
        /// </summary>
        [Description("note title")]
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Description("note content")]
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}