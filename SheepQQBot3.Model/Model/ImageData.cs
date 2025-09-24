using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Model
{
    public class ImageData
    {
        [JsonPropertyName("file")]
        public string File { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("string")]
        public string FileSize { get; set; }

        [JsonPropertyName("file_name")]
        public string FileName { get; set; }

        [JsonPropertyName("base64")]
        public string FileBase64 { get; set; }
    }
}