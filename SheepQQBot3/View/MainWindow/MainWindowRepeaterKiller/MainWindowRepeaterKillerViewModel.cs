using SheepQQBot3.Model.Config;

namespace SheepQQBot3.View
{
    public partial class MainWindowRepeaterKillerViewModel : MainWindowViewModelBase
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public MainWindowRepeaterKillerViewModel()
        {
        }

        private RepeaterKillerConfig _repeaterKillerConfig;
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