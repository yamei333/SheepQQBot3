using Masuit.Tools;
using SheepQQBot3.Model.Enums;
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
}