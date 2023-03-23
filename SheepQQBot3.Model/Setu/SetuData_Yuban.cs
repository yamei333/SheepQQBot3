using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Setu
{
    public class SetuResponse_Yuban
    {
        [JsonPropertyName("data")]
        public SetuData_Yuban[] Data { get; set; }
    }

    public class SetuData_Yuban
    {
        [JsonPropertyName("urls")]
        public SetuData_Lolicon_Url Urls { get; set; }

        /// <summary>
        /// 图片信息
        /// </summary>
        [JsonPropertyName("artwork")]
        public SetuData_Yuban_Artwork Artwork { get; set; }

        /// <summary>
        /// 画师
        /// </summary>
        [JsonPropertyName("author")]
        public SetuData_Yuban_Author Author { get; set; }

        [JsonIgnore]
        public string SetuInfo => $"来源:PIXIV 画师:{Author.Name} PID:{Artwork.Id}";
    }

    public class SetuData_Yuban_Author
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class SetuData_Yuban_Artwork
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }
    }
}