using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.JsonCard
{
    /// <summary>
    /// QQJson卡片
    /// </summary>
    public class JsonCard(string ark)
    {
        [JsonPropertyName("ark")]
        public string Ark { get; set; } = ark;
    }
}