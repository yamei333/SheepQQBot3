namespace SheepQQBot3.View;

public partial class MainWindowAlarmAideSubmitViewModel : MainWindowViewModelBase
{
    /// <summary>
    /// 初始化
    /// </summary>
    public MainWindowAlarmAideSubmitViewModel()
    {
    }

    private long? _selectedMemberId;
    /// <summary>
    /// 选中的投稿成员ID
    /// </summary>
    public long? SelectedMemberId
    {
        get => _selectedMemberId;
        set
        {
            if (_selectedMemberId == value)
                return;

            _selectedMemberId = value;
            OnPropertyChanged(nameof(SelectedMemberId));
        }
    }
}