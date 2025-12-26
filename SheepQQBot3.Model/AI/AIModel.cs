using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    public class AIModel(string model, string url, string key)
    {
        /// <summary>
        /// 模型名
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; init; } = model;

        /// <summary>
        /// 模型请求Url
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; init; } = url;

        /// <summary>
        /// 模型ApiKey
        /// </summary>
        [JsonPropertyName("key")]
        public string Key { get; init; } = key;
    }
}