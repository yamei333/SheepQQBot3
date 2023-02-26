using System;
using System.Text.Json.Serialization;
using SheepQQBot3.Model.Enums;
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

        [JsonPropertyName("messagetype")]
        public MessageType MessageType { get; set; }

        [JsonPropertyName("sender")]
        public Sender Sender { get; set; }

        public GroupMessage(ReceiveData receiveData)
        {
            DateTime = receiveData.Time.ToDateTime();
            UserId = receiveData.User_Id;
            Anonymous = receiveData.Anonymous;
            Font = receiveData.Font;
            GroupId = receiveData.Group_Id;
            Message = receiveData.Message;
            RawMessage = receiveData.Raw_Message;
            MessageId = receiveData.Message_Id;
            MessageType = receiveData.Message_Type;
            Sender = receiveData.Sender;
        }

        public GroupMessage(ClientData clientData)
        {
            DateTime = clientData.Time.ToDateTime();
            GroupId = clientData.Group_Id;
            Message = clientData.Message;
            RawMessage = clientData.Raw_Message;
            MessageId = clientData.Message_Id;
            //LogMessageType = clientData.Message_Type;
            Sender = clientData.Sender;
        }
    }
}