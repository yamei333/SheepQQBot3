using System;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// 自定义提醒
/// </summary>
public class CustomAlarm
{
    [JsonPropertyName(nameof(Id))]
    public Guid Id { get; set; }

    [JsonPropertyName(nameof(TargetId))]
    public long TargetId { get; set; }

    /// <summary>
    /// 是否是群提醒
    /// </summary>
    [JsonPropertyName(nameof(IsGroup))]
    public bool IsGroup { get; set; }

    [JsonPropertyName(nameof(GroupId))]
    public long? GroupId { get; set; }

    [JsonPropertyName(nameof(IsAtTarget))]
    public bool IsAtTarget { get; set; }

    [JsonPropertyName(nameof(IsLoop))]
    public bool IsLoop { get; set; }

    [JsonPropertyName(nameof(IsBark))]
    public bool IsBark { get; set; }

    [JsonPropertyName(nameof(AlarmDate))]
    public DateTime AlarmDate { get; set; }

    [JsonPropertyName(nameof(AlarmMessage))]
    public string AlarmMessage { get; set; }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public CustomAlarm(
        Guid id,
        bool isGroup,
        long? groupId,
        long targetId,
        DateTime alarmDate,
        string alarmMessage,
        bool isAtTarget,
        bool isLoop,
        bool isBark)
    {
        Id = id;
        IsGroup = isGroup;
        GroupId = groupId;
        TargetId = targetId;
        AlarmDate = alarmDate;
        AlarmMessage = alarmMessage;
        IsAtTarget = isAtTarget;
        IsLoop = isLoop;
        IsBark = isBark;
    }
}