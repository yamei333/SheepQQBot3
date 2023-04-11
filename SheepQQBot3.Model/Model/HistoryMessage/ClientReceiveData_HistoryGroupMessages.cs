using System.Text.Json.Serialization;

namespace SheepQQBot3.Model
{
    public class ClientReceiveData_HistoryMessages
    {
        [JsonPropertyName("data")]
        public HistoryMessageData Data { get; set; }

        [JsonPropertyName("retcode")]
        public int RetCode { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class HistoryMessageData
    {
        [JsonPropertyName("messages")]
        public HistoryMessage[] Messages { get; set; }
    }
}