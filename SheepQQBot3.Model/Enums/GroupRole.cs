using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GroupRole
{
    /// <summary>
    /// 群主
    /// </summary>
    Owner,

    /// <summary>
    /// 管理员
    /// </summary>
    Admin,

    /// <summary>
    /// 成员
    /// </summary>
    Member,
}