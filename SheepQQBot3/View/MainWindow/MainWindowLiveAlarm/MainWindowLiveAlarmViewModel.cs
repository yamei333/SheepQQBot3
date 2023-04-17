using SheepQQBot3.Model.Config;

namespace SheepQQBot3.View;

public partial class MainWindowLiveAlarmViewModel : MainWindowViewModelBase
{
    /// <summary>
    /// 初始化
    /// </summary>
    public MainWindowLiveAlarmViewModel()
    {
    }

    private LiveAlarmConfig _selectedLiveAlarmConfig;
    /// <summary>
    /// 选中的直播提醒项
    /// </summary>

    public LiveAlarmConfig SelectedLiveAlarmConfig
    {
        get => _selectedLiveAlarmConfig;
        set
        {
            if (_selectedLiveAlarmConfig == value)
                return;

            _selectedLiveAlarmConfig = value;
            OnPropertyChanged(nameof(SelectedLiveAlarmConfig));
        }
    }

    private long? _selectedMemberId;
    /// <summary>
    /// 选中的成员ID
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