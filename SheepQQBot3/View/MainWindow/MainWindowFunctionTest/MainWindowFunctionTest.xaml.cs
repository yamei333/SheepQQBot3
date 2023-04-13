using System.Windows;
using System.Windows.Controls;
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
                Vm.CqApi.SendGroupMessageAsync(groupId, TxtTestSendGroupMessage_Content.Text, Vm.SetConfigs);
        }

        /// <summary>
        /// 测试发送群消息(风控消息)
        /// </summary>
        private void TestSendGroupMessage_BlockMessage(object sender, RoutedEventArgs e)
        {
            var message = "[CQ:xml,data=<?xml version=\"1.0\" " +
                          "encoding=\"utf-8\"?><msg templateID=\"12345\" action=\"web\" brief=\"RPG\" " +
                          "serviceID=\"1\" Url=\"http://pcro.jp/\"><item layout=\"2\"><picture cover=\"\"/>" +
                          "<Title>ぷちっとくろにくる</Title><summary>カワイイ</summary></item><source/></msg>,resid=1]";
            if (long.TryParse(TxtTestSendGroupMessage_GroupId.Text, out var groupId))
                Vm.CqApi.SendGroupMessageAsync(groupId, message, Vm.SetConfigs);
        }

        private void TestSendPrivateMessage(object sender, RoutedEventArgs e)
        {
            if (long.TryParse(TxtTestSendPrivateMessage_TargetId.Text, out var targetId))
                Vm.CqApi.SendPrivateMessageAsync(targetId, TxtTestSendGroupMessage_Content.Text);
        }

        //Vm.CqApi.SendGroupForwardMessage(15873217, new GroupForwardMessage[]
        //{
        //    new ("SDPM", 173629299, "我太弱了"),
        //    new ("SDPM", 173629299, "我没有任何牌面!"),
        //});

        //if (long.TryParse(TxtTestSendJsonMessage_GroupId.Text, out var groupId))
        //    Vm.CqApi.SendGroupMessage(groupId, TxtTestSendJsonMessage_Content.Text, Vm.SetConfigs);
    }
}