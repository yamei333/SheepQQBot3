using System.Text.Json.Serialization;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.Model;

public class GroupMember
{
    [JsonPropertyName("age")]
    public int Age { get; set; }

    [JsonPropertyName("area")]
    public string Area { get; set; }

    [JsonPropertyName("card")]
    public string Card { get; set; }

    [JsonPropertyName("card_changeable")]
    public bool CardChangeable { get; set; }

    [JsonPropertyName("group_id")]
    public long GroupId { get; set; }

    [JsonPropertyName("join_time")]
    public long JoinTime { get; set; }

    [JsonPropertyName("last_sent_time")]
    public long LastSendTime { get; set; }

    [JsonPropertyName("level")]
    public string LevelStr { get; set; }

    [JsonPropertyName("nickname")]
    public string NickName { get; set; }

    [JsonPropertyName("role")]
    public GroupRole Role { get; set; }

    [JsonPropertyName("sex")]
    public string Sex { get; set; }

    [JsonPropertyName("shut_up_timestamp")]
    public long ShutUpTimestamp { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("title_expire_time")]
    public long TitleExpireTime { get; set; }

    [JsonPropertyName("unfriendly")]
    public bool Unfriendly { get; set; }

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }
}