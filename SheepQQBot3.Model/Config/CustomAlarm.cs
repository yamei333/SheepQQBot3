using System;
using MessagePack;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// 自定义提醒
/// </summary>
[MessagePackObject]
public class CustomAlarm
{
    [Key(nameof(Id))]
    public Guid Id { get; set; }

    [Key(nameof(TargetId))]
    public long TargetId { get; set; }

    /// <summary>
    /// 是否是群提醒
    /// </summary>
    [Key(nameof(IsGroup))]
    public bool IsGroup { get; set; }

    [Key(nameof(GroupId))]
    public long? GroupId { get; set; }

    [Key(nameof(IsAtTarget))]
    public bool IsAtTarget { get; set; }

    [Key(nameof(IsLoop))]
    public bool IsLoop { get; set; }

    [Key(nameof(IsBark))]
    public bool IsBark { get; set; }

    [Key(nameof(AlarmDate))]
    public DateTime AlarmDate { get; set; }

    [Key(nameof(AlarmMessage))]
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