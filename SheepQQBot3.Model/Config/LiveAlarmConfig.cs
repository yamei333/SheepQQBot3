using System;
using System.Text.Json.Serialization;
using MessagePack;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// 直播提醒配置
    /// </summary>
    [MessagePackObject]
    public class LiveAlarmConfig : NotifyPropertyChangedConfigBase
    {
        /// <summary>
        /// 直播房间号
        /// </summary>
        [Key(nameof(LiveRoomId))]
        public long LiveRoomId { get; set; }

        /// <summary>
        /// 直播类型(平台)
        /// </summary>
        [Key(nameof(LiveType))]
        public LiveType LiveType { get; set; }

        /// <summary>
        /// <see cref="LiveType"/>的说明
        /// </summary>
        [IgnoreMember]
        [JsonIgnore]
        public string LiveTypeString
            => LiveType switch
            {
                LiveType.Bilibili => "B站",
                _ => string.Empty
            };

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
        public LiveAlarmConfig(
            Guid id,
            LiveType liveType,
            long liveRoomId,
            bool isActive = false)
        {
            Id = id;
            LiveType = liveType;
            LiveRoomId = liveRoomId;
            _isActive = isActive;
        }
    }
}