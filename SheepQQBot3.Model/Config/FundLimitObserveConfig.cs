using SheepQQBot3.Model.Enums;
using System;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// 基金阈值观测配置
/// </summary>
public partial class FundLimitObserveConfig : NotifyPropertyChangedConfigBase
{
    /// <summary>
    /// 阈值观测名称
    /// </summary>
    [JsonPropertyName(nameof(LimitObserveName))]
    public string LimitObserveName { get; set; }

    /// <summary>
    /// 正则表达式条件
    /// </summary>
    [JsonPropertyName(nameof(Condition))]
    public string Condition { get; set; }

    [JsonIgnore]
    private ConcurrentDictionary<int, LimitObserveFundConfig> _limitObserveFundConfigs;

    /// <summary>
    /// 阈值观测基金配置
    /// </summary>
    [JsonPropertyName(nameof(LimitObserveFundConfigs))]
    public ConcurrentDictionary<int, LimitObserveFundConfig> LimitObserveFundConfigs
    {
        get => _limitObserveFundConfigs;
        set
        {
            _limitObserveFundConfigs = value;
            OnPropertyChanged(nameof(LimitObserveFundConfigs));
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
    public FundLimitObserveConfig(
        Guid id,
        string limitObserveName,
        string condition)
    {
        Id = id;
        LimitObserveName = limitObserveName;
        Condition = condition;
        _limitObserveFundConfigs = [];
    }
}

/// <summary>
/// 阈值观测基金配置
/// </summary>
public class LimitObserveFundConfig
{
    /// <summary>
    /// 基金编号
    /// </summary>
    [JsonPropertyName(nameof(FundId))]
    public string FundId { get; set; }

    /// <summary>
    /// 观察类型
    /// </summary>
    [JsonPropertyName(nameof(FundObserveType))]
    public FundObserveType FundObserveType { get; set; }

    /// <summary>
    /// 播报阈值
    /// </summary>
    [JsonPropertyName(nameof(AlertLimit))]
    public float AlertLimit { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [JsonPropertyName(nameof(IsActive))]
    public bool IsActive { get; set; }

    /// <summary>
    /// 播报阈值
    /// </summary>
    [JsonIgnore]
    public string AlertLimitString => $"{AlertLimit:0.00}";

    [JsonIgnore]
    public string FundObserveTypeString =>
        FundObserveType switch
        {
            FundObserveType.Week => "周",
            FundObserveType.Month => "月",
            FundObserveType.ThreeMonths => "3月",
            FundObserveType.SixMonths => "半年",
            FundObserveType.Year => "年",
            _ => throw new ArgumentOutOfRangeException(),
        };

    /// <summary>
    /// 最后一次执行时间
    /// </summary>
    [JsonPropertyName(nameof(LastExecuteDate))]
    public DateTime LastExecuteDate { get; set; } = DateTime.MinValue;

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public LimitObserveFundConfig(
        string fundId,
        FundObserveType fundObserveType,
        float alertLimit,
        bool isActive = false)
    {
        FundId = fundId;
        FundObserveType = fundObserveType;
        AlertLimit = alertLimit;
        IsActive = isActive;
    }
}