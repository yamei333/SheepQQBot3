using SheepQQBot3.Model.Extension;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Setu;

public class SetuData_JitsuSelf
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("pics")]
    public string[] Urls { get; set; }

    /// <summary>
    /// pixiv 图片ID
    /// </summary>
    public string Pid => SetuExtensions.RegGetPixivPid.Match(Urls[0]).Value;

    [JsonIgnore]
    public string SetuInfo => $"PID:{Pid}";
}