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
        public DateTime DateTime { get; set; }
        
        public long UserId { get; set; }
        
        public string Anonymous { get; set; }
        
        public int Font { get; set; }
        
        public long GroupId { get; set; }
        
        public string Message { get; set; }
        
        public string RawMessage { get; set; }
        
        public int MessageId { get; set; }
        
        public Sender Sender { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public GroupMessage(ReceiveData receiveData)
        {
            DateTime = receiveData.DateTime;
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
            DateTime = clientData.DateTime;
            GroupId = clientData.GroupId;
            Message = clientData.Message;
            RawMessage = clientData.RawMessage;
            MessageId = clientData.MessageId;
            Sender = clientData.Sender;
        }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public GroupMessage(HistoryMessage historyMessage)
        {
            DateTime = historyMessage.DateTime;
            UserId = historyMessage.UserId;
            Anonymous = historyMessage.Anonymous;
            Font = historyMessage.Font;
            GroupId = historyMessage.GroupId;
            Message = historyMessage.Message;
            RawMessage = historyMessage.RawMessage;
            MessageId = historyMessage.MessageId;
            Sender = historyMessage.Sender;
        }
    }
}