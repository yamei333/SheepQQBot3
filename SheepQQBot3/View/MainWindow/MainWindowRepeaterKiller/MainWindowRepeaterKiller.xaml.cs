using System.Windows;
using System.Windows.Controls;
using SheepQQBot3.Extensions;

namespace SheepQQBot3.View
{
    /// <summary>
    /// MainWindowRepeaterKiller.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindowRepeaterKiller : UserControl
    {
        private static MainWindowRepeaterKillerViewModel _vm => PublicVar.Vm.MainWindowRepeaterKillerViewModel;

        public MainWindowRepeaterKiller()
        {
            InitializeComponent();
        }

        private void MainWindowRepeaterKiller_OnLoaded(object sender, RoutedEventArgs e)
        {
            DataContext = PublicVar.Vm.MainWindowRepeaterKillerViewModel;
            _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
        }

        private void OnRepeatLimitChanged(object sender, TextChangedEventArgs e)
        {
            var repeatLimit = _vm.RepeaterKillerConfig.RepeatLimit;
            ;
            ConfigExtensions.SaveConfig();
        }
    }
}