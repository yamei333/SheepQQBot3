using System;
using System.Text;
using System.Text.Json.Serialization;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;

namespace SheepQQBot3.Model.Config
{
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
        public bool IsGroupMessage => MessageType == LogMessageType.GroupMessage;

        /// <summary>
        /// 是否有GroudId
        /// </summary>
        [JsonIgnore]
        public bool HasGroupId => !string.IsNullOrEmpty(GroupId);

        public LogMessageType MessageType { get; set; }

        public string OperatorId { get; set; }
        public string SenderId { get; set; }
        public string GroupId { get; set; }
        public string TargetId { get; set; }
        public string MessageId { get; set; }
        public string Content { get; set; }

        private readonly DateTime _logDate;

        public string MessageColor
            => MessageType switch
            {
                LogMessageType.MetaData => DefaultColor,
                LogMessageType.GroupMessage => DefaultColor,
                LogMessageType.GroupRevokeMessage => DefaultColor,
                LogMessageType.GroupPoke => DefaultColor,
                LogMessageType.System_Info => DefaultColor,
                LogMessageType.AlarmAide => DefaultColor,
                LogMessageType.FundHelper => DefaultColor,
                LogMessageType.System_Error => "Red",
                LogMessageType.System_Warning => "Blue",
                _ => throw new ArgumentOutOfRangeException()
            };

        public string MessageTypeStr
            => MessageType switch
            {
                LogMessageType.MetaData => "元事件",
                LogMessageType.GroupMessage => "群消息",
                LogMessageType.GroupRevokeMessage => "群消息撤回",
                LogMessageType.GroupPoke => "群戳一戳",
                LogMessageType.AlarmAide => "闹钟助手",
                LogMessageType.FundHelper => "基金助手",
                LogMessageType.System_Info => "Bot消息",
                LogMessageType.System_Error => "Bot错误",
                LogMessageType.System_Warning => "Bot警告",
                _ => throw new ArgumentOutOfRangeException()
            };

        protected RunLog(LogMessageType messageType, string content)
        {
            _logDate = DateTime.Now;
            MessageType = messageType;
            SenderId = SystemSenderId;
            Content = content;
        }

        protected RunLog(LogMessageType messageType, long senderId, string content)
        {
            _logDate = DateTime.Now;
            MessageType = messageType;
            SenderId = senderId.ToString();
            Content = content;
        }
    }

    public class RunLog_SystemInfo : RunLog
    {
        public RunLog_SystemInfo(string content) : base(LogMessageType.System_Info, content)
        { }
    }

    public class RunLog_SystemWarning : RunLog
    {
        public RunLog_SystemWarning(string content) : base(LogMessageType.System_Warning, content)
        { }
    }

    public class RunLog_SystemError : RunLog
    {
        public RunLog_SystemError(string content) : base(LogMessageType.System_Error, content)
        { }
    }

    public class RunLog_GroupMessage : RunLog
    {
        public RunLog_GroupMessage(GroupMessage groupMessage)
            : base(LogMessageType.GroupMessage, groupMessage.Sender!.User_Id, groupMessage.Message!)
        {
            GroupId = groupMessage.GroupId.ToString();
        }
    }

    public class RunLog_GroupRevokeMessage : RunLog
    {
        public RunLog_GroupRevokeMessage(GroupRevokeMessage groupRevokeMessage)
            : base(LogMessageType.GroupRevokeMessage, groupRevokeMessage.UserId, "撤回消息")
        {
            OperatorId = groupRevokeMessage.OperatorId.ToString();
            GroupId = groupRevokeMessage.GroupId.ToString();
            MessageId = groupRevokeMessage.MessageId.ToString();
        }
    }

    public class RunLog_GroupPoke : RunLog
    {
        public RunLog_GroupPoke(GroupPoke groupPoke)
            : base(LogMessageType.GroupPoke, groupPoke.SenderId, $"[{groupPoke.SenderId}] 戳了戳 [{groupPoke.TargetId}]")
        {
            OperatorId = groupPoke.SenderId.ToString();
            GroupId = groupPoke.GroupId.ToString();
            TargetId = groupPoke.TargetId.ToString();
        }
    }

    public class RunLog_AlarmAide : RunLog
    {
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

    public class RunLog_FundHelper : RunLog
    {
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
}