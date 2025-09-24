using Masuit.Tools.Systems;
using SheepQQBot3.Model.Enums;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// 功能配置
/// </summary>
public class BotFunction
{
    /// <summary>
    /// 获得BotFunctionType的Display属性
    /// </summary>
    [JsonIgnore]
    public string DisplayName => BotFunctionType.GetDisplay();

    [JsonPropertyName(nameof(BotFunctionType))]
    public BotFunctionType BotFunctionType { get; }

    /// <summary>
    /// 是否使用中
    /// </summary>
    [JsonPropertyName(nameof(IsUsed))]
    public bool IsUsed { get; set; }

    /// <summary>
    /// 是否允许使用
    /// </summary>
    [JsonIgnore]
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