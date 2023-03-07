using System;
using System.Text;
using System.Text.Json.Serialization;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// 运行日志
    /// </summary>
    public class RunLog
    {
        private const string DefaultColor = "Black";
        private const string SystemSenderId = "系统";

        [JsonIgnore]
        public string DateTimeStr => _logDate.ToString("HH:mm:ss");

        [JsonIgnore]
        public string DateTimeStrFFF => _logDate.ToString("HH:mm:ss.fff");

        [JsonIgnore]
        public string ContentTitle => Content.ByteSubstring(80, "...");

        [JsonIgnore]
        public bool IsWarp => Encoding.Default.GetBytes(Content).Length > 42;

        [JsonIgnore]
        public bool IsGroupMessage => LogMessageType == LogMessageType.GroupMessage;

        /// <summary>
        /// 是否有GroudId
        /// </summary>
        [JsonIgnore]
        public bool HasGroupId => !string.IsNullOrEmpty(GroupId);

        public LogMessageType LogMessageType { get; set; }
        public string MessageType { get; set; }

        public string OperatorId { get; set; }
        public string SenderId { get; set; }
        public string GroupId { get; set; }
        public string TargetId { get; set; }

        /// <summary>
        /// 其他ID
        /// </summary>
        public string OtherId { get; set; }

        public string MessageId { get; set; }
        public string Content { get; set; }
        public bool IsBlackList { get; set; }

        private readonly DateTime _logDate;

        /// <summary>
        /// 消息颜色
        /// </summary>
        public string MessageColor
            => LogMessageType switch
            {
                LogMessageType.MetaData => DefaultColor,
                LogMessageType.GroupMessage => DefaultColor,
                LogMessageType.GroupRevokeMessage => DefaultColor,
                LogMessageType.GroupPoke => DefaultColor,
                LogMessageType.System_Info => DefaultColor,
                LogMessageType.AlarmAide => DefaultColor,
                LogMessageType.FundHelper => DefaultColor,
                LogMessageType.LiveAlarm => DefaultColor,
                LogMessageType.GenshinDailyNoteAlarm => DefaultColor,
                LogMessageType.System_Error => "Red",
                LogMessageType.System_Warning => "Blue",
                LogMessageType.BlockedByServer => "Blue",
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <summary>
        /// <see cref="LogMessageType"/>说明
        /// </summary>
        public string MessageTypeStr
            => LogMessageType switch
            {
                LogMessageType.MetaData => "元事件",
                LogMessageType.GroupMessage => "群消息",
                LogMessageType.GroupRevokeMessage => "群消息撤回",
                LogMessageType.GroupPoke => "群戳一戳",
                LogMessageType.AlarmAide => "闹钟助手",
                LogMessageType.FundHelper => "基金助手",
                LogMessageType.LiveAlarm => "直播提醒",
                LogMessageType.GenshinDailyNoteAlarm => "原神每日提醒",
                LogMessageType.System_Info => "Bot消息",
                LogMessageType.System_Error => "Bot错误",
                LogMessageType.System_Warning => "Bot警告",
                LogMessageType.BlockedByServer => "账号风控",
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <summary>
        /// 初始化
        /// </summary>
        protected RunLog(LogMessageType logMessageType, string content)
        {
            _logDate = DateTime.Now;
            LogMessageType = logMessageType;
            SenderId = SystemSenderId;
            Content = content;
            IsBlackList = false;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected RunLog(LogMessageType logMessageType, long senderId, string content)
            : this(logMessageType, content)
        {
            SenderId = senderId.ToString();
        }
    }

    /// <inheritdoc />
    public class RunLog_SystemInfo : RunLog
    {
        /// <inheritdoc />
        public RunLog_SystemInfo(string content) : base(LogMessageType.System_Info, content)
        { }
    }

    /// <inheritdoc />
    public class RunLog_SystemWarning : RunLog
    {
        /// <inheritdoc />
        public RunLog_SystemWarning(string content) : base(LogMessageType.System_Warning, content)
        { }
    }

    /// <inheritdoc />
    public class RunLog_SystemError : RunLog
    {
        /// <inheritdoc />
        public RunLog_SystemError(string content) : base(LogMessageType.System_Error, content)
        { }
    }

    /// <inheritdoc />
    public class RunLog_GroupMessage : RunLog
    {
        /// <inheritdoc />
        public RunLog_GroupMessage(GroupMessage groupMessage)
            : base(LogMessageType.GroupMessage, groupMessage.Sender!.User_Id, groupMessage.Message!)
        {
            GroupId = groupMessage.GroupId.ToString();
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
            : base(LogMessageType.GroupRevokeMessage, groupRevokeMessage.UserId, "撤回消息")
        {
            OperatorId = groupRevokeMessage.OperatorId.ToString();
            GroupId = groupRevokeMessage.GroupId.ToString();
            MessageId = groupRevokeMessage.MessageId.ToString();
        }
    }

    /// <inheritdoc />
    public class RunLog_GroupPoke : RunLog
    {
        /// <inheritdoc />
        public RunLog_GroupPoke(GroupPoke groupPoke)
            : base(LogMessageType.GroupPoke, groupPoke.SenderId, $"[{groupPoke.SenderId}] 戳了戳 [{groupPoke.TargetId}]")
        {
            OperatorId = groupPoke.SenderId.ToString();
            GroupId = groupPoke.GroupId.ToString();
            TargetId = groupPoke.TargetId.ToString();
        }
    }

    /// <summary>
    /// 闹钟助手日志类型
    /// </summary>
    public class RunLog_AlarmAide : RunLog
    {
        /// <inheritdoc />
        public RunLog_AlarmAide(BotConfigTargetType targetType, long targetId, string content)
            : base(LogMessageType.AlarmAide, targetId, content)
        {
            switch (targetType)
            {
                case BotConfigTargetType.Group:
                    GroupId = "群消息";
                    break;
                case BotConfigTargetType.Private:
                    GroupId = "私聊消息";
                    break;
                case BotConfigTargetType.Common:
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null);
            }
        }
    }

    /// <summary>
    /// 基金助手日志类型
    /// </summary>
    public class RunLog_FundHelper : RunLog
    {
        /// <inheritdoc />
        public RunLog_FundHelper(BotConfigTargetType targetType, long targetId, string content)
            : base(LogMessageType.FundHelper, targetId, content)
        {
            switch (targetType)
            {
                case BotConfigTargetType.Group:
                    GroupId = "群消息";
                    break;
                case BotConfigTargetType.Private:
                    GroupId = "私聊消息";
                    break;
                case BotConfigTargetType.Common:
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null);
            }
        }
    }

    /// <summary>
    /// 直播提醒日志类型
    /// </summary>
    public class RunLog_LiveAlarm : RunLog
    {
        /// <inheritdoc />
        public RunLog_LiveAlarm(BotConfigTargetType targetType, string otherId, long targetId, string content)
            : base(LogMessageType.LiveAlarm, targetId, content)
        {
            OtherId = otherId;
            switch (targetType)
            {
                case BotConfigTargetType.Group:
                    MessageType = "群消息";
                    break;
                case BotConfigTargetType.Private:
                    MessageType = "私聊消息";
                    break;
                case BotConfigTargetType.Common:
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null);
            }
        }
    }

    /// <summary>
    /// 原神每日提醒日志类型
    /// </summary>
    public class RunLog_GenshinDailyNoteAlarm : RunLog
    {
        /// <inheritdoc />
        public RunLog_GenshinDailyNoteAlarm(BotConfigTargetType targetType, long senderId, string content)
            : base(LogMessageType.GenshinDailyNoteAlarm, senderId, content)
        {
            switch (targetType)
            {
                case BotConfigTargetType.Group:
                    MessageType = "群消息";
                    break;
                case BotConfigTargetType.Private:
                    MessageType = "私聊消息";
                    break;
                case BotConfigTargetType.Common:
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null);
            }
        }
    }

    /// <summary>
    /// 风控消息(发送被屏蔽)
    /// </summary>
    public class RunLog_BlockedByServer : RunLog
    {
        /// <inheritdoc />
        public RunLog_BlockedByServer(string message)
            : base(LogMessageType.BlockedByServer, 0, message)
        {
        }
    }
}