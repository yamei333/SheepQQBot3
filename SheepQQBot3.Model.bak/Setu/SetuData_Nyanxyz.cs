using Newtonsoft.Json;

namespace SheepQQBot3.Model.Setu
{
    public class SetuResponse_Nyanxyz
    {
        public SetuData_Nyanxyz Data { get; set; }
    }

    public class SetuData_Nyanxyz
    {
        public List<string> Url { get; set; }

        [JsonIgnore]
        public string SetuInfo => $"来源:未知";
    }
}