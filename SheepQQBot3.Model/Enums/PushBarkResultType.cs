using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PushBarkResultType
{
    /// <summary>
    /// 推送成功
    /// </summary>
    Success,

    /// <summary>
    /// 推送失败
    /// </summary>
    Failed,

    /// <summary>
    /// 发起推送失败
    /// </summary>
    PushError,
}