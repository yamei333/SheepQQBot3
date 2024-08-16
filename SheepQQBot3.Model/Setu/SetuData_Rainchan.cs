using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Setu;

public class SetuData_Rainchan
{
    /// <summary>
    /// pixiv 图片ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// 画师
    /// </summary>
    [JsonPropertyName("userName")]
    public string UserName { get; set; }

    [JsonIgnore]
    public string SetuInfo => $"画师:{UserName}\r\nPID:{Id}";
}