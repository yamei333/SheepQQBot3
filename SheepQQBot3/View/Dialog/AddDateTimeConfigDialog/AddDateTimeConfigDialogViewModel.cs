using System.ComponentModel;

namespace SheepQQBot3.View;

public class AddDateTimeConfigDialogViewModel : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public AddDateTimeConfigDialogViewModel()
    {
        AlarmName = string.Empty;
        Condition = @"\d{4}-\d{2}-\d{2}-\d{1}-\d{1} (01|09|10|11|12|13|14|15|16|17|18|19|20|21|22|23|00):00:\d{2}";
    }

    private string _alarmName;
    public string AlarmName
    {
        get => _alarmName;
        set
        {
            if (_alarmName == value)
                return;

            _alarmName = value;
            OnPropertyChanged(nameof(AlarmName));
        }
    }

    private string _condition;
    public string Condition
    {
        get => _condition;
        set
        {
            if (_condition == value)
                return;

            _condition = value;
            OnPropertyChanged(nameof(Condition));
        }
    }
}