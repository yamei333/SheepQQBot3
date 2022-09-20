using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.View
{
    public class AddFundLimitDialogViewModel : INotifyPropertyChanged
    {
        private string _fundId;
        private float _alertLimit;
        private AddFundLimitDialogConfig _selectedAddFundLimitDialogConfig;
        private List<AddFundLimitDialogConfig> _addFundLimitDialogConfigs;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public AddFundLimitDialogViewModel()
        {
            FundId = string.Empty;
            var addFundLimitDialogConfigs = new List<AddFundLimitDialogConfig>
            {
                new AddFundLimitDialogConfig(FundObserveType.Week, "周"),
                new AddFundLimitDialogConfig(FundObserveType.Month, "月"),
                new AddFundLimitDialogConfig(FundObserveType.ThreeMonths, "季度"),
                new AddFundLimitDialogConfig(FundObserveType.SixMonths, "半年"),
                new AddFundLimitDialogConfig(FundObserveType.Week, "年"),
            };
            AddFundLimitDialogConfigs = addFundLimitDialogConfigs;
            SelectedAddFundLimitDialogConfig = addFundLimitDialogConfigs.First();
        }

        /// <summary>
        /// 类型
        /// </summary>
        public List<AddFundLimitDialogConfig> AddFundLimitDialogConfigs
        {
            get => _addFundLimitDialogConfigs;
            set
            {
                if (_addFundLimitDialogConfigs == value)
                    return;

                _addFundLimitDialogConfigs = value;
                OnPropertyChanged(nameof(AddFundLimitDialogConfigs));
            }
        }

        /// <summary>
        /// 当前选中类型
        /// </summary>
        public AddFundLimitDialogConfig SelectedAddFundLimitDialogConfig
        {
            get => _selectedAddFundLimitDialogConfig;
            set
            {
                _selectedAddFundLimitDialogConfig = value;
                OnPropertyChanged(nameof(SelectedAddFundLimitDialogConfig));
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
        public float AlertLimit
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