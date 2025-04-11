using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.LiveAlarm;

public class LiveRoomData
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
    public int LiveStatus { get; set; }

    [JsonIgnore]
    public LiveStatusType LiveStatusType => (LiveStatusType)LiveStatus;

    /// <summary>
    /// 直播开播时间
    /// </summary>
    [JsonPropertyName("live_time")]
    public long LiveStartTime { get; set; }

    /// <summary>
    /// 直播封面
    /// </summary>
    [JsonPropertyName("cover_from_user")]
    public string Cover { get; set; }

    /// <summary>
    /// 直播关键帧画面
    /// </summary>
    [JsonPropertyName("keyframe")]
    public string KeyFrame { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [JsonPropertyName("uname")]
    public string Name { get; set; }

    /// <summary>
    /// 用户头像
    /// </summary>
    [JsonPropertyName("face")]
    public string Face { get; set; }
}

public enum LiveStatusType
{
    End = 0,
    Live = 1,
    Live2 = 2,
}

//public class AnchorInfo
//{
//    [JsonPropertyName("base_info")]
//    public UserBaseInfo UserBaseInfo { get; set; }
//}

//public class UserBaseInfo
//{
//    [JsonPropertyName("uname")]
//    public string Name { get; set; }

//    /// <summary>
//    /// 用户头像
//    /// </summary>
//    [JsonPropertyName("face")]
//    public string Face { get; set; }
//}