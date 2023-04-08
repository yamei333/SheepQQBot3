using System.Windows;
using System.Windows.Controls;
using SheepQQBot3.Model;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    /// <summary>
    /// MainWindowFunctionTest.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindowFunctionTest : UserControl
    {
        public MainWindowFunctionTest()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 测试发送群消息
        /// </summary>
        private void TestSendGroupMessage(object sender, RoutedEventArgs e)
        {
            if (long.TryParse(TxtTestSendGroupMessage_GroupId.Text, out var groupId))
                Vm.CqApi.SendGroupMessage(groupId, TxtTestSendGroupMessage_Content.Text, Vm.SetConfigs);
        }

        private void TestSendPrivateMessage(object sender, RoutedEventArgs e)
        {
            if (long.TryParse(TxtTestSendPrivateMessage_TargetId.Text, out var targetId))
                Vm.CqApi.SendPrivateMessage(targetId, TxtTestSendGroupMessage_Content.Text);
        }

        private void TestSendJsonMessage(object sender, RoutedEventArgs e)
        {
            //Vm.CqApi.SendGroupForwardMessage(15873217, new GroupForwardMessage[]
            //{
            //    new ("SDPM", 173629299, "我太弱了"),
            //    new ("SDPM", 173629299, "我没有任何牌面!"),
            //});

            //if (long.TryParse(TxtTestSendJsonMessage_GroupId.Text, out var groupId))
            //    Vm.CqApi.SendGroupMessage(groupId, TxtTestSendJsonMessage_Content.Text, Vm.SetConfigs);
        }
    }
}