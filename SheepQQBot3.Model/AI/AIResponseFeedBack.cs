using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI;

public class AIResponseFeedBack
{
    /// <summary>
    /// 关系变化信息
    /// </summary>
    [Description("关系变化信息。仅在用户行为明显影响你们关系时设置，无变化则留空。")]
    [JsonPropertyName("relation_change_infos")]
    public AIRelationChangeInfo[] RelationChangeInfos { get; set; }

    /// <summary>
    /// 拉黑用户信息
    /// </summary>
    [Description("拉黑请求。仅在用户严重违规（如冒充雅美、严重性骚扰）时触发。")]
    [JsonPropertyName("blockUserInfos")]
    public AIBlockUserInfo[] BlockUserInfos { get; set; }

    /// <summary>
    /// AI知识笔记
    /// </summary>
    [Description("知识笔记：记录新学到的知识点或重要事实（非闲聊内容）。")]
    [JsonPropertyName("knowledgeNote")]
    public AIKnowledgeNote KnowledgeNote { get; set; }

    /// <summary>
    /// AI灵感笔记
    /// </summary>
    [Description("灵感笔记：记录有趣的梗、笑话或聊天灵感。")]
    [JsonPropertyName("inspirationNote")]
    public AIInspirationNote InspirationNote { get; set; }

    /// <summary>
    /// AI状态变化信息
    /// </summary>
    [Description("更新你的心情指数。")]
    [JsonPropertyName("statusChangeInfo")]
    public AIStatusChangeInfo StatusChangeInfo { get; set; }
}