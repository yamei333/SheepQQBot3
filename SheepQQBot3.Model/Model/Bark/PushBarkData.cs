using System.Text.Json.Serialization;

namespace SheepQQBot3.Model;

/// <summary>
/// Bark推送数据类
/// </summary>
public class PushBarkData
{
    /// <summary>
    /// 标题
    /// </summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Title { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [JsonPropertyName("body")]
    public string Body { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [JsonPropertyName("icon")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Icon { get; set; }

    /// <summary>
    /// 目标链接
    /// </summary>
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string LinkUrl { get; set; }

    /// <summary>
    /// 是否存档
    /// </summary>
    [JsonPropertyName("isArchive")]
    public int IsArchive { get; set; }

    /// <summary>
    /// 是否可复制
    /// </summary>
    [JsonPropertyName("copy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? IsCopy { get; set; }

    /// <summary>
    /// 是否自动复制
    /// </summary>
    [JsonPropertyName("autoCopy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? IsAutoCopy { get; set; }

    /// <summary>
    /// 设置响铃名称
    /// </summary>
    [JsonPropertyName("sound")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Sound { get; set; }

    /// <summary>
    /// 是否持续响铃30秒
    /// </summary>
    [JsonPropertyName("call")]
    public int? IsCall { get; set; }

    /// <summary>
    /// 群组名称
    /// </summary>
    [JsonPropertyName("group")]
    public string Group { get; set; }
}