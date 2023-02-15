using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text.Json.Serialization;
using MessagePack;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// 闹钟助手配置
    /// </summary>
    [MessagePackObject]
    public class AlarmAideConfig : INotifyPropertyChanged
    {
        private bool _isActive;
        private bool _isDefault;

        
        [field: IgnoreMember, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        [Key(nameof(Id))]
        public Guid Id { get; set; }

        /// <summary>
        /// 闹钟名称
        /// </summary>
        [Key(nameof(AlarmName))]
        public string AlarmName { get; set; }

        /// <summary>
        /// 正则表达式条件
        /// </summary>
        [Key(nameof(Condition))]
        public string Condition { get; set; }

        private ConcurrentDictionary<int, string> _alarmTexts;
        /// <summary>
        /// 闹钟消息
        /// </summary>
        [Key(nameof(AlarmTexts))]
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
        [Key(nameof(IsDefault))]
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
        [Key(nameof(IsActive))]
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