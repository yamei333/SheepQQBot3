using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Setu;

/// <summary>
/// SetuResponse_Lolisuki
/// </summary>
/// <remarks>复制于<see cref="SetuResponse_Lolicon"/></remarks>
public class SetuResponse_Lolisuki
{
    [JsonPropertyName("data")]
    public SetuData_Lolisuki[] Data { get; set; }
}

public class SetuData_Lolisuki
{
    [JsonPropertyName("urls")]
    public SetuData_Lolisuki_Url Urls { get; set; }

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

    [JsonIgnore]
    public string SetuInfo => $"来源:[Lolisuki]PIXIV 画师:{Author} PID:{Pid}";
}

public class SetuData_Lolisuki_Url
{
    [JsonPropertyName("original")]
    public string Original { get; set; }
}