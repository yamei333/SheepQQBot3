using System;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Model.WebApi;

[Serializable]
public class WebApi_SendMessage
{
    /// <summary>
    /// 验证用字段
    /// </summary>
    [JsonPropertyName("sheepqqbot3")]
    public string SheepQQBot3 { get; set; }

    /// <summary>
    /// 是否发送群消息
    /// </summary>
    [JsonPropertyName("isgroup")]
    public bool IsGroup { get; set; }

    /// <summary>
    /// 目标ID
    /// </summary>
    [JsonPropertyName("targetid")]
    public string TargetId { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }
}