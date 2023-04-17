using System;
using System.Text.Json.Serialization;
using SheepQQBot3.Model.Enums;
using Yamei.Common;

namespace SheepQQBot3.Model;

/// <summary>
/// 历史记录消息
/// </summary>
public class HistoryMessage
{
    /// <summary>
    /// 默认构造函数
    /// </summary>
    public HistoryMessage()
    {
    }

    [JsonPropertyName("post_type")]
    public PostType PostType { get; set; }

    [JsonPropertyName("message_type")]
    public MessageTargetType MessageType { get; set; }

    [JsonPropertyName("sub_type")]
    public SubType SubType { get; set; }

    [JsonIgnore]
    public DateTime DateTime => Time.ToDateTime();

    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    [JsonPropertyName("anonymous")]
    public string Anonymous { get; set; }

    [JsonPropertyName("font")]
    public int Font { get; set; }

    [JsonPropertyName("group_id")]
    public long GroupId { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("raw_message")]
    public string RawMessage { get; set; }

    [JsonPropertyName("message_id")]
    public int MessageId { get; set; }

    [JsonPropertyName("sender")]
    public Sender Sender { get; set; }
}