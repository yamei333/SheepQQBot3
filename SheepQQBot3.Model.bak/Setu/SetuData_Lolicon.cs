using Newtonsoft.Json;

namespace SheepQQBot3.Model.Setu
{
    public class SetuResponse_Lolicon
    {
        public SetuData_Lolicon[] Data { get; set; }
    }

    public class SetuData_Lolicon
    {
        public SetuData_Lolicon_Url Urls { get; set; }

        /// <summary>
        /// 画师
        /// </summary>
        public string author { get; set; }

        /// <summary>
        /// pixiv 图片ID
        /// </summary>
        public string Pid { get; set; }

        [JsonIgnore]
        public string SetuInfo => $"来源:PIXIV 画师:{author} PID:{Pid}";
    }

    public class SetuData_Lolicon_Url
    {
        public string Original { get; set; }
        public string Small { get; set; }
        public string Medium { get; set; }
    }
}