using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Model.GetIP
{
    public class Ipify
    {
        [JsonPropertyName("ip")]
        public string IP { get; set; }
    }
}