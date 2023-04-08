using System;
using System.Text.Json.Serialization;
using Yamei.Common;

namespace SheepQQBot3.Model
{
    /// <summary>
    /// 群消息
    /// </summary>
    public class GroupMessage
    {
        [JsonPropertyName("datetime")]
        public DateTime DateTime { get; set; }

        [JsonPropertyName("userid")]
        public long UserId { get; set; }

        [JsonPropertyName("anonymous")]
        public string Anonymous { get; set; }

        [JsonPropertyName("font")]
        public int Font { get; set; }

        [JsonPropertyName("groupid")]
        public long GroupId { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("rawmessage")]
        public string RawMessage { get; set; }

        [JsonPropertyName("messageid")]
        public int MessageId { get; set; }

        [JsonPropertyName("sender")]
        public Sender Sender { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public GroupMessage(ReceiveData receiveData)
        {
            DateTime = receiveData.Time.ToDateTime();
            UserId = receiveData.UserId;
            Anonymous = receiveData.Anonymous;
            Font = receiveData.Font;
            GroupId = receiveData.GroupId;
            Message = receiveData.Message;
            RawMessage = receiveData.RawMessage;
            MessageId = receiveData.MessageId;
            Sender = receiveData.Sender;
        }

        public GroupMessage(ClientData clientData)
        {
            DateTime = clientData.Time.ToDateTime();
            GroupId = clientData.GroupId;
            Message = clientData.Message;
            RawMessage = clientData.RawMessage;
            MessageId = clientData.MessageId;
            //LogMessageType = clientData.MessageTargetType;
            Sender = clientData.Sender;
        }
    }
}