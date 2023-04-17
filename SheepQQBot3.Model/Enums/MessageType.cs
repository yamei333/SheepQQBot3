using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Enums;

/// <summary>
/// 目标类型
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageTargetType
{
    /// <summary>
    /// 私聊消息
    /// </summary>
    Private,

    /// <summary>
    /// 群消息
    /// </summary>
    Group,

    /// <summary>
    /// 频道消息
    /// </summary>
    Guild,
}