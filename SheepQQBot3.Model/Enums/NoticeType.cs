using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NoticeType
{
    /// <summary>
    /// 群消息撤回
    /// </summary>
    Group_Recall,

    /// <summary>
    /// 好友消息撤回
    /// </summary>
    Friend_Recall,

    /// <summary>
    /// 通知信息
    /// </summary>
    Notify,

    /// <summary>
    /// 其他客户端在线状态变更
    /// </summary>
    Client_Status,

    /// <summary>
    /// 上传群文件
    /// </summary>
    Group_Upload,

    /// <summary>
    /// 群名片变更
    /// </summary>
    Group_Card,

    /// <summary>
    /// 接收到离线文件
    /// </summary>
    Offline_File,

    /// <summary>
    /// 精华消息
    /// </summary>
    Essence,

    /// <summary>
    /// 频道消息表情贴更新
    /// </summary>
    Message_Reactions_Updated,

    /// <summary>
    /// 子频道信息更新
    /// </summary>
    Channel_Updated,

    /// <summary>
    /// 子频道创建
    /// </summary>
    Channel_Created,

    /// <summary>
    /// 子频道删除
    /// </summary>
    Channel_Destroyed,

    /// <summary>
    /// 群员增加事件
    /// </summary>
    Group_Increase,

    /// <summary>
    /// 群员减少事件(退群)
    /// </summary>
    Group_Decrease,

    /// <summary>
    ///
    /// </summary>
    Friend_Add,

    /// <summary>
    ///
    /// </summary>
    Group_Admin,

    /// <summary>
    /// 群禁言
    /// </summary>
    Group_Ban,

    /// <summary>
    /// 群消息表情回应
    /// </summary>
    Group_Msg_Emoji_Like,
}