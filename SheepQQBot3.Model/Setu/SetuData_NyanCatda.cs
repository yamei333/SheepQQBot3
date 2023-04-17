using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Setu;

public class SetuResponse_NyanCatda
{
    [JsonPropertyName("data")]
    public SetuData_NyanCatda Data { get; set; }
}

public class SetuData_NyanCatda
{
    [JsonPropertyName("url")]
    public List<string> Url { get; set; }

    [JsonIgnore]
    public string SetuInfo => $"来源:PIXIV";
}