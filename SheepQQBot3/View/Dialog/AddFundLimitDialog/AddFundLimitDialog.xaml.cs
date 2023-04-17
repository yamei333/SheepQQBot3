using System.Linq;
using System.Windows;
using SheepQQBot3.Enums;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.View;

/// <summary>
/// AddFundLimitDialog.xaml 的交互逻辑
/// </summary>
public partial class AddFundLimitDialog
    : AddDialogWindowBase<AddFundLimitDialogViewModel>
{
    public string FundId;
    public FundObserveType FundObserveType;
    public float AlertLimit;

    public AddFundLimitDialog(Window owner, object menuItem, DialogMode mode)
        : base(owner, menuItem, mode)
        => InitializeComponent();

    /// <inheritdoc />
    protected override void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Mode != DialogMode.Edit)
            return;

        Vm.FundId = FundId;
        Vm.AlertLimit = AlertLimit;
        Vm.SelectedAddFundLimitDialogConfig = Vm.AddFundLimitDialogConfigs
            .First(each => each.FundObserveType == FundObserveType);

        if (Mode == DialogMode.Add)
            TxtFundId.SelectAll();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        FundId = Vm.FundId;
        FundObserveType = Vm.SelectedAddFundLimitDialogConfig.FundObserveType;
        AlertLimit = Vm.AlertLimit;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}