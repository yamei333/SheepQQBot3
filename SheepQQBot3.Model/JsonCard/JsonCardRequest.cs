using CommonLibrary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.JsonCard
{
    /// <summary>
    /// QQJson卡片
    /// </summary>
    public class JsonCardRequest
    {
        [JsonPropertyName("ark")]
        public string Ark { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public JsonCardRequest(string ark)
        {
            Ark = ark;
        }

        public JsonCardRequest(JsonCard_TianxuanShare jsonCard)
        {
            Ark = JsonSerializer.Serialize(jsonCard, JsonExtensions.DefaultJsonOptions);
        }
    }
}