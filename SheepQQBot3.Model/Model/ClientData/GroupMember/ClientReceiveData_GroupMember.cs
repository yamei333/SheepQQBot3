using System.Text.Json.Serialization;

namespace SheepQQBot3.Model
{
    public class ClientReceiveData_GroupMember
    {
        [JsonPropertyName("data")]
        public GroupMember[] Data { get; set; }

        [JsonPropertyName("retcode")]
        public int RetCode { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("msg")]
        public string Message { get; set; }

        [JsonPropertyName("wording")]
        public string Wording { get; set; }
    }
}