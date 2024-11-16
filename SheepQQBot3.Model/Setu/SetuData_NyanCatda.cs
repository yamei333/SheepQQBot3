using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Setu;

public class SetuResponse_NyanCatda
{
    [JsonPropertyName("data")]
    public SetuData_NyanCatda[] Data { get; set; }
}

public class SetuData_NyanCatda
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    /// <summary>
    /// 画师
    /// </summary>
    [JsonPropertyName("author")]
    public string Author { get; set; }

    /// <summary>
    /// pixiv 图片ID
    /// </summary>
    [JsonPropertyName("pid")]
    public int Pid { get; set; }

    /// <summary>
    /// pixiv Tag
    /// </summary>
    [JsonPropertyName("tags")]
    public HashSet<string> Tags { get; set; }

    [JsonIgnore]
    public string SetuInfo => $"{Author}\r\n{Pid}";
}