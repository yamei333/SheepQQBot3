using System;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using MessagePack;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// 基金阈值观测配置
    /// </summary>
    [MessagePackObject]
    public class FundLimitObserveConfig : NotifyPropertyChangedConfigBase
    {
        /// <summary>
        /// 阈值观测名称
        /// </summary>
        [Key(nameof(LimitObserveName))]
        public string LimitObserveName { get; set; }

        /// <summary>
        /// 正则表达式条件
        /// </summary>
        [Key(nameof(Condition))]
        public string Condition { get; set; }

        [Key(nameof(_limitObserveFundConfigs))]
        private ConcurrentDictionary<int, LimitObserveFundConfig> _limitObserveFundConfigs;

        /// <summary>
        /// 阈值观测基金配置
        /// </summary>
        [IgnoreMember]
        public ConcurrentDictionary<int, LimitObserveFundConfig> LimitObserveFundConfigs
        {
            get => _limitObserveFundConfigs;
            set
            {
                _limitObserveFundConfigs = value;
                OnPropertyChanged(nameof(LimitObserveFundConfigs));
            }
        }

        [Key(nameof(_isActive))]
        private bool _isActive;

        /// <summary>
        /// 是否启用
        /// </summary>
        [IgnoreMember]
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public FundLimitObserveConfig(
            Guid id,
            string limitObserveName,
            string condition,
            bool isActive = false)
        {
            Id = id;
            LimitObserveName = limitObserveName;
            Condition = condition;
            _isActive = isActive;
            _limitObserveFundConfigs = new ConcurrentDictionary<int, LimitObserveFundConfig>();
        }
    }

    /// <summary>
    /// 阈值观测基金配置
    /// </summary>
    [MessagePackObject]
    public class LimitObserveFundConfig
    {
        /// <summary>
        /// 基金编号
        /// </summary>
        [Key(nameof(FundId))]
        public string FundId { get; set; }

        /// <summary>
        /// 观察类型
        /// </summary>
        [Key(nameof(FundObserveType))]
        public FundObserveType FundObserveType { get; set; }

        /// <summary>
        /// 播报阈值
        /// </summary>
        [Key(nameof(AlertLimit))]
        public float AlertLimit { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Key(nameof(IsActive))]
        public bool IsActive { get; set; }

        /// <summary>
        /// 播报阈值
        /// </summary>
        [IgnoreMember]
        [JsonIgnore]
        public string AlertLimitString => $"{AlertLimit:0.00}";

        [IgnoreMember]
        [JsonIgnore]
        public string FundObserveTypeString =>
            FundObserveType switch
            {
                FundObserveType.Week => "周",
                FundObserveType.Month => "月",
                FundObserveType.ThreeMonths => "3月",
                FundObserveType.SixMonths => "半年",
                FundObserveType.Year => "年",
                _ => throw new ArgumentOutOfRangeException()
            };

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
}