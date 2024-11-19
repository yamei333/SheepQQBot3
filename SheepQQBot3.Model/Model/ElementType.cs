using System.Text.Json.Serialization;

namespace SheepQQBot3.Model;

/// <summary>
/// 消息节点类型
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ElementType
{
    /// <summary>
    /// 文本消息
    /// </summary>
    text,

    /// <summary>
    /// at消息
    /// </summary>
    at,

    /// <summary>
    /// 默认表情
    /// </summary>
    face,

    /// <summary>
    /// 图片
    /// </summary>
    image,

    record,
    video,
    rps,
    dice,
    shake,
    poke,
    anonymous,
    share,
    contact,
    location,
    music,
    reply,

    /// <summary>
    /// xml消息节点
    /// </summary>
    xml,

    /// <summary>
    /// json消息节点
    /// </summary>
    json,

    /// <summary>
    /// 合并转发节点
    /// </summary>
    node,

    /// <summary>
    /// markdown消息节点
    /// </summary>
    markdown,

    /// <summary>
    /// 转发消息消息节点
    /// </summary>
    forward,

    /// <summary>
    /// 雅美自定义_色图排行
    /// </summary>
    ym_setu_rank,

    /// <summary>
    /// 雅美自定义_消息重定向
    /// </summary>
    ym_redirect,

    /// <summary>
    /// 雅美自定义_系统是否空闲状态
    /// </summary>
    ym_ifnotidle,

    /// <summary>
    /// 雅美自定义_Bark推送消息
    /// </summary>
    ym_bark,
}