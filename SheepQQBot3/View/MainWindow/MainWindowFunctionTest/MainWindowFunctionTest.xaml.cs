using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.JsonCard;
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
            await BotClient.SendGroupMessageAsync(groupId, TxtTestSendGroupMessage_Content.Text, Vm.SetConfigs).ConfigureAwait(false);
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
        var signedJsonText = "{\"app\":\"com.tencent.tianxuan.share\",\"config\":{\"ctime\":1721896417,\"token\":\"e1e951801b4dabc3c419e628e6bc223f\"},\"meta\":{\"news\":{\"desc\":\"徐州鸡3\",\"jumpUrl\":\"https://www.baidu.com/\",\"preview\":\"https://tianxuan.gtimg.cn/45176_272fa035/assets/200x200.jpg\",\"tag\":\"徐州鸡4\",\"tagIcon\":\"https://tianxuan.gtimg.cn/42065_b555f19c/assets/qq.png\",\"title\":\"徐州鸡2\"}},\"prompt\":\"徐州鸡1\",\"ver\":\"0.0.0.1\",\"view\":\"news\"}\n";
        var message = $"[CQ:json,data={signedJsonText}]";
        if (long.TryParse(TxtTestSendGroupMessage_GroupId.Text, out var groupId))
            await BotClient.SendGroupMessageAsync(groupId, message, Vm.SetConfigs).ConfigureAwait(false);
    }

    private void TestSendPrivateMessage(object sender, RoutedEventArgs e)
    {
        if (long.TryParse(TxtTestSendPrivateMessage_TargetId.Text, out var targetId))
            BotClient.SendPrivateMessageAsync(targetId, TxtTestSendGroupMessage_Content.Text);
    }

    /// <summary>
    /// 测试发送Json消息
    /// </summary>
    private async void TestSendJsonMessage1(object sender, RoutedEventArgs e)
    {
        //var miniAppJson = await Vm.BotServer.GetMiniAppJsonAsync(MiniAppType.WeiBo,
        //    "测试标题", "测试内容", "https://thirdqq.qlogo.cn/g?b=oidb&k=09ElpZZZUTHFhoIlvs0lFg&kti=ZyBvjxHhVOI&s=640",
        //    "https://www.bilibili.com/video/BV1GJ411x7h7/?share_source=copy_web&vd_source=f952e0bb6dedea89c4fea625fbb2aab1")
        //    .ConfigureAwait(false);

        if (long.TryParse(TxtTestSendJsonMessage1_GroupId.Text, out var groupId))
        {
            //var sendMessages = new List<GroupForwardMessage>
            //{
            //    new("测试1", 10000, "test1"),
            //    new("测试2", 10000, "test2"),
            //};
            //await Vm.BotServer.SendGroupForwardMessageAsync(groupId, sendMessages).ConfigureAwait(false);

            //// MEMO : markdown发送测试
            //var sendMessages = new List<GroupForwardMessage>
            //{
            //    new("测试2", 252961222, CQCode.MarkDown("### 测试徐州鸡\\n- 鸡群1\\n- 鸡群2\\n- 鸡群3")),
            //};

            //await Vm.BotServer.SendGroupForwardMessageAsync(groupId, sendMessages,
            //    "title", ["content"], "summary", "prompt", 15, data =>
            //{
            //    if (data is { IsSuccessed: false, RetCode: 1200 })
            //    {
            //        var reg = new Regex("(?<=发送转发消息（res_id：).+(?= 失败)", RegexOptions.Multiline);
            //        var match = reg.Match(data.Wording);
            //        if (match.Success)
            //        {
            //            var sendMessages2 = new List<GroupForwardMessage>
            //            {
            //                new("测试1", 252961222, "测试md"),
            //                new("测试2", 252961222, CQCode.Forward(match.Value)),
            //            };
            //            Vm.BotServer.SendGroupForwardMessageAsync(groupId, sendMessages2).ConfigureAwait(false);
            //        }
            //    }
            //}).ConfigureAwait(false);

            var miniAppJson = await BotClient.GetMiniAppJsonAsync(MiniAppType.Bilibili, "title", "content",
                "https://ragnarokonline.gungho.jp/gameguide/system/expand-item/images/glacier/map01_armor.png",
                "https://www.bilibili.com/video/BV1GJ411x7h7/").ConfigureAwait(false);
            await BotClient.SendGroupMessageAsync(groupId, CQCode.Json(miniAppJson))
                .ConfigureAwait(false);
            //await Vm.BotServer.SendGroupMessageAsync(groupId, await CQExtensions.JsonCard_TianxuanShareAsync(
            //    TxtTestSendJsonMessage1_Title.Text,
            //    TxtTestSendJsonMessage1_Content.Text,
            //    TxtTestSendJsonMessage1_Url.Text,
            //    string.Empty).ConfigureAwait(false),
            //    Vm.SetConfigs).ConfigureAwait(false);
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