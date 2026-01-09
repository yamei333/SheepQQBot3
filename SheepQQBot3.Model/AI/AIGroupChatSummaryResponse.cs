using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI;

[Description("Structured response for the Roleplay.")]
public class AIGroupChatSummaryResponse : AIResponseFeedBack
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
}

public class AIGroupChatSummaryResponseContent : AIResponseContentCommon
{
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
}