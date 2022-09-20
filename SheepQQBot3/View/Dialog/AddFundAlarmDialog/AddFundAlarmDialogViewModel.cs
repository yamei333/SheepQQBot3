using System.ComponentModel;

namespace SheepQQBot3.View
{
    public class AddFundAlarmDialogViewModel : INotifyPropertyChanged
    {
        private string _fundId;
        private string _fundRemark;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public AddFundAlarmDialogViewModel()
        {
            FundId = string.Empty;
            FundRemark = string.Empty;
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
        /// 备注
        /// </summary>
        public string FundRemark
        {
            get => _fundRemark;
            set
            {
                _fundRemark = value;
                OnPropertyChanged(nameof(FundRemark));
            }
        }
    }
}