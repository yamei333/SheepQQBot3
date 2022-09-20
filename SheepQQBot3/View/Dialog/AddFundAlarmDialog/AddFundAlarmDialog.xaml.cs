using System.Windows;
using SheepQQBot3.Enums;

namespace SheepQQBot3.View
{
    /// <summary>
    /// AddFundAlarmDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddFundAlarmDialog
        : AddDialogWindowBase<AddFundAlarmDialogViewModel>
    {
        public string FundId;
        public string FundRemark;

        public AddFundAlarmDialog(Window owner, object menuItem, DialogMode mode)
            : base(owner, menuItem, mode)
            => InitializeComponent();

        /// <inheritdoc />
        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Mode != DialogMode.Edit)
                return;

            Vm.FundId = FundId;
            Vm.FundRemark = FundRemark;

            TxtFundId.SelectAll();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            FundId = Vm.FundId;
            FundRemark = Vm.FundRemark;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}