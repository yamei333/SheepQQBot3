using System;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model;

/// <summary>
/// 发送Message的节点
/// </summary>
[Serializable]
public sealed class GroupForwardMessageElement
{
    /// <summary>
    /// 类型
    /// </summary>
    [JsonPropertyName("type")]
    public ElementType Type { get; } = ElementType.node;

    /// <summary>
    /// 节点信息
    /// </summary>
    [JsonPropertyName("data")]
    public GroupForwardMessage Data { get; set; }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public GroupForwardMessageElement(GroupForwardMessage message)
    {
        Data = message;
    }
}