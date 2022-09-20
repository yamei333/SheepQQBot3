using System;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model
{
    /// <summary>
    /// 发送Message的节点
    /// </summary>
    [Serializable]
    public sealed class Element
    {
        /// <summary>
        /// 类型
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; } = string.Empty;

        /// <summary>
        /// 节点信息
        /// </summary>
        [JsonPropertyName("data")]
        public ElementBaseData Data { get; set; }

        public Element(string type, ElementBaseData baseData)
        {
            this.Type = type;
            Data = baseData;
        }

        private Element() => Data = new ElementBaseData();

        private Element(ElementBaseData baseData) => Data = baseData;
    }

    /// <summary>
    /// 节点信息, 包含所有字段
    /// </summary>
    [Serializable]
    public class ElementBaseData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("qq")]
        public string QQ { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("file")]
        public string File { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("ignore")]
        public string Ignore { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("lat")]
        public string Lat { get; set; }

        [JsonPropertyName("lon")]
        public string Lon { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("audio")]
        public string Audio { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }

        public ElementBaseData()
        {
        }

        public ElementBaseData(string text) => this.Text = text;
    }
}