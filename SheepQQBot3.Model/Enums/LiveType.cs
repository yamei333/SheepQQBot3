using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Enums;

/// <summary>
/// 直播类型(平台)
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LiveType
{
    Bilibili,
}