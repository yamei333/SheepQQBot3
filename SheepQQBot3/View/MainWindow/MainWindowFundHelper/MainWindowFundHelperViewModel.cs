using System.Collections.Generic;
using System.ComponentModel;
using SheepQQBot3.Model.Config;

namespace SheepQQBot3.View
{
    public partial class MainWindowFundHelperViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        private static MainWindowViewModel _mainVm => PublicVar.Vm;

        private FundAlarmConfig _selectedFundAlarmConfig;
        private FundLimitObserveConfig _selectedFundLimitObserveConfig;

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
        public MainWindowFundHelperViewModel()
        {
        }

        /// <summary>
        /// 选中的基金播报项
        /// </summary>
        public FundAlarmConfig SelectedFundAlarmConfig
        {
            get => _selectedFundAlarmConfig;
            set
            {
                if (_selectedFundAlarmConfig == value)
                    return;

                _selectedFundAlarmConfig = value;
                OnPropertyChanged(nameof(SelectedFundAlarmConfig));
            }
        }

        /// <summary>
        /// 选中的基金阈值观测项
        /// </summary>
        public FundLimitObserveConfig SelectedFundLimitObserveConfig
        {
            get => _selectedFundLimitObserveConfig;
            set
            {
                if (_selectedFundLimitObserveConfig == value)
                    return;

                _selectedFundLimitObserveConfig = value;
                OnPropertyChanged(nameof(SelectedFundLimitObserveConfig));
            }
        }

        private KeyValuePair<int, AlarmFundConfig> _selectedAlarmFundConfig;
        /// <summary>
        /// 选中的播报基金项
        /// </summary>
        public KeyValuePair<int, AlarmFundConfig> SelectedAlarmFundConfig
        {
            get => _selectedAlarmFundConfig;
            set
            {
                if (_selectedAlarmFundConfig.Key == value.Key && _selectedAlarmFundConfig.Value == value.Value)
                    return;

                _selectedAlarmFundConfig = value;
                OnPropertyChanged(nameof(SelectedAlarmFundConfig));
            }
        }

        private KeyValuePair<int, LimitObserveFundConfig> _selectedLimitObserveFundConfig;
        /// <summary>
        /// 选中的阈值观测基金项
        /// </summary>
        public KeyValuePair<int, LimitObserveFundConfig> SelectedLimitObserveFundConfig
        {
            get => _selectedLimitObserveFundConfig;
            set
            {
                if (_selectedLimitObserveFundConfig.Key == value.Key && _selectedLimitObserveFundConfig.Value == value.Value)
                    return;

                _selectedLimitObserveFundConfig = value;
                OnPropertyChanged(nameof(SelectedLimitObserveFundConfig));
            }
        }
    }
}