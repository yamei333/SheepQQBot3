using System;
using System.Text.Json.Serialization;
using Yamei.Common;

namespace SheepQQBot3.Model
{
    public class GroupRevokeMessage
    {
        [JsonPropertyName("datetime")]
        public DateTime DateTime { get; set; }

        [JsonPropertyName("operatorid")]
        public long OperatorId { get; set; }

        [JsonPropertyName("userid")]
        public long UserId { get; set; }

        [JsonPropertyName("groupid")]
        public long GroupId { get; set; }

        [JsonPropertyName("messageid")]
        public int MessageId { get; set; }

        [JsonPropertyName("groupmessage")]
        public GroupMessage GroupMessage { get; set; }

        public GroupRevokeMessage(ReceiveData receiveData)
        {
            DateTime = receiveData.Time.ToDateTime();
            OperatorId = receiveData.Operator_Id;
            UserId = receiveData.User_Id;
            GroupId = receiveData.Group_Id;
            MessageId = receiveData.Message_Id;
        }
    }
}