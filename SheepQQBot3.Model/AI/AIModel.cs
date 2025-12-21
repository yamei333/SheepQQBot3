using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    public class AIModel(string model, bool supportImage)
    {
        /// <summary>
        /// 模型名
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; init; } = model;

        /// <summary>
        /// 是否支持生成图片
        /// </summary>
        [JsonPropertyName("support_image")]
        public bool SupportImage { get; init; } = supportImage;
    }
}