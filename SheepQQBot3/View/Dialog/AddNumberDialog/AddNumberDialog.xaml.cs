using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using SheepQQBot3.Enums;
using SheepQQBot3.Model.Extension;

namespace SheepQQBot3.View;

/// <summary>
/// AddNumberDialog.xaml 的交互逻辑
/// </summary>
public partial class AddNumberDialog
    : AddDialogWindowBase<AddNumberDialogViewModel>
{
    private static readonly Regex _regex = RegexGenerator.Number();

    private static bool IsTextAllowed(string text) => !_regex.IsMatch(text);

    /// <summary>
    /// 增加的数字内容
    /// </summary>
    public int? AddNumber { get; set; }

    public AddNumberDialog(Window owner, object menuItem, DialogMode mode, string title)
        : base(owner, menuItem, mode, title)
        => InitializeComponent();

    /// <inheritdoc />
    protected override void OnLoaded(object sender, RoutedEventArgs e)
    {
        Vm.AddNumber = AddNumber;
        TxtContent.SelectAll();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        AddNumber = Vm.AddNumber;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnCtrlEnter(object sender, KeyEventArgs e)
    {
        KeyEventHelper.OnKeyDown(e, ModifierKeys.Control, Key.Enter, () =>
        {
            OK_Click(sender, null);
        });
    }

    private void TxtContent_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !IsTextAllowed(e.Text);
    }
}