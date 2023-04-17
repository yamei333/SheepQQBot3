using System.Windows;
using SheepQQBot3.Enums;

namespace SheepQQBot3.View;

/// <summary>
/// AddGenshinDailyNoteAlarmDialog.xaml 的交互逻辑
/// </summary>
public partial class AddGenshinDailyNoteAlarmDialog
    : AddDialogWindowBase<AddGenshinDailyNoteAlarmDialogViewModel>
{
    /// <inheritdoc />
    public AddGenshinDailyNoteAlarmDialog(Window owner, object menuItem, DialogMode mode)
        : base(owner, menuItem, mode)
        => InitializeComponent();

    /// <inheritdoc />
    public AddGenshinDailyNoteAlarmDialog(
        Window owner, object menuItem, DialogMode mode,
        string configName, string cookies, string barkKey, long targetId)
        : this(owner, menuItem, mode)
    {
        Vm.ConfigName = configName;
        Vm.Cookies = cookies;
        Vm.BarkKey = barkKey;
        Vm.TargetId = targetId;
    }

    /// <inheritdoc />
    protected override void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Mode != DialogMode.Edit)
            return;

        if (Mode == DialogMode.Add)
            TxtConfigName.SelectAll();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}