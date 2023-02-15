using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text.Json.Serialization;
using MessagePack;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// 基金播报配置
    /// </summary>
    [MessagePackObject]
    public class FundAlarmConfig : INotifyPropertyChanged
    {
        [field: IgnoreMember, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        [Key(nameof(ConfigId))]
        public Guid ConfigId { get; set; }

        /// <summary>
        /// 播报名称
        /// </summary>
        [Key(nameof(AlarmName))]
        public string AlarmName { get; set; }

        /// <summary>
        /// 正则表达式条件
        /// </summary>
        [Key(nameof(Condition))]
        public string Condition { get; set; }

        [Key(nameof(_alarmFundConfigs))]
        private ConcurrentDictionary<int, AlarmFundConfig> _alarmFundConfigs;

        /// <summary>
        /// 播报基金配置
        /// </summary>
        [IgnoreMember]
        public ConcurrentDictionary<int, AlarmFundConfig> AlarmFundConfigs
        {
            get => _alarmFundConfigs;
            set
            {
                _alarmFundConfigs = value;
                OnPropertyChanged(nameof(AlarmFundConfigs));
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
        public FundAlarmConfig(
            Guid configId,
            string alarmName,
            string condition,
            bool isActive = false)
        {
            ConfigId = configId;
            AlarmName = alarmName;
            Condition = condition;
            _isActive = isActive;
            _alarmFundConfigs = new ConcurrentDictionary<int, AlarmFundConfig>();
        }
    }

    /// <summary>
    /// 播报基金配置
    /// </summary>
    [MessagePackObject]
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
        [Key(nameof(FundId))]
        public string FundId { get; set; }

        /// <summary>
        /// 基金备注
        /// </summary>
        [Key(nameof(FundRemark))]
        public string FundRemark { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Key(nameof(IsActive))]
        public bool IsActive { get; set; }
    }
}