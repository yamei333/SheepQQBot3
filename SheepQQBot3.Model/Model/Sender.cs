using Masuit.Tools;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model;

public class Sender
{
    [JsonPropertyName("age")]
    public int Age { get; set; }

    [JsonPropertyName("area")]
    public string Area { get; set; }

    [JsonPropertyName("card")]
    public string Card { get; set; }

    [JsonIgnore]
    public string CardName => Card.IsNullOrEmpty() ? NickName : Card;

    [JsonPropertyName("level")]
    public string Level { get; set; }

    [JsonPropertyName("nickname")]
    public string NickName { get; set; }

    [JsonPropertyName("role")]
    public GroupRole Role { get; set; }

    [JsonPropertyName("sex")]
    public string Sex { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    [JsonPropertyName("group_id")]
    public long? GroupId { get; set; }
}

public static class SenderUtil
{
    public static AIChatSender ToAIChatSender(
        this GroupMember groupMember,
        ConcurrentDictionary<long, AIChatSender> cachedSenders)
    {
        var userId = groupMember.UserId;
        var cachedSender = cachedSenders.GetValueOrDefault(userId);
        var name = cachedSender?.Name ?? groupMember.NickName;
        var otherName = cachedSender?.BName ?? groupMember.Card;
        return new AIChatSender
        {
            QQ = userId,
            Name = name,
            BName = otherName.IsNullOrEmpty() || otherName == name ? null : otherName,
            Gander = cachedSender?.Gander ?? groupMember.Sex,
            Birthday = cachedSender?.Birthday,
            Other = cachedSender?.Other,
        };
    }

    public static AIChatSender ToAIChatSender(
        this Sender sender,
        ConcurrentDictionary<long, AIChatSender> cachedSenders)
    {
        var userId = sender.UserId;
        var cachedSender = cachedSenders.GetValueOrDefault(userId);
        var name = cachedSender?.Name ?? sender.NickName;
        var otherName = cachedSender?.BName ?? sender.CardName;
        return new AIChatSender
        {
            QQ = userId,
            Name = name,
            BName = otherName.IsNullOrEmpty() || otherName == name ? null : otherName,
            Gander = cachedSender?.Gander ?? sender.Sex,
            Birthday = cachedSender?.Birthday,
            Other = cachedSender?.Other,
        };
    }

    public static ConcurrentDictionary<long, AIChatSender> ToSenderDictionary(
        this Dictionary<long, GroupMember> groupMembers,
        ConcurrentDictionary<long, AIChatSender> cachedSenders)
    {
        return groupMembers.ToConcurrentDictionary(
            each => each.Key,
            each => each.Value.ToAIChatSender(cachedSenders));
    }
}