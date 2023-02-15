using System.Text.Json.Serialization;

namespace SheepQQBot3.Model
{
    public class ClientData
    {
        [JsonPropertyName("group")]
        public bool Group { get; set; }

        [JsonPropertyName("group_id")]
        public long Group_Id { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("message_id")]
        public int Message_Id { get; set; }

        [JsonPropertyName("message_seq")]
        public int Message_Seq { get; set; }

        [JsonPropertyName("message_type")]
        public string Message_Type { get; set; }

        [JsonPropertyName("raw_message")]
        public string Raw_Message { get; set; }

        [JsonPropertyName("read_id")]
        public int Read_Id { get; set; }

        [JsonPropertyName("time")]
        public int Time { get; set; }

        [JsonPropertyName("sender")]
        public Sender Sender { get; set; }
    }
}