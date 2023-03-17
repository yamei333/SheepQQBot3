using System.Text.Json.Serialization;

namespace SheepQQBot3.Model
{
    public class PushBarkRequest
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("timestamp")]
        public int TimeStamp { get; set; }
    }
}