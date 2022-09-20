using System.ComponentModel;
using SheepQQBot3.Model.Config;

namespace SheepQQBot3.View
{
    public partial class MainWindowRepeaterKillerViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        private static MainWindowViewModel _mainVm => PublicVar.Vm;

        private RepeaterKillerConfig _repeaterKillerConfig;

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
        public MainWindowRepeaterKillerViewModel()
        {
        }

        /// <summary>
        /// 复读机杀手配置
        /// </summary>
        public RepeaterKillerConfig RepeaterKillerConfig
        {
            get => _repeaterKillerConfig;
            set
            {
                if (_repeaterKillerConfig == value)
                    return;

                _repeaterKillerConfig = value;
                OnPropertyChanged(nameof(RepeaterKillerConfig));
            }
        }
    }
}