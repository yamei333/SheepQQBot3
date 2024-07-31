using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.JsonCard
{
    /// <summary>
    /// NTQQCookies
    /// </summary>
    public class NTQQCookies
    {
        [JsonPropertyName("data")]
        public NTQQCookies_Data Data { get; set; }
    }

    public class NTQQCookies_Data
    {
        [JsonPropertyName("bkn")]
        public string Bkn { get; set; }
    }
}