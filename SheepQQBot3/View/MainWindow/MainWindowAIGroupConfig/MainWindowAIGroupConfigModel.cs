namespace SheepQQBot3.View;

public partial class MainWindowAIGroupConfigModel : MainWindowViewModelBase
{
    /// <summary>
    /// 初始化
    /// </summary>
    public MainWindowAIGroupConfigModel()
    {
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