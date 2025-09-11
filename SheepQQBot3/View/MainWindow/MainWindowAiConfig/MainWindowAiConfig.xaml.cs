using SheepQQBot3.Extensions;
using System.Windows;

namespace SheepQQBot3.View;

/// <summary>
/// MainWindowAiConfig.xaml 的交互逻辑
/// </summary>
public partial class MainWindowAIConfig
{
    private static MainWindowAIConfigModel _vm => PublicVar.Vm.MainWindowAIConfigModel;

    public MainWindowAIConfig()
    {
        InitializeComponent();
    }

    private void MainWindowAIConfig_OnLoaded(object sender, RoutedEventArgs e)
    {
        //if (TabAiSystem.Items.Count <= 0)
        //{
        //    PublicVar.AICharacter.SystemInstruction.ForEach(each =>
        //    {
        //        var tab = new TabItem
        //        {
        //            Header = each.Key,
        //        };
        //        TabAiSystem.Items.Add(tab);
        //    });
        //}
        DataContext = PublicVar.Vm.MainWindowAIConfigModel;
        _vm.OnPropertyChanged(nameof(_vm.SelectedSetConfig));
        ListAiSystem.SelectedIndex = 0;
    }

    private void BtnChangeAiConfigSystem_OnClick(object sender, RoutedEventArgs e)
    {
        TxtAiConfigSystem.IsReadOnly = !TxtAiConfigSystem.IsReadOnly;
        if (TxtAiConfigSystem.IsReadOnly)
        {
            BtnChangeAiConfigSystem.Content = "编辑内容";
            ConfigExtensions.SaveAICharacter();
        }
        else
        {
            BtnChangeAiConfigSystem.Content = "保存修改";
        }
    }
}