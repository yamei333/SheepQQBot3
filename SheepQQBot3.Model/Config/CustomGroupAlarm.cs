using System;
using MessagePack;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// 群自定义提醒
/// </summary>
[MessagePackObject]
public class CustomGroupAlarm
{
    [Key(nameof(Id))]
    public Guid Id { get; set; }

    [Key(nameof(TargetId))]
    public long TargetId { get; set; }

    [Key(nameof(GroupId))]
    public long GroupId { get; set; }

    [Key(nameof(IsAtTarget))]
    public bool IsAtTarget { get; set; }

    [Key(nameof(AlarmDate))]
    public DateTime AlarmDate { get; set; }

    [Key(nameof(AlarmMessage))]
    public string AlarmMessage { get; set; }

    public CustomGroupAlarm(
        Guid id,
        long groupId,
        long targetId,
        DateTime alarmDate,
        string alarmMessage,
        bool isAtTarget)
    {
        Id = id;
        GroupId = groupId;
        TargetId = targetId;
        this.AlarmDate = alarmDate;
        this.AlarmMessage = alarmMessage;
        this.IsAtTarget = isAtTarget;
    }
}