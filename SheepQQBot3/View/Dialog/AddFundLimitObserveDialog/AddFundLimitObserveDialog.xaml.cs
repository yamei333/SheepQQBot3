using System.Linq;
using System.Windows;
using SheepQQBot3.Enums;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.View
{
    /// <summary>
    /// AddFundLimitObserveDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddFundLimitObserveDialog
        : AddDialogWindowBase<AddFundLimitObserveDialogViewModel>
    {
        public string FundId;
        public FundObserveType FundObserveType;
        public float AlertLimit;

        public AddFundLimitObserveDialog(Window owner, object menuItem, DialogMode mode)
            : base(owner, menuItem, mode)
            => InitializeComponent();

        /// <inheritdoc />
        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Mode != DialogMode.Edit)
                return;

            Vm.FundId = FundId;
            Vm.AlertLimit = AlertLimit;
            Vm.SelectedAddFundLimitObserveDialogConfig = Vm.AddFundLimitDialogConfigs
                .First(each => each.FundObserveType == FundObserveType);

            if (Mode == DialogMode.Add)
                TxtFundId.SelectAll();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            var alertLimit = Vm.AlertLimit;
            if (!alertLimit.HasValue)
            {
                MessageBox.Show("必须输入阈值!");
                return;
            }

            FundId = Vm.FundId;
            FundObserveType = Vm.SelectedAddFundLimitObserveDialogConfig.FundObserveType;
            AlertLimit = alertLimit.Value;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}