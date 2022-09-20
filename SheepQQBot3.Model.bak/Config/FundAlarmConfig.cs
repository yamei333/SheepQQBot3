using System.Collections.Concurrent;
using System.ComponentModel;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// 基金播报配置
    /// </summary>
    public class FundAlarmConfig : INotifyPropertyChanged
    {
        private ConcurrentDictionary<int, AlarmFundConfig> _alarmFundConfigs;
        private bool _isActive;

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Guid ConfigId { get; set; }

        /// <summary>
        /// 播报名称
        /// </summary>
        public string AlarmName { get; set; }

        /// <summary>
        /// 正则表达式条件
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// 播报基金配置
        /// </summary>
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
        public string FundId { get; set; }

        /// <summary>
        /// 基金备注
        /// </summary>
        public string FundRemark { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; }
    }
}