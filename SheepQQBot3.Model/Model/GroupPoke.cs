using System;
using System.Text.Json.Serialization;
using Yamei.Common;

namespace SheepQQBot3.Model
{
    public class GroupPoke
    {
        [JsonPropertyName("datetime")]
        public DateTime DateTime { get; set; }

        [JsonPropertyName("targetid")]
        public long TargetId { get; set; }

        [JsonPropertyName("groupid")]
        public long GroupId { get; set; }

        [JsonPropertyName("senderid")]
        public long SenderId { get; set; }

        public GroupPoke(ReceiveData receiveData)
        {
            DateTime = receiveData.Time.ToDateTime();
            SenderId = receiveData.Sender_Id;
            TargetId = receiveData.Target_Id;
            GroupId = receiveData.GroupId;
        }
    }
}