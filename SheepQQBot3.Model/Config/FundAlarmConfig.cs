using System;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// 基金播报配置
/// </summary>
public partial class FundAlarmConfig : NotifyPropertyChangedConfigBase
{
    /// <summary>
    /// 播报名称
    /// </summary>
    [JsonPropertyName(nameof(AlarmName))]
    public string AlarmName { get; set; }

    /// <summary>
    /// 正则表达式条件
    /// </summary>
    [JsonPropertyName(nameof(Condition))]
    public string Condition { get; set; }

    [JsonIgnore]
    private ConcurrentDictionary<int, AlarmFundConfig> _alarmFundConfigs;

    /// <summary>
    /// 播报基金配置
    /// </summary>
    [JsonPropertyName(nameof(AlarmFundConfigs))]
    public ConcurrentDictionary<int, AlarmFundConfig> AlarmFundConfigs
    {
        get => _alarmFundConfigs;
        set
        {
            _alarmFundConfigs = value;
            OnPropertyChanged(nameof(AlarmFundConfigs));
        }
    }

    /// <summary>
    /// 最后一次执行时间
    /// </summary>
    [JsonPropertyName(nameof(LastExecuteDate))]
    public DateTime LastExecuteDate { get; set; } = DateTime.MinValue;

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public FundAlarmConfig(
        Guid id,
        string alarmName,
        string condition)
    {
        Id = id;
        AlarmName = alarmName;
        Condition = condition;
        _alarmFundConfigs = [];
    }
}

/// <summary>
/// 播报基金配置
/// </summary>
public class AlarmFundConfig
{
    /// <summary>
    /// 默认构造函数
    /// </summary>
    public AlarmFundConfig(
        string fundId,
        string fundRemark,
        bool isActive = false)
    {
        FundId = fundId;
        FundRemark = fundRemark;
        IsActive = isActive;
    }

    /// <summary>
    /// 基金编号
    /// </summary>
    [JsonPropertyName(nameof(FundId))]
    public string FundId { get; set; }

    /// <summary>
    /// 基金备注
    /// </summary>
    [JsonPropertyName(nameof(FundRemark))]
    public string FundRemark { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [JsonPropertyName(nameof(IsActive))]
    public bool IsActive { get; set; }
}