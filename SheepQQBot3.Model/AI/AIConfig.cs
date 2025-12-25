using Masuit.Tools;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    public class AIConfig
    {
        /// <summary>
        /// API Key
        /// </summary>
        [JsonPropertyName("apiKey_chat")]
        public string ApiKeyChat { get; set; }

        /// <summary>
        /// API Key Image
        /// </summary>
        [JsonPropertyName("apiKey_image")]
        public string ApiKeyImage { get; set; }

        /// <summary>
        /// BaseUrl
        /// </summary>
        [JsonPropertyName("baseUrl_chat")]
        public string BaseUrlChat { get; set; }

        /// <summary>
        /// BaseUrl
        /// </summary>
        [JsonPropertyName("baseUrl_image")]
        public string BaseUrlImage { get; set; }

        /// <summary>
        /// 聊天模型
        /// </summary>
        [JsonPropertyName("model_chat")]
        public AIModel ModelChat { get; set; }

        /// <summary>
        /// 生成图片模型
        /// </summary>
        [JsonPropertyName("model_image")]
        public AIModel ModelImage { get; set; }

        /// <summary>
        /// 群聊总结模型
        /// </summary>
        [JsonPropertyName("model_summary")]
        public AIModel ModelSummary { get; set; }

        /// <summary>
        /// Model Think Token
        /// </summary>
        [JsonPropertyName("thinkToken")]
        public int ThinkToken { get; set; }

        /// <summary>
        /// Model Temperature
        /// </summary>
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }

        /// <summary>
        /// Model TopP
        /// </summary>
        [JsonPropertyName("topP")]
        public float TopP { get; set; }

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
        /// Max Token
        /// </summary>
        [JsonPropertyName("maxToken")]
        public int MaxToken { get; set; }

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
        public bool IsUseAI => !ApiKeyChat.IsNullOrEmpty() && !string.IsNullOrEmpty(ModelChat.Model);
    }
}