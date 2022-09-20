using System.Windows;
using System.Windows.Input;
using SheepQQBot3.Enums;

namespace SheepQQBot3.View
{
    /// <summary>
    /// AddAlarmAideTextDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddAlarmAideTextDialog
        : AddDialogWindowBase<AddAlarmAideTextDialogViewModel>
    {
        public string AlarmText { get; set; }

        public AddAlarmAideTextDialog(Window owner, object menuItem, DialogMode mode)
            : base(owner, menuItem, mode)
            => InitializeComponent();

        /// <inheritdoc />
        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Mode != DialogMode.Edit)
                return;
            Vm.AlarmContent = AlarmText;
            TxtContent.SelectAll();
            //Dispatcher.BeginInvoke(DispatcherPriority.Input,
            //    new Action(() =>
            //    {
            //        TxtContent.Focus(); // Set Logical Focus
            //        Keyboard.Focus(TxtContent); // Set Keyboard Focus
            //    }));
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            AlarmText = Vm.AlarmContent;
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
    }
}