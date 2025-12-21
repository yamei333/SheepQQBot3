using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.LiveAlarm;

public class BilibiliLive_Response
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("data")]
    public BilibiliLive_Data Data { get; set; }
}

public class BilibiliLive_Data
{
    [JsonPropertyName("room_info")]
    public BilibiliLive_RoomInfo RoomInfo { get; set; }

    [JsonPropertyName("anchor_info")]
    public BilibiliLive_AnchorInfo AnchorInfo { get; set; }
}

public class BilibiliLive_RoomInfo
{
    /// <summary>
    /// 直播间标题
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; }

    /// <summary>
    /// 直播间标题
    /// </summary>
    [JsonPropertyName("room_id")]
    public int RoomId { get; set; }

    /// <summary>
    /// 直播状态
    /// </summary>
    [JsonPropertyName("live_status")]
    public LiveStatusType LiveStatus { get; set; }

    /// <summary>
    /// 直播开播时间
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
}

[JsonConverter(typeof(JsonNumberEnumConverter<LiveStatusType>))]
public enum LiveStatusType
{
    End = 0,
    Live = 1,
    Round = 2,
}

public class BilibiliLive_AnchorInfo
{
    [JsonPropertyName("base_info")]
    public BilibiliLive_UserBaseInfo BaseInfo { get; set; }
}

public class BilibiliLive_UserBaseInfo
{
    [JsonPropertyName("uname")]
    public string Name { get; set; }

    /// <summary>
    /// 用户头像
    /// </summary>
    [JsonPropertyName("face")]
    public string Face { get; set; }
}