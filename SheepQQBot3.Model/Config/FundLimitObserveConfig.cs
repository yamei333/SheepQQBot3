using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// 基金阈值观测配置
    /// </summary>
    [Serializable]
    public class FundLimitObserveConfig : INotifyPropertyChanged
    {
        private ConcurrentDictionary<int, LimitObserveFundConfig> _limitObserveFundConfigs;
        private bool _isActive;

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Guid ConfigId { get; set; }

        /// <summary>
        /// 阈值观测名称
        /// </summary>
        public string LimitObserveName { get; set; }

        /// <summary>
        /// 正则表达式条件
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// 阈值观测基金配置
        /// </summary>
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
        /// 是否启用
        /// </summary>
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
            Guid configId,
            string limitObserveName,
            string condition,
            bool isActive = false)
        {
            ConfigId = configId;
            LimitObserveName = limitObserveName;
            Condition = condition;
            _isActive = isActive;
            _limitObserveFundConfigs = new ConcurrentDictionary<int, LimitObserveFundConfig>();
        }
    }

    /// <summary>
    /// 阈值观测基金配置
    /// </summary>
    public class LimitObserveFundConfig
    {
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

        /// <summary>
        /// 基金编号
        /// </summary>
        public string FundId { get; set; }

        /// <summary>
        /// 观察类型
        /// </summary>
        public FundObserveType FundObserveType;

        /// <summary>
        /// 播报阈值
        /// </summary>
        public float AlertLimit { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 播报阈值
        /// </summary>
        public string AlertLimitString => $"{AlertLimit:0.00}";

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
    }
}