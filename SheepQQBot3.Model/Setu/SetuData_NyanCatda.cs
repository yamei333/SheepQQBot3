using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Setu
{
    public class SetuResponse_NyanCatda
    {
        public SetuData_NyanCatda Data { get; set; }
    }

    public class SetuData_NyanCatda
    {
        public List<string> Url { get; set; }

        [JsonIgnore]
        public string SetuInfo => $"来源:PIXIV";
    }
}