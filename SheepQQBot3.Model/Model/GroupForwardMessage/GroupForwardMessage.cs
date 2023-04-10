using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model
{
    public class GroupForwardMessage
    {
        [JsonPropertyName("id")]
        public int? MessageId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("uin")]
        public string UserId { get; set; }

        [JsonPropertyName("content")]
        public List<Element> Message { get; set; }

        public GroupForwardMessage(string name, long userId, string message)
        {
            Name = name;
            UserId = userId.ToString();
            Message = MessageUtil.ProcessCQMessage(message);
        }

        public GroupForwardMessage(int messageId)
        {
            MessageId = messageId;
        }
    }
}