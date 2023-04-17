using System.ComponentModel;

namespace SheepQQBot3.View;

public class AddAlarmAideTextDialogViewModel : INotifyPropertyChanged
{
    public string _alarmContent;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public AddAlarmAideTextDialogViewModel()
    {
        AlarmContent = string.Empty;
    }

    /// <summary>
    /// 闹钟提醒内容
    /// </summary>
    public string AlarmContent
    {
        get => _alarmContent;
        set
        {
            if (_alarmContent == value)
                return;

            _alarmContent = value;
            OnPropertyChanged(nameof(AlarmContent));
        }
    }
}