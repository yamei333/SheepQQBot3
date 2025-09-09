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
        /// 表情包路径
        /// </summary>
        [JsonPropertyName("facePath")]
        public string FacePath { get; set; }

        /// <summary>
        /// 知识库路径
        /// </summary>
        [JsonPropertyName("knowledge")]
        public string KnowledgePath { get; set; }

        /// <summary>
        /// OpenWeatherMap Key
        /// </summary>
        [JsonPropertyName("openWeatherMapKey")]
        public string OpenWeatherMapKey { get; set; }

        /// <summary>
        /// 是否参与群聊
        /// </summary>
        [JsonPropertyName("useGroupChat")]
        public ConcurrentDictionary<long, bool> UseGroupChat { get; set; } = [];

        /// <summary>
        /// 是否使用AI功能
        /// </summary>
        [JsonIgnore]
        public bool IsUseAI => ApiKeys?.Any() == true && !string.IsNullOrEmpty(Model);
    }
}