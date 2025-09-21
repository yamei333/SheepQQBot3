using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    public class AIConfig
    {
        /// <summary>
        /// Keys和最后一次使用时间
        /// </summary>
        [JsonPropertyName("apiKeys")]
        public ConcurrentDictionary<string, long> ApiKeys { get; set; }

        /// <summary>
        /// 模型
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; }

        /// <summary>
        /// Model Think Token
        /// </summary>
        [JsonPropertyName("thinkToken")]
        public int ThinkToken { get; set; }

        /// <summary>
        /// Model Temperature
        /// </summary>
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        ///// <summary>
        ///// Model TopP
        ///// </summary>
        //[JsonPropertyName("topP")]
        //public double TopP { get; set; }

        ///// <summary>
        ///// Model TopK
        ///// </summary>
        //[JsonPropertyName("topK")]
        //public int TopK { get; set; }

        ///// <summary>
        ///// Model FrequencyPenalty
        ///// </summary>
        //[JsonPropertyName("frequencyPenalty")]
        //public double FrequencyPenalty { get; set; }

        /// <summary>
        /// 表情包路径
        /// </summary>
        [JsonPropertyName("facePath")]
        public string FacePath { get; set; }

        /// <summary>
        /// OpenWeatherMap Key
        /// </summary>
        [JsonPropertyName("openWeatherMapKey")]
        public string OpenWeatherMapKey { get; set; }

        /// <summary>
        /// 是否使用AI功能
        /// </summary>
        [JsonIgnore]
        public bool IsUseAI => ApiKeys?.Any() == true && !string.IsNullOrEmpty(Model);
    }
}