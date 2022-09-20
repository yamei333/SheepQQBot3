using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.View
{
    public class AddFundLimitObserveDialogViewModel : INotifyPropertyChanged
    {
        private string _fundId;
        private float? _alertLimit;
        private AddFundLimitObserveDialogConfig _selectedAddFundLimitObserveDialogConfig;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public AddFundLimitObserveDialogViewModel()
        {
            FundId = string.Empty;
            AlertLimit = null;
            var addFundLimitDialogConfigs = new List<AddFundLimitObserveDialogConfig>
            {
                new(FundObserveType.Week, "周"),
                new(FundObserveType.Month, "月"),
                new(FundObserveType.ThreeMonths, "季度"),
                new(FundObserveType.SixMonths, "半年"),
                new(FundObserveType.Week, "年"),
            };
            AddFundLimitDialogConfigs = addFundLimitDialogConfigs;
            SelectedAddFundLimitObserveDialogConfig = addFundLimitDialogConfigs.First();
        }

        private List<AddFundLimitObserveDialogConfig> _addFundLimitObserveDialogConfigs;
        /// <summary>
        /// 类型
        /// </summary>
        public List<AddFundLimitObserveDialogConfig> AddFundLimitDialogConfigs
        {
            get => _addFundLimitObserveDialogConfigs;
            set
            {
                if (_addFundLimitObserveDialogConfigs == value)
                    return;

                _addFundLimitObserveDialogConfigs = value;
                OnPropertyChanged(nameof(AddFundLimitDialogConfigs));
            }
        }

        /// <summary>
        /// 当前选中类型
        /// </summary>
        public AddFundLimitObserveDialogConfig SelectedAddFundLimitObserveDialogConfig
        {
            get => _selectedAddFundLimitObserveDialogConfig;
            set
            {
                _selectedAddFundLimitObserveDialogConfig = value;
                OnPropertyChanged(nameof(SelectedAddFundLimitObserveDialogConfig));
            }
        }

        /// <summary>
        /// 基金编号
        /// </summary>
        public string FundId
        {
            get => _fundId;
            set
            {
                _fundId = value;
                OnPropertyChanged(nameof(FundId));
            }
        }

        /// <summary>
        /// 阈值
        /// </summary>
        public float? AlertLimit
        {
            get => _alertLimit;
            set
            {
                _alertLimit = value;
                OnPropertyChanged(nameof(AlertLimit));
            }
        }
    }
}