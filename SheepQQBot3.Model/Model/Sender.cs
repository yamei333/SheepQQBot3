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
    public static Sender ToSender(this GroupMember groupMember)
        => new()
        {
            GroupId = groupMember.GroupId,
            NickName = groupMember.NickName,
            Age = groupMember.Age,
            Card = groupMember.Card,
            UserId = groupMember.UserId,
            Sex = groupMember.Sex,
        };

    public static AIChatSender ToAIChatSender(this GroupMember groupMember)
    {
        var userId = groupMember.UserId;
        var name = groupMember.NickName;
        var otherName = groupMember.Card;
        return new AIChatSender
        {
            Name = name,
            Gander = groupMember.Sex,
            BName = otherName.IsNullOrEmpty() || otherName == name ? null : otherName,
            QQId = userId,
            Identity = userId == 252961222 ? "至亲" : "群友",
        };
    }

    public static AIChatSender ToAIChatSender(this Sender sender)
    {
        var userId = sender.UserId;
        var name = sender.NickName;
        var cardName = sender.CardName;
        return new AIChatSender
        {
            Name = name,
            Gander = sender.Sex,
            BName = cardName.IsNullOrEmpty() || cardName == name ? null : cardName,
            QQId = userId,
            Identity = userId == 252961222 ? "至亲" : "群友",
        };
    }

    public static ConcurrentDictionary<long, AIChatSender> ToSenderDictionary(this Dictionary<long, GroupMember> groupMembers)
    {
        var senders = groupMembers.ToConcurrentDictionary(each => each.Key, each => each.Value.ToAIChatSender());
        senders.GetOrAdd(22222, new AIChatSender
        {
            Name = "System",
            QQId = 22222,
            Identity = AIMessageSourceTypeUtil.SYSTEM,
        });
        return senders;
    }
}