using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using SheepQQBot3.Enums;

namespace SheepQQBot3.View
{
    /// <summary>
    /// AddAlarmAideSubmitMemberDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddAlarmAideSubmitMemberDialog
        : AddDialogWindowBase<AddAlarmAideSubmitMemberDialogViewModel>
    {
        private static readonly Regex _regex = new Regex("[^0-9]+");
        private static bool IsTextAllowed(string text) => !_regex.IsMatch(text);

        public int AlarmAideMemberId { get; set; }

        public AddAlarmAideSubmitMemberDialog(Window owner, object menuItem, DialogMode mode)
            : base(owner, menuItem, mode)
            => InitializeComponent();

        /// <inheritdoc />
        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            Vm.AlarmAideMemberId = AlarmAideMemberId;
            TxtContent.SelectAll();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            AlarmAideMemberId = Vm.AlarmAideMemberId;
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
}