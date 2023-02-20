using System.ComponentModel;

namespace SheepQQBot3.View
{
    public class AddAlarmAideSubmitMemberDialogViewModel : INotifyPropertyChanged
    {
        public int _alarmAideMemberId;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public AddAlarmAideSubmitMemberDialogViewModel()
        {
            AlarmAideMemberId = 0;
        }

        /// <summary>
        /// 闹钟助手投稿成员ID
        /// </summary>
        public int AlarmAideMemberId
        {
            get => _alarmAideMemberId;
            set
            {
                if (_alarmAideMemberId == value)
                    return;

                _alarmAideMemberId = value;
                OnPropertyChanged(nameof(AlarmAideMemberId));
            }
        }
    }
}