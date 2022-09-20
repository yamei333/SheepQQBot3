using Newtonsoft.Json;

namespace SheepQQBot3.Model.Setu
{
    public class SetuResponse_Yuban
    {
        public SetuData_Yuban[] Data { get; set; }
    }

    public class SetuData_Yuban
    {
        public SetuData_Lolicon_Url Urls { get; set; }

        /// <summary>
        /// 图片信息
        /// </summary>
        public SetuData_Yuban_Artwork Artwork { get; set; }

        /// <summary>
        /// 画师
        /// </summary>
        public SetuData_Yuban_Author Author { get; set; }

        [JsonIgnore]
        public string SetuInfo => $"来源:PIXIV 画师:{Author.Name} PID:{Artwork.Id}";
    }

    public class SetuData_Yuban_Author
    {
        public string Name { get; set; }
    }

    public class SetuData_Yuban_Artwork
    {
        public string Id { get; set; }
    }
}