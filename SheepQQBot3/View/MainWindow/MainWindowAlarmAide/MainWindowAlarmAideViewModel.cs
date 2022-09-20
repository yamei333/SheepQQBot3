using System;
using System.Collections.Generic;
using System.ComponentModel;
using SheepQQBot3.Model.Config;
using SheepQQBot3.SDK.Client;

namespace SheepQQBot3.View
{
    public partial class MainWindowAlarmAideViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        private AlarmAideConfig _selectedAlarmAideConfig;

        private static MainWindowViewModel _mainVm => PublicVar.Vm;

        /// <summary>
        /// 值变化时调用, 用于通知界面
        /// </summary>
        /// <param name="propertyName">属性名</param>
        public void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Dictionary<Guid, SetConfig> SetConfigs => _mainVm.SetConfigs;
        public SetConfig SelectedSetConfig => _mainVm.SelectedSetConfig;
        public CQAPI Api => _mainVm.CqApi;

        /// <summary>
        /// 初始化
        /// </summary>
        public MainWindowAlarmAideViewModel()
        {
        }

        public AlarmAideConfig SelectedAlarmAideConfig
        {
            get => _selectedAlarmAideConfig;
            set
            {
                if (_selectedAlarmAideConfig == value)
                    return;

                _selectedAlarmAideConfig = value;
                SelectedAlarmText = default;
                OnPropertyChanged(nameof(SelectedAlarmAideConfig));
            }
        }

        private KeyValuePair<int, string> _selectedAlarmText;
        /// <summary>
        /// 选中的提醒列表内容
        /// </summary>
        public KeyValuePair<int, string> SelectedAlarmText
        {
            get => _selectedAlarmText;
            set
            {
                if (_selectedAlarmText.Key == value.Key && _selectedAlarmText.Value == value.Value)
                    return;

                _selectedAlarmText = value;
                OnPropertyChanged(nameof(SelectedAlarmText));
            }
        }

        private int _selectedMemberId;
        /// <summary>
        /// 选中的提醒列表内容
        /// </summary>
        public int SelectedMemberId
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

        private bool _isSingleSelectedAlarmAideText;
        /// <summary>
        /// 是否单选提醒列表内容
        /// </summary>
        public bool IsSingleSelectedAlarmAideText
        {
            get => _isSingleSelectedAlarmAideText;
            set
            {
                if (_isSingleSelectedAlarmAideText == value)
                    return;

                _isSingleSelectedAlarmAideText = value;
                OnPropertyChanged(nameof(IsSingleSelectedAlarmAideText));
            }
        }
    }
}