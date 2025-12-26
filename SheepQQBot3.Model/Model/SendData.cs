using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model;

[Serializable]
public class SendData
{
    /// <summary>
    /// 接口方法
    /// </summary>
    [JsonPropertyName("action")]
    public string Action { get; }

    [JsonPropertyName("params")]
    public ParamData ParamData { get; set; }

    /// <summary>
    /// 回声
    /// </summary>
    [JsonPropertyName("echo")]
    public string Echo { get; set; }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public SendData(string action, ParamData paramData, string echo)
    {
        this.Action = action;
        this.ParamData = paramData;
        this.Echo = echo;
    }
}

public class ParamData
{
    [JsonPropertyName("group_id")]
    public string GroupId { get; set; }

    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    [JsonPropertyName("message_id")]
    public string MessageId { get; set; }

    [JsonPropertyName("times")]
    public string Times { get; set; }

    [JsonPropertyName("reject_add_request")]
    public string Reject_Add_Request { get; set; }

    [JsonPropertyName("duration")]
    public string Duration { get; set; }

    [JsonPropertyName("enable")]
    public string Enable { get; set; }

    [JsonPropertyName("card")]
    public string Card { get; set; }

    [JsonPropertyName("group_name")]
    public string GroupName { get; set; }

    [JsonPropertyName("message")]
    public List<Element> Message { get; set; }

    [JsonPropertyName("no_cache")]
    public bool NoCache { get; set; }

    [JsonPropertyName("emoji_id")]
    public string EmojiId { get; set; }

    [JsonPropertyName("domain")]
    public string Domain { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("desc")]
    public string Content { get; set; }

    [JsonPropertyName("picUrl")]
    public string PicUrl { get; set; }

    [JsonPropertyName("jumpUrl")]
    public string JumpUrl { get; set; }

    [JsonPropertyName("iconUrl")]
    public string IconUrl { get; set; }

    [JsonPropertyName("webUrl")]
    public string WebUrl { get; set; }

    [JsonPropertyName("file_id")]
    public string FileId { get; set; }
}