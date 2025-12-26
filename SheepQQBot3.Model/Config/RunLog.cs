using Masuit.Tools;
using Masuit.Tools.Systems;
using OpenAI.Chat;
using SheepQQBot3.Model.Enums;
using System;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// 运行日志
/// </summary>
public class RunLog
{
    //private const string DefaultColor = "Black";
    private const string SystemSenderId = "系统";

    [JsonIgnore]
    public string DateTimeStr => _logDate.ToString("HH:mm:ss");

    [JsonIgnore]
    public string DateTimeStrFFF => _logDate.ToString("HH:mm:ss.fff");

    [JsonIgnore]
    public bool IsGroupMessage => LogMessageType == LogMessageType.GroupMessage;

    /// <summary>
    /// 是否有GroudId
    /// </summary>
    [JsonIgnore]
    public bool HasGroupId => !GroupId.IsNullOrEmpty();

    public LogMessageType LogMessageType { get; set; }

    [JsonIgnore]
    public string TargetTypeStr => TargetType.GetDisplay();

    public BotConfigTargetType TargetType { get; set; }
    public string OperatorId { get; set; }
    public string SenderId { get; set; }
    public string GroupId { get; set; }
    public string TargetId { get; set; }

    /// <summary>
    /// 其他内容
    /// </summary>
    public string OtherContent { get; set; }

    /// <summary>
    /// AI用量
    /// </summary>
    public ChatTokenUsage Usage { get; set; }

    public string MessageId { get; set; }
    public string Content { get; set; }
    public bool IsBlackList { get; set; }

    private readonly DateTime _logDate;

    ///// <summary>
    ///// 消息颜色
    ///// </summary>
    //public string MessageColor
    //    => LogMessageType switch
    //    {
    //        LogMessageType.MetaData => DefaultColor,
    //        LogMessageType.GroupMessage => DefaultColor,
    //        LogMessageType.GroupRevokeMessage => DefaultColor,
    //        LogMessageType.GroupPoke => DefaultColor,
    //        LogMessageType.System_Info => DefaultColor,
    //        LogMessageType.BotBackground_Info => DefaultColor,
    //        LogMessageType.AlarmAide => DefaultColor,
    //        LogMessageType.FundHelper => DefaultColor,
    //        LogMessageType.LiveAlarm => DefaultColor,
    //        LogMessageType.System_Error => "Red",
    //        LogMessageType.System_Warning => "Blue",
    //        LogMessageType.BlockedByServer => "Blue",
    //        LogMessageType.AIRequest => "Purple",
    //        _ => throw new ArgumentOutOfRangeException(),
    //    };

    /// <summary>
    /// 初始化
    /// </summary>
    protected RunLog(LogMessageType logMessageType, BotConfigTargetType targetType, string content)
    {
        _logDate = DateTime.Now;
        LogMessageType = logMessageType;
        TargetType = targetType;
        SenderId = SystemSenderId;
        Content = content;
        IsBlackList = false;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    protected RunLog(LogMessageType logMessageType, BotConfigTargetType targetType, string senderId, string content)
        : this(logMessageType, targetType, content)
    {
        SenderId = senderId;
    }
}

/// <inheritdoc />
public class RunLog_SystemInfo : RunLog
{
    /// <inheritdoc />
    public RunLog_SystemInfo(string content)
        : base(LogMessageType.System_Info, BotConfigTargetType.Common, content)
    { }
}

/// <inheritdoc />
public class RunLog_BotBackgroundInfo : RunLog
{
    /// <inheritdoc />
    public RunLog_BotBackgroundInfo(string content)
        : base(LogMessageType.BotBackground_Info, BotConfigTargetType.Common, content)
    {
        SenderId = "NapCat";
    }
}

/// <inheritdoc />
public class RunLog_SystemWarning : RunLog
{
    /// <inheritdoc />
    public RunLog_SystemWarning(string content)
        : base(LogMessageType.System_Warning, BotConfigTargetType.Common, content)
    { }
}

/// <inheritdoc />
public class RunLog_SystemError : RunLog
{
    /// <inheritdoc />
    public RunLog_SystemError(string content)
        : base(LogMessageType.System_Error, BotConfigTargetType.Common, content)
    { }
}

/// <inheritdoc />
public class RunLog_GroupMessage : RunLog
{
    /// <inheritdoc />
    public RunLog_GroupMessage(GroupMessage groupMessage)
        : base(LogMessageType.GroupMessage, BotConfigTargetType.Group, groupMessage.Sender!.UserId.ToString(), groupMessage.Message!)
    {
        GroupId = groupMessage.GroupId;
        MessageId = groupMessage.MessageId;
    }
}

/// <inheritdoc />
public class RunLog_GroupMessageBlackList : RunLog_GroupMessage
{
    /// <inheritdoc />
    public RunLog_GroupMessageBlackList(GroupMessage groupMessage)
        : base(groupMessage)
    {
        IsBlackList = true;
    }
}

/// <inheritdoc />
public class RunLog_GroupRevokeMessage : RunLog
{
    /// <inheritdoc />
    public RunLog_GroupRevokeMessage(GroupRevokeMessage groupRevokeMessage)
        : base(LogMessageType.GroupRevokeMessage, BotConfigTargetType.Group, groupRevokeMessage.UserId, "撤回消息")
    {
        OperatorId = groupRevokeMessage.OperatorId;
        GroupId = groupRevokeMessage.GroupId;
        MessageId = groupRevokeMessage.MessageId;
    }
}

/// <inheritdoc />
public class RunLog_GroupPoke : RunLog
{
    /// <inheritdoc />
    public RunLog_GroupPoke(GroupPoke groupPoke)
        : base(LogMessageType.GroupPoke, BotConfigTargetType.Group, groupPoke.SenderId, $"[{groupPoke.SenderId}] 戳了戳 [{groupPoke.TargetId}]")
    {
        OperatorId = groupPoke.SenderId;
        GroupId = groupPoke.GroupId;
        TargetId = groupPoke.TargetId;
    }
}

/// <summary>
/// 闹钟助手日志类型
/// </summary>
public class RunLog_AlarmAide : RunLog
{
    /// <inheritdoc />
    public RunLog_AlarmAide(BotConfigTargetType targetType, string targetId, string content)
        : base(LogMessageType.AlarmAide, targetType, targetId, content)
    {
    }
}

/// <summary>
/// 基金助手日志类型
/// </summary>
public class RunLog_FundHelper : RunLog
{
    /// <inheritdoc />
    public RunLog_FundHelper(BotConfigTargetType targetType, string targetId, string content)
        : base(LogMessageType.FundHelper, targetType, targetId, content)
    {
    }
}

/// <summary>
/// 直播提醒日志类型
/// </summary>
public class RunLog_LiveAlarm : RunLog
{
    /// <inheritdoc />
    public RunLog_LiveAlarm(BotConfigTargetType targetType, string otherContent, string targetId, string content)
        : base(LogMessageType.LiveAlarm, targetType, targetId, content)
    {
        OtherContent = otherContent;
    }
}

/// <summary>
/// 风控消息(发送被屏蔽)
/// </summary>
public class RunLog_BlockedByServer : RunLog
{
    /// <inheritdoc />
    public RunLog_BlockedByServer(string message)
        : base(LogMessageType.BlockedByServer, BotConfigTargetType.Common, message)
    {
    }
}

/// <summary>
/// 风控消息(发送被屏蔽)
/// </summary>
public class RunLog_AIRequest : RunLog
{
    /// <inheritdoc />
    public RunLog_AIRequest(
        string requestUserId,
        bool isGroup,
        ChatTokenUsage usage)
        : base(LogMessageType.AIRequest, BotConfigTargetType.Common, $"哈基米AI请求({(isGroup ? $"群:{requestUserId}" : $"个人:{requestUserId}")})")
    {
        TargetId = requestUserId;
        Usage = usage;
    }
}