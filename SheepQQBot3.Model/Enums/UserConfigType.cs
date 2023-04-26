using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Enums;

/// <summary>
/// 用户自定义配置类型
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserConfigType
{
    /// <summary>
    /// BarkKey
    /// </summary>
    [Display(Name = "BarkKey")]
    BarkKey,
}