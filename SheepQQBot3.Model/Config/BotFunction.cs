using System.Text.Json.Serialization;
using Masuit.Tools.Systems;
using MessagePack;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// 功能配置
/// </summary>
[MessagePackObject]
public class BotFunction
{
    /// <summary>
    /// 获得BotFunctionType的Display属性
    /// </summary>
    [JsonIgnore]
    [IgnoreMember]
    public string DisplayName => BotFunctionType.GetDisplay();

    [Key(nameof(BotFunctionType))]
    public BotFunctionType BotFunctionType { get; }

    /// <summary>
    /// 是否使用中
    /// </summary>
    [Key(nameof(IsUsed))]
    public bool IsUsed { get; set; }

    /// <summary>
    /// 是否允许使用
    /// </summary>
    [IgnoreMember]
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 初始化
    /// </summary>
    public BotFunction(BotFunctionType botFunctionType, bool isUsed)
    {
        BotFunctionType = botFunctionType;
        IsUsed = isUsed;
    }
}