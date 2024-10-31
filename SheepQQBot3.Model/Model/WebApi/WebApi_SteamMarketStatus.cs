using System;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Model.WebApi;

[Serializable]
public class WebApi_SteamMarketStatus
{
    /// <summary>
    /// 验证用字段
    /// </summary>
    [JsonPropertyName("sheepqqbot3")]
    public string SheepQQBot3 { get; set; }
}