using System;
using System.Collections.Concurrent;
using MessagePack;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// 直播提醒配置
    /// </summary>
    [MessagePackObject]
    public class AlarmAideConfig : NotifyPropertyChangedConfigBase
    {
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

        [Key(nameof(_alarmTexts))]
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

        [Key(nameof(_isDefault))]
        private bool _isDefault;

        /// <summary>
        /// 是否默认投稿项
        /// </summary>
        [IgnoreMember]
        public bool IsDefault
        {
            get => _isDefault;
            set
            {
                _isDefault = value;
                OnPropertyChanged(nameof(IsDefault));
            }
        }

        [Key(nameof(_isActive))]
        private bool _isActive;

        /// <inheritdoc />
        public AlarmAideConfig(string alarmName, string condition)
        {
            Id = Guid.NewGuid();
            AlarmName = alarmName;
            Condition = condition;
            IsDefault = false;
            AlarmTexts = new ConcurrentDictionary<int, string>();
        }

        /// <inheritdoc />
        public AlarmAideConfig()
        {
        }
    }
}