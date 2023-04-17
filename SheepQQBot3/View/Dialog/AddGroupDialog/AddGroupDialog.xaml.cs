using System.Linq;
using System.Windows;
using SheepQQBot3.Enums;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.View;

/// <summary>
/// AddGroupDialog.xaml 的交互逻辑
/// </summary>
public partial class AddGroupDialog
    : AddDialogWindowBase<AddGroupDialogViewModel>
{
    public long TargetId;
    public BotConfigTargetType TargetType;
    public string TargetName;

    public AddGroupDialog(Window owner, object menuItem, DialogMode mode)
        : base(owner, menuItem, mode)
        => InitializeComponent();

    /// <inheritdoc />
    protected override void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Mode != DialogMode.Edit)
            return;

        Vm.TargetId = TargetId;
        Vm.TargetName = TargetName;
        Vm.SelectedGroupConfig = Vm.GroupConfigs.First(each => each.TargetType == TargetType);

        TxtGroupId.SelectAll();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        TargetId = Vm.TargetId;
        TargetType = Vm.SelectedGroupConfig.TargetType;
        var targetName = Vm.TargetName;
        TargetName = string.IsNullOrEmpty(targetName) ? "未设定" : targetName;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}