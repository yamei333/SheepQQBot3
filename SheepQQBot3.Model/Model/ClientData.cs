using System;
using System.Text.Json.Serialization;
using Yamei.Common;

namespace SheepQQBot3.Model;

public class ClientData
{
    [JsonPropertyName("group_id")]
    public long GroupId { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("message_id")]
    public int MessageId { get; set; }

    [JsonPropertyName("message_seq")]
    public int MessageSeq { get; set; }

    [JsonPropertyName("message_type")]
    public string MessageType { get; set; }

    [JsonPropertyName("raw_message")]
    public string RawMessage { get; set; }

    [JsonPropertyName("real_id")]
    public int RealId { get; set; }

    [JsonIgnore]
    public DateTime DateTime => Time.ToDateTime();

    [JsonPropertyName("time")]
    public int Time { get; set; }

    [JsonPropertyName("sender")]
    public Sender Sender { get; set; }
}