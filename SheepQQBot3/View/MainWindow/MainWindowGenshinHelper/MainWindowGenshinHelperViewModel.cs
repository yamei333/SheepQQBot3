using SheepQQBot3.Model.Config;

namespace SheepQQBot3.View
{
    public partial class MainWindowGenshinHelperViewModel : MainWindowViewModelBase
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public MainWindowGenshinHelperViewModel()
        {
        }

        private GenshinResinAlarm _selectedGenshinResinAlarm;
        /// <summary>
        /// 选中的原神树脂提醒项
        /// </summary>

        public GenshinResinAlarm SelectedGenshinResinAlarm
        {
            get => _selectedGenshinResinAlarm;
            set
            {
                if (_selectedGenshinResinAlarm == value)
                    return;

                _selectedGenshinResinAlarm = value;
                OnPropertyChanged(nameof(SelectedGenshinResinAlarm));
            }
        }
    }
}