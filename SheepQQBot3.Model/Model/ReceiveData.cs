using System;
using System.Text.Json.Serialization;
using SheepQQBot3.Model.Enums;
using Yamei.Common;

namespace SheepQQBot3.Model;

public class ReceiveData
{
    [JsonPropertyName("meta_event_type")]
    public string Meta_Event_Type { get; set; }

    [JsonPropertyName("sub_type")]
    public SubType SubType { get; set; }

    [JsonIgnore]
    public DateTime DateTime => Time.ToDateTime();

    [JsonPropertyName("time")]
    public int Time { get; set; }

    [JsonPropertyName("post_type")]
    public PostType PostType { get; set; }

    [JsonPropertyName("notice_type")]
    public NoticeType NoticeType { get; set; }

    [JsonPropertyName("self_id")]
    public long SelfId { get; set; }

    [JsonPropertyName("operator_id")]
    public long OperatorId { get; set; }

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    [JsonPropertyName("sender_id")]
    public long SenderId { get; set; }

    [JsonPropertyName("target_id")]
    public long TargetId { get; set; }

    [JsonPropertyName("anonymous")]
    public string Anonymous { get; set; }

    [JsonPropertyName("font")]
    public int Font { get; set; }

    [JsonPropertyName("group_id")]
    public long GroupId { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("message_id")]
    public int MessageId { get; set; }

    /// <summary>
    /// 消息目标类型
    /// </summary>
    [JsonPropertyName("message_type")]
    public MessageTargetType MessageTargetType { get; set; }

    /// <summary>
    /// 原始消息
    /// 0.13.0.0 起弃用
    /// </summary>
    //[JsonPropertyName("raw_message")]
    //public string RawMessage { get; set; }

    [JsonPropertyName("sender")]
    public Sender Sender { get; set; }

    /// <summary>
    /// 当前龙王
    /// </summary>
    [JsonPropertyName("current_talkative")]
    public object Current_Talkative { get; set; }

    /// <summary>
    /// 历史龙王
    /// </summary>
    [JsonPropertyName("talkative_list")]
    public object Talkative_List { get; set; }

    /// <summary>
    /// 群聊之火
    /// </summary>
    [JsonPropertyName("performer_list")]
    public object Performer_List { get; set; }

    /// <summary>
    /// 群聊炽焰
    /// </summary>
    [JsonPropertyName("legend_list")]
    public object Legend_List { get; set; }

    /// <summary>
    /// 冒尖小春笋
    /// </summary>
    [JsonPropertyName("strong_newbie_list")]
    public object Strong_Newbie_List { get; set; }

    /// <summary>
    /// 快乐之源
    /// </summary>
    [JsonPropertyName("emotion_list")]
    public object Emotion_List { get; set; }
}