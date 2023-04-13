using System;
using MessagePack;

namespace SheepQQBot3.Model
{
    /// <summary>
    /// 配置类基类
    /// </summary>
    [MessagePackObject]
    public abstract class NotifyPropertyChangedConfigBase : NotifyPropertyChangedBase
    {
        /// <summary>
        /// ID
        /// </summary>
        [Key(nameof(Id))]
        public Guid Id { get; set; }

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

        /// <inheritdoc />
        protected NotifyPropertyChangedConfigBase()
        {
            _isActive = false;
        }
    }
}