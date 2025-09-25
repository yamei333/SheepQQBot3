using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    public class GroupMemberInfo
    {
        [JsonPropertyName("userInfos")]
        public ConcurrentDictionary<long, AIChatSender> UserInfos { get; set; }

        public GroupMemberInfo()
        {
        }
    }
}