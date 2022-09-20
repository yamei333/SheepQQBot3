using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Setu
{
    public class SetuResponse_Lolicon
    {
        [JsonPropertyName("data")]
        public SetuData_Lolicon[] Data { get; set; }
    }

    public class SetuData_Lolicon
    {
        [JsonPropertyName("urls")]
        public SetuData_Lolicon_Url Urls { get; set; }

        /// <summary>
        /// 画师
        /// </summary>
        [JsonPropertyName("author")]
        public string Author { get; set; }

        /// <summary>
        /// pixiv 图片ID
        /// </summary>
        [JsonPropertyName("pid")]
        public string Pid { get; set; }

        [JsonIgnore]
        public string SetuInfo => $"来源:PIXIV 画师:{Author} PID:{Pid}";
    }

    public class SetuData_Lolicon_Url
    {
        [JsonPropertyName("original")]
        public string Original { get; set; }

        [JsonPropertyName("small")]
        public string Small { get; set; }

        [JsonPropertyName("medium")]
        public string Medium { get; set; }
    }
}