using SheepQQBot3.Model.Config;

namespace SheepQQBot3.View;

public partial class MainWindowBlackListViewModel : MainWindowViewModelBase
{
    /// <summary>
    /// 初始化
    /// </summary>
    public MainWindowBlackListViewModel()
    {
    }

    /// <summary>
    /// 选中的成员ID
    /// </summary>
    public BlackListUserConfig SelectedBlackListUserConfig
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged(nameof(SelectedBlackListUserConfig));
        }
    }
}