using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Extension;
using System.Windows;
using System.Windows.Controls;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.View;

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
    private async void TestSendGroupMessage(object sender, RoutedEventArgs e)
    {
        if (long.TryParse(TxtTestSendGroupMessage_GroupId.Text, out var groupId))
            await Vm.BotServer.SendGroupMessageAsync(groupId, TxtTestSendGroupMessage_Content.Text, Vm.SetConfigs).ConfigureAwait(false);
    }

    /// <summary>
    /// 测试发送群消息(风控消息)
    /// </summary>
    private async void TestSendGroupMessage_BlockMessage(object sender, RoutedEventArgs e)
    {
        var message = "[CQ:xml,data=<?xml version=\"1.0\" " +
                      "encoding=\"utf-8\"?><msg templateID=\"12345\" action=\"web\" brief=\"RPG\" " +
                      "serviceID=\"1\" Url=\"http://pcro.jp/\"><item layout=\"2\"><picture cover=\"\"/>" +
                      "<Title>ぷちっとくろにくる</Title><summary>カワイイ</summary></item><source/></msg>,resid=1]";
        if (long.TryParse(TxtTestSendGroupMessage_GroupId.Text, out var groupId))
            await Vm.BotServer.SendGroupMessageAsync(groupId, message, Vm.SetConfigs).ConfigureAwait(false);
    }

    private void TestSendPrivateMessage(object sender, RoutedEventArgs e)
    {
        if (long.TryParse(TxtTestSendPrivateMessage_TargetId.Text, out var targetId))
            Vm.BotServer.SendPrivateMessageAsync(targetId, TxtTestSendGroupMessage_Content.Text);
    }

    /// <summary>
    /// 测试发送Json消息
    /// </summary>
    private async void TestSendJsonMessage1(object sender, RoutedEventArgs e)
    {
        if (long.TryParse(TxtTestSendGroupMessage_GroupId.Text, out var groupId))
        {
            await Vm.BotServer.SendGroupMessageAsync(groupId, await CQCode.JsonCard_StructMsgAsync(
                TxtTestSendJsonMessage1_Title.Text,
                TxtTestSendJsonMessage1_Content.Text,
                TxtTestSendJsonMessage1_Url.Text,
                string.Empty).ConfigureAwait(false),
                Vm.SetConfigs).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 测试发送Bark推送
    /// </summary>
    private async void TestSendPushBarkMessage(object sender, RoutedEventArgs e)
    {
        await PushExtensions.PushBarkMessageAsync(TxtTestPushBarkMessage_Message.Text, TxtTestPushBarkMessage_Title.Text)
            .ConfigureAwait(false);
    }

    //Vm.CqApi.SendGroupForwardMessage(15873217, new GroupForwardMessage[]
    //{
    //    new ("SDPM", 173629299, "我太弱了"),
    //    new ("SDPM", 173629299, "我没有任何牌面!"),
    //});

    //if (long.TryParse(TxtTestSendJsonMessage_GroupId.Text, out var groupId))
    //    Vm.CqApi.SendGroupMessage(groupId, TxtTestSendJsonMessage_Content.Text, Vm.SetConfigs);
}