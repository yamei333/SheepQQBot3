using System.Windows;
using SheepQQBot3.Enums;

namespace SheepQQBot3.View
{
    /// <summary>
    /// AddDateTimeConfigDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddDateTimeConfigDialog
        : AddDialogWindowBase<AddDateTimeConfigDialogViewModel>
    {
        public string AlarmName { get; set; }
        public string Condition { get; set; }

        public AddDateTimeConfigDialog(Window owner, object menuItem, DialogMode mode)
            : base(owner, menuItem, mode)
            => InitializeComponent();

        /// <inheritdoc />
        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Mode != DialogMode.Edit)
                return;

            Vm.AlarmName = AlarmName;
            Vm.Condition = Condition;

            TxtName.SelectAll();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            AlarmName = Vm.AlarmName;
            Condition = Vm.Condition;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}