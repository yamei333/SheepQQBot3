using SheepQQBot3.Model.Enums;
using System;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// 直播提醒配置
/// </summary>
public partial class LiveAlarmConfig : NotifyPropertyChangedConfigBase
{
    /// <summary>
    /// 直播房间号
    /// </summary>
    [JsonPropertyName(nameof(LiveRoomId))]
    public long LiveRoomId { get; set; }

    /// <summary>
    /// 直播类型(平台)
    /// </summary>
    [JsonPropertyName(nameof(LiveType))]
    public LiveType LiveType { get; set; }

    /// <summary>
    /// <see cref="LiveType"/>的说明
    /// </summary>
    [JsonIgnore]
    public string LiveTypeString
        => LiveType switch
        {
            LiveType.Bilibili => "B站",
            _ => string.Empty,
        };

    /// <summary>
    /// 最后一次执行时间
    /// </summary>
    [JsonPropertyName(nameof(LastExecuteDate))]
    public DateTime LastExecuteDate { get; set; } = DateTime.MinValue;

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public LiveAlarmConfig(
        Guid id,
        LiveType liveType,
        long liveRoomId)
    {
        Id = id;
        LiveType = liveType;
        LiveRoomId = liveRoomId;
    }
}