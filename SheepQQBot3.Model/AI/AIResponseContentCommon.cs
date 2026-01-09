using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI;

public class AIResponseContentCommon
{
    /// <summary>
    /// 思考内容
    /// </summary>
    [Description("【关键】内心独白（用户不可见）。在回复前，先分析用户意图，结合当前心情(Mood)和关系，决定你的情绪反应（开心/生气/傲娇等）。")]
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
    /// 距离下条消息间隔(毫秒)
    /// </summary>
    [Description("发送此消息前的延迟（毫秒）。建议 500-2000 以模拟真人打字速度。")]
    [JsonPropertyName("msg_interval")]
    [Required]
    public int? Delay { get; set; }
}