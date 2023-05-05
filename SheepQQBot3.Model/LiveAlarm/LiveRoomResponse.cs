using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.LiveAlarm;

public class LiveRoomResponse
{
    [JsonPropertyName("data")]
    public LiveRoomData Data { get; set; }
}

public class LiveRoomData
{
    [JsonPropertyName("room_info")]
    public RoomInfo RoomInfo { get; set; }

    [JsonPropertyName("anchor_info")]
    public AnchorInfo AnchorInfo { get; set; }
}

public enum LiveStatusType
{
    End = 0,
    Live = 1,
}

public class RoomInfo
{
    /// <summary>
    /// 直播间标题
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; }

    /// <summary>
    /// 直播状态
    /// </summary>
    [JsonPropertyName("live_status")]
    public int LiveStatus { get; set; }

    [JsonIgnore]
    public LiveStatusType LiveStatusType => (LiveStatusType)LiveStatus;

    /// <summary>
    /// 直播开始时间
    /// </summary>
    [JsonPropertyName("live_start_time")]
    public long LiveStartTime { get; set; }

    /// <summary>
    /// 直播封面
    /// </summary>
    [JsonPropertyName("cover")]
    public string Cover { get; set; }

    /// <summary>
    /// 直播关键帧画面
    /// </summary>
    [JsonPropertyName("keyframe")]
    public string KeyFrame { get; set; }

    /// <summary>
    /// 直播分区名称-子名称
    /// </summary>
    [JsonPropertyName("area_name")]
    public string AreaName { get; set; }

    /// <summary>
    /// 直播分区名称
    /// </summary>
    [JsonPropertyName("parent_area_name")]
    public string ParentAreaName { get; set; }
}

public class AnchorInfo
{
    [JsonPropertyName("base_info")]
    public UserBaseInfo UserBaseInfo { get; set; }
}

public class UserBaseInfo
{
    [JsonPropertyName("uname")]
    public string Name { get; set; }

    [JsonPropertyName("face")]
    public string Face { get; set; }
}