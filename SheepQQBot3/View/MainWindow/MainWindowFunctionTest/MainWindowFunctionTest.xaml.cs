using System.Windows;
using System.Windows.Controls;
using SheepQQBot3.Model.Extension;
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
            await GlobalBotClient.SendGroupMessageAsync(groupId.ToString(), TxtTestSendGroupMessage_Content.Text, Vm.SetConfigs).ConfigureAwait(false);
    }

    /// <summary>
    /// 测试发送群消息(风控消息)
    /// </summary>
    private async void TestSendGroupMessage_BlockMessage(object sender, RoutedEventArgs e)
    {
        //var message = "[CQ:xml,data=<?xml version=\"1.0\" " +
        //              "encoding=\"utf-8\"?><msg templateID=\"12345\" action=\"web\" brief=\"RPG\" " +
        //              "serviceID=\"1\" Url=\"http://pcro.jp/\"><item layout=\"2\"><picture cover=\"\"/>" +
        //              "<Title>ぷちっとくろにくる</Title><summary>カワイイ</summary></item><source/></msg>,resid=1]";
        var signedJsonText
            = "{\"app\":\"com.tencent.tianxuan.share\",\"config\":{\"ctime\":1721896417,\"token\":\"e1e951801b4dabc3c419e628e6bc223f\"},\"meta\":{\"news\":{\"desc\":\"徐州鸡3\",\"jumpUrl\":\"https://www.baidu.com/\",\"preview\":\"https://tianxuan.gtimg.cn/45176_272fa035/assets/200x200.jpg\",\"tag\":\"徐州鸡4\",\"tagIcon\":\"https://tianxuan.gtimg.cn/42065_b555f19c/assets/qq.png\",\"title\":\"徐州鸡2\"}},\"prompt\":\"徐州鸡1\",\"ver\":\"0.0.0.1\",\"view\":\"news\"}\n";
        var message = $"[CQ:json,data={signedJsonText}]";
        if (long.TryParse(TxtTestSendGroupMessage_GroupId.Text, out var groupId))
        {
            // await GlobalBotClient.SendGroupMessageAsync(groupId.ToString(), message, Vm.SetConfigs).ConfigureAwait(false);
            await GlobalBotClient.SendGroupTuwenAsync(groupId.ToString(),
                "测试标题", "测试内容", "[测试]",
                "https://tianxuan.gtimg.cn/45176_272fa035/assets/200x200.jpg",
                "https://live.bilibili.com/21678788").ConfigureAwait(false);
        }
    }

    private void TestSendPrivateMessage(object sender, RoutedEventArgs e)
    {
        if (long.TryParse(TxtTestSendPrivateMessage_TargetId.Text, out var targetId))
            GlobalBotClient.SendPrivateMessageAsync(targetId.ToString(), TxtTestSendGroupMessage_Content.Text);
    }

    /// <summary>
    /// 测试发送卡片消息
    /// </summary>
    private async void TestSendTuwen(object sender, RoutedEventArgs e)
    {
        if (long.TryParse(TxtTestSendTuwen_GroupId.Text, out var groupId))
        {
            await GlobalBotClient.SendGroupTuwenAsync(groupId.ToString(),
                    TxtTestSendTuwen_Title.Text, TxtTestSendTuwen_Content.Text, TxtTestSendTuwen_Summary.Text,
                    TxtTestSendTuwen_Preview.Text, TxtTestSendTuwen_Url.Text)
                .ConfigureAwait(false);
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