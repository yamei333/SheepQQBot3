using System.Text.Json.Serialization;

namespace SheepQQBot3.Model
{
    public class ClientReceiveData
    {
        [JsonPropertyName("data")]
        public ClientData Data { get; set; }

        [JsonPropertyName("retcode")]
        public int RetCode { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonIgnore]
        public bool IsSuccessed => RetCode == 0;

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("msg")]
        public string Message { get; set; }

        [JsonPropertyName("wording")]
        public string Wording { get; set; }

        [JsonPropertyName("echo")]
        public string Echo { get; set; }
    }
}