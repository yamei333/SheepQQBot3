using System;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// 闹钟助手配置
/// </summary>
public partial class AlarmAideConfig : NotifyPropertyChangedConfigBase
{
    /// <summary>
    /// 闹钟名称
    /// </summary>
    [JsonPropertyName(nameof(AlarmName))]
    public string AlarmName { get; set; }

    /// <summary>
    /// 正则表达式条件
    /// </summary>
    [JsonPropertyName(nameof(Condition))]
    public string Condition { get; set; }

    /// <summary>
    /// 闹钟消息
    /// </summary>
    [JsonPropertyName(nameof(AlarmTexts))]
    public ConcurrentDictionary<int, string> AlarmTexts
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(AlarmTexts));
        }
    }

    /// <summary>
    /// 是否默认投稿项
    /// </summary>
    [JsonPropertyName(nameof(IsDefault))]
    public bool IsDefault
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(IsDefault));
        }
    }

    /// <summary>
    /// 最后一次执行时间
    /// </summary>
    [JsonPropertyName(nameof(LastExecuteDate))]
    public DateTime LastExecuteDate { get; set; } = DateTime.MinValue;

    /// <inheritdoc />
    public AlarmAideConfig(string alarmName, string condition)
    {
        Id = Guid.NewGuid();
        AlarmName = alarmName;
        Condition = condition;
        IsDefault = false;
        AlarmTexts = [];
    }

    /// <inheritdoc />
    public AlarmAideConfig()
    {
    }
}