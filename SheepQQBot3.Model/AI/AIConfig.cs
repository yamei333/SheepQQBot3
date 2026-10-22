using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI;

public class AIConfig
{
    /// <summary>
    /// 最大重试次数(默认值)
    /// </summary>
    public const int DEFAULT_MAX_RETRY_TIMES = 2;

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
    /// 最大重试次数
    /// </summary>
    [JsonPropertyName("maxRetryTimes")]
    public int MaxRetryTimes { get; set; } = DEFAULT_MAX_RETRY_TIMES;

    /// <summary>
    /// 是否使用AI功能
    /// </summary>
    [JsonIgnore]
    public bool IsUseAI => !string.IsNullOrEmpty(ModelChat.Key) && !string.IsNullOrEmpty(ModelChat.Url);
}