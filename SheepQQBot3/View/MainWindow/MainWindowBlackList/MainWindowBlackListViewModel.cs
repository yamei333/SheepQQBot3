using System.ComponentModel;
using SheepQQBot3.Model.Config;

namespace SheepQQBot3.View
{
    public partial class MainWindowBlackListViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        private static MainWindowViewModel _mainVm => PublicVar.Vm;

        /// <summary>
        /// 值变化时调用, 用于通知界面
        /// </summary>
        /// <param name="propertyName">属性名</param>
        public void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public SetConfig SelectedSetConfig => _mainVm.SelectedSetConfig;

        /// <summary>
        /// 初始化
        /// </summary>
        public MainWindowBlackListViewModel()
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
}