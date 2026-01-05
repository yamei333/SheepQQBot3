using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI;

[Description("Structured response for the Roleplay.")]
public class AIGroupChatSummaryResponse
{
    /// <summary>
    /// 回复信息内容
    /// </summary>
    [Description(@"回复消息列表（气泡载荷）。
【拟人化连发机制】：
1. 模拟人类手速：人类很少一次性发一大段话，而是习惯连续发送多条短消息。

【独立性原则】：
注意：数组中的每一个对象都是独立的！即使是同一轮回复，每一条消息也必须重新生成单独的 think、expression_code 和 sensory，严禁合并到根节点！
群聊总结相关：严禁将所有话题合并在一条消息里！必须拆解为：话题1是一个对象，话题2是另一个对象。")]
    [JsonPropertyName("contents")]
    [Required]
    public AIGroupChatSummaryResponseContent[] Contents { get; set; }

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

public class AIGroupChatSummaryResponseContent
{
    /// <summary>
    /// 思考内容
    /// </summary>
    [Description("【关键】内心独白（用户不可见）。在回复前，先分析用户意图，结合当前心情(Mood)和好感度，决定你的情绪反应（开心/生气/傲娇等）。")]
    [JsonPropertyName("think")]
    public string Think { get; set; }

    /// <summary>
    /// 肢体语言
    /// </summary>
    [Description("你所做的动作描述，猫娘特征（如耳朵抖动、尾巴摇摆、炸毛）及面部神态也需要描写，无需加上自己的名字。")]
    [JsonPropertyName("body_language")]
    public string Body { get; set; }

    /// <summary>
    /// 感官
    /// </summary>
    [Description("环境感官描写（你看到、听到、闻到或触碰到的感觉）。")]
    [JsonPropertyName("sensory")]
    public string Sensory { get; set; }

    /// <summary>
    /// 心理描写
    /// </summary>
    [Description("潜意识的情感波动或深层心理状态。")]
    [JsonPropertyName("psychological_desc")]
    public string Mind { get; set; }

    /// <summary>
    /// 神情
    /// </summary>
    [Description("当前的面部表情枚举值。")]
    [JsonPropertyName("expression_code")]
    public AIExpressionType? Face { get; set; }

    /// <summary>
    /// 表情包代码
    /// </summary>
    [Description("选择一个最能配合当前这条文字（Text）语气的表情 Key。如果这句话很平淡，可以留空。")]
    [JsonPropertyName("sticker_code")]
    public AIEmojiType? Emoji { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    [Description(@"气泡内的纯文本内容。
【长度与切分】：
1. 单个气泡字数限制：严格控制在50字以内。
2. 只要你觉得这句话还没说完，或者太长了，就立刻结束当前字符串。将剩下的内容放到 Contents 数组的 **下一个对象的 Text 字段** 中。
3. 严禁 Markdown 和 长篇大论。

【去客服化】：
1. 严禁服务式反问。
2. 允许把天聊死。")]
    [JsonPropertyName("text")]
    [NotNull]
    [Required]
    public string Text { get; set; }

    /// <summary>
    /// 距离下条消息间隔(毫秒)
    /// </summary>
    [Description("发送此消息前的延迟（毫秒）。建议 500-2000 以模拟真人打字速度。")]
    [JsonPropertyName("msg_interval")]
    [Required]
    public int? Delay { get; set; }
}