using System.Text.Json.Serialization;

namespace SheepQQBot3.Model
{
    public class HolidayInfoData
    {
        [JsonPropertyName("holiday")]
        public bool Holiday { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }
    }
}