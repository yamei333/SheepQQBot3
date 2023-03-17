using SheepQQBot3.Model;

namespace SheepQQBot3.View
{
    public class AddGenshinDailyNoteAlarmDialogViewModel : NotifyPropertyChangedBase
    {
        public AddGenshinDailyNoteAlarmDialogViewModel()
        {
            ConfigName = string.Empty;
            Cookies = string.Empty;
            BarkKey = string.Empty;
        }

        private string _configName;
        /// <summary>
        /// 配置名称
        /// </summary>
        public string ConfigName
        {
            get => _configName;
            set
            {
                _configName = value;
                OnPropertyChanged(nameof(ConfigName));
            }
        }

        private string _cookies;
        /// <summary>
        /// Cookies
        /// </summary>
        public string Cookies
        {
            get => _cookies;
            set
            {
                _cookies = value;
                OnPropertyChanged(nameof(Cookies));
            }
        }

        private string _barkKey;
        /// <summary>
        /// BarkKey
        /// </summary>
        public string BarkKey
        {
            get => _barkKey;
            set
            {
                _barkKey = value;
                OnPropertyChanged(nameof(BarkKey));
            }
        }

        private long? _targetId;
        /// <summary>
        /// 目标ID
        /// </summary>
        public long? TargetId
        {
            get => _targetId;
            set
            {
                _targetId = value;
                OnPropertyChanged(nameof(TargetId));
            }
        }
    }
}