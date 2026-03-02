using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Setu;

public class SetuResponse_Mossia
{
    [JsonPropertyName("data")]
    public SetuData_Mossia[] Data { get; set; }
}

public class SetuData_Mossia
{
    [JsonPropertyName("urlsList")]
    public SetuData_Mossia_Url[] Urls { get; set; }

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
    [JsonPropertyName("tagsList")]
    public SetuData_Mossia_Tag[] Tags { get; set; }

    [JsonIgnore]
    public string SetuInfo => $"{Author}\r\n{Pid}";
}

public class SetuData_Mossia_Url
{
    [JsonPropertyName("urlSize")]
    public string UrlSize { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class SetuData_Mossia_Tag
{
    [JsonPropertyName("tagName")]
    public string TagName { get; set; }

    [JsonPropertyName("tagEn")]
    public string TagEn { get; set; }
}