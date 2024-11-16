using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Setu;

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
    public int Pid { get; set; }

    /// <summary>
    /// pixiv Tag
    /// </summary>
    [JsonPropertyName("tags")]
    public HashSet<string> Tags { get; set; }

    [JsonIgnore]
    public string SetuInfo => $"{Author}\r\n{Pid}";
}

public class SetuData_Lolicon_Url
{
    [JsonPropertyName("original")]
    public string Original { get; set; }
}