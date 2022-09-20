using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SheepQQBot3.View
{
    /// <summary>
    /// MainWindowRunlog.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindowRunlog : UserControl
    {
        private static MainWindowRunlogViewModel _vm => PublicVar.Vm.MainWindowRunlogViewModel;

        public MainWindowRunlog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 日志右键菜单
        /// </summary>
        private void RunLog_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var selectedRunLog = _vm.SelectedRunLog;
            if (selectedRunLog == null || !selectedRunLog.IsGroupMessage)
                e.Handled = true;
        }

        /// <summary>
        /// 复读这条消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunLog_OnRepeat(object sender, RoutedEventArgs e)
        {
            var selectedRunLog = _vm.SelectedRunLog;
            _vm.Api.SendGroupMessage(long.Parse(selectedRunLog.GroupId), selectedRunLog.Content, _vm.SetConfigs);
        }

        private void RunLogMessageText_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            textBox.SelectAll();
        }

        private void RunLogMessageText_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var textBox = (TextBox)sender;
            textBox.Focus();
            e.Handled = true;
        }
    }
}