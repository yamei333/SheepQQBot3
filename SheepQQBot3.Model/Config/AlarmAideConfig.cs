using System;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// 闹钟助手配置
    /// </summary>
    [Serializable]
    public class AlarmAideConfig : INotifyPropertyChanged
    {
        private bool _isActive;
        private bool _isDefault;

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// 唯一标识
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 闹钟名称
        /// </summary>
        public string AlarmName { get; set; }

        /// <summary>
        /// 正则表达式条件
        /// </summary>
        public string Condition { get; set; }

        private ConcurrentDictionary<int, string> _alarmTexts;
        /// <summary>
        /// 闹钟消息
        /// </summary>
        public ConcurrentDictionary<int, string> AlarmTexts
        {
            get => _alarmTexts;
            set
            {
                _alarmTexts = value;
                OnPropertyChanged(nameof(AlarmTexts));
            }
        }

        /// <summary>
        /// 是否默认投稿项
        /// </summary>
        public bool IsDefault
        {
            get => _isDefault;
            set
            {
                _isDefault = value;
                OnPropertyChanged(nameof(IsDefault));
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

        public AlarmAideConfig(string alarmName, string condition)
        {
            Id = Guid.NewGuid();
            AlarmName = alarmName;
            Condition = condition;
            IsActive = false;
            IsDefault = false;
            AlarmTexts = new ConcurrentDictionary<int, string>();
        }

        public AlarmAideConfig()
        {
        }
    }
}