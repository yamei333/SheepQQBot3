using JiebaNet.Analyser;
using JiebaNet.Segmenter;
using SheepQQBot3.Model;
using static SheepQQBot3.Model.MessageUtil;

namespace SheepQQBot3.UnitTest.Utils;

public class TestClass
{
    public string Text1 { get; set; }

    public string Text2 { get; set; }
}

[TestClass]
public class MessageUtilTest
{
    [TestMethod]
    public void NormalTest()
    {
        //var jsonInfo = "{\"self_id\":205552607,\"user_id\":381559116,\"time\":1709883310,\"message_id\":-2147483512,\"real_id\":\"7343892892256555542\",\"message_type\":\"group\",\"sender\":{\"user_id\":381559116,\"nickname\":\"凡\",\"card\":\"ruojiji2\",\"role\":\"owner\"},\"raw_message\":\"女的走了你工作量翻倍吗\",\"font\":14,\"sub_type\":\"normal\",\"message\":[{\"data\":{\"text\":\"女的走了你工作量翻倍吗\"},\"type\":\"text\"}],\"message_format\":\"array\",\"post_type\":\"message\",\"group_id\":414774779}";
        //var zap = JsonSerializer.Deserialize<ClientData>(jsonInfo);

        #region Test

        //var jsonData = JsonSerializer.Serialize(new SendData("send_group_msg", new ParamData
        //{
        //    GroupId = "11111",
        //    Messages = new List<Element>
        //    {
        //        new Element("text", new ElementBaseData("测试消息"))
        //    }
        //}), new JsonSerializerOptions
        //{
        //    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        //});

        //var zap =
        //    "{\"post_type\":\"notice\",\"notice_type\":\"notify\",\"time\":1676531224,\"self_id\":1366869256,\"sub_type\":\"honor\",\"group_id\":122187517,\"user_id\":252961222,\"honor_type\":\"talkative\"}";
        //var setuResponse = JsonSerializer.Deserialize<ReceiveData>(zap);

        //var receiveJson = @"{""anonymous"":null,""font"":0,""group_id"":675106101,
        //    ""message"":""test for tlpmdm"",""message_id"":-1144564185,
        //    ""message_type"":""group"",""post_type"":""message"",
        //    ""raw_message"":""test for tlpmdm"",""self_id"":1366869256,""sender"":{
        //        ""age"":0,""area"":"""",""card"":""一只哈"",""level"":"""",
        //        ""nickname"":""淘气の雅美"",""role"":""owner"",""sex"":""unknown"",""Title"":"""",""user_id"":252961222
        //        },
        //    ""sub_type"":""normal"",""time"":1610090450,""user_id"":252961222}";
        //var receiveData = JsonConvert.DeserializeObject<ReceiveData>(receiveJson);

        #endregion Test

        var s = "狂战其实也能玩西洋剑";
        var jbs = new JiebaSegmenter();
        var tfidfExtractor = new TfidfExtractor(jbs);
        var result = tfidfExtractor.ExtractTagsWithWeight(s);
        ;

        //var tokens = posSeg.Cut(s);

        //var result = "pm今天玩原神了吗".SegmenterCut();
        ;
        //WordCloudExtensions.GenerateWordCloud();

        //var result = NSFWExtensions.CheckWebImage(
        //    "https://gchat.qpic.cn/gchatpic_new/252961222/484873217-2405041752-ED5B3C62736EB2ABB48CFA37443635B0/0?term=2&amp;is_origin=0");
        ;
    }

    [TestMethod]
    public void ProcessCQMessageTest_Common()
    {
        var message = ProcessCQMessage("zstlpmdm[CQ:at,qq=22222][CQ:face,id=123,kd=456]sheep");
        var element1 = message[0];
        Assert.AreEqual(ElementType.text, element1.Type);
        Assert.AreEqual("zstlpmdm", element1.Data.Text);

        var element2 = message[1];
        Assert.AreEqual(ElementType.at, element2.Type);
        Assert.AreEqual("22222", element2.Data.QQ);

        var element3 = message[2];
        Assert.AreEqual(ElementType.face, element3.Type);
        Assert.AreEqual("123", element3.Data.Id);

        var element4 = message[3];
        Assert.AreEqual(ElementType.text, element4.Type);
        Assert.AreEqual("sheep", element4.Data.Text);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementAt()
    {
        var element = ProcessCQAreaMessage("[CQ:at,qq=22222]");
        Assert.AreEqual(ElementType.at, element.Type);
        Assert.AreEqual("22222", element.Data.QQ);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementText()
    {
        var element = ProcessCQAreaMessage("zstlpmdm");
        Assert.AreEqual(ElementType.text, element.Type);
        Assert.AreEqual("zstlpmdm", element.Data.Text);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementFace()
    {
        var element = ProcessCQAreaMessage("[CQ:face,id=123,kd=456]");
        Assert.AreEqual(ElementType.face, element.Type);
        Assert.AreEqual("123", element.Data.Id);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementImage()
    {
        var element = ProcessCQAreaMessage("[CQ:image,File=http://baidu.com/1.jpg]");
        Assert.AreEqual(ElementType.image, element.Type);
        Assert.AreEqual("http://baidu.com/1.jpg", element.Data.File);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementRecord()
    {
        var element = ProcessCQAreaMessage("[CQ:record,File=http://baidu.com/1.mp3]");
        Assert.AreEqual(ElementType.record, element.Type);
        Assert.AreEqual("http://baidu.com/1.mp3", element.Data.File);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementVideo()
    {
        var element = ProcessCQAreaMessage("[CQ:video,File=http://baidu.com/1.mp4]");
        Assert.AreEqual(ElementType.video, element.Type);
        Assert.AreEqual("http://baidu.com/1.mp4", element.Data.File);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementRps()
    {
        var element = ProcessCQAreaMessage("[CQ:rps]");
        Assert.AreEqual(ElementType.rps, element.Type);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementDice()
    {
        var element = ProcessCQAreaMessage("[CQ:dice]");
        Assert.AreEqual(ElementType.dice, element.Type);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementShake()
    {
        var element = ProcessCQAreaMessage("[CQ:shake]");
        Assert.AreEqual(ElementType.shake, element.Type);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementPoke()
    {
        var element = ProcessCQAreaMessage("[CQ:poke,type=126,id=2003]");
        Assert.AreEqual(ElementType.poke, element.Type);
        Assert.AreEqual("126", element.Data.Type);
        Assert.AreEqual("2003", element.Data.Id);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementAnonymous()
    {
        var element = ProcessCQAreaMessage("[CQ:anonymous,ignore=0]");
        Assert.AreEqual(ElementType.anonymous, element.Type);
        Assert.AreEqual("0", element.Data.Ignore);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementShare()
    {
        var element = ProcessCQAreaMessage("[CQ:share,url=http://baidu.com,title=百度]");
        Assert.AreEqual(ElementType.share, element.Type);
        Assert.AreEqual("http://baidu.com", element.Data.Url);
        Assert.AreEqual("百度", element.Data.Title);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementContact()
    {
        var element = ProcessCQAreaMessage("[CQ:contact,type=qq,id=10001000]");
        Assert.AreEqual(ElementType.contact, element.Type);
        Assert.AreEqual("qq", element.Data.Type);
        Assert.AreEqual("10001000", element.Data.Id);

        element = ProcessCQAreaMessage("[CQ:contact,type=group,id=100100]");
        Assert.AreEqual(ElementType.contact, element.Type);
        Assert.AreEqual("group", element.Data.Type);
        Assert.AreEqual("100100", element.Data.Id);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementLocation()
    {
        var element = ProcessCQAreaMessage("[CQ:location,lat=39.8969426,lon=116.3109099,title=牌面,content=马厩]");
        Assert.AreEqual(ElementType.location, element.Type);
        Assert.AreEqual("39.8969426", element.Data.Lat);
        Assert.AreEqual("116.3109099", element.Data.Lon);
        Assert.AreEqual("牌面", element.Data.Title);
        Assert.AreEqual("马厩", element.Data.Content);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementMusic()
    {
        var element = ProcessCQAreaMessage("[CQ:music,type=163,id=28949129]");
        Assert.AreEqual(ElementType.music, element.Type);
        Assert.AreEqual("163", element.Data.Type);
        Assert.AreEqual("28949129", element.Data.Id);

        element = ProcessCQAreaMessage("[CQ:music,type=custom,Url=http://baidu.com,Audio=http://baidu.com/1.mp3,Title=音乐标题]");
        Assert.AreEqual(ElementType.music, element.Type);
        Assert.AreEqual("custom", element.Data.Type);
        Assert.AreEqual("http://baidu.com", element.Data.Url);
        Assert.AreEqual("http://baidu.com/1.mp3", element.Data.Audio);
        Assert.AreEqual("音乐标题", element.Data.Title);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementReply()
    {
        var element = ProcessCQAreaMessage("[CQ:reply,id=123456]");
        Assert.AreEqual(ElementType.reply, element.Type);
        Assert.AreEqual("123456", element.Data.Id);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementYM_Play()
    {
        ProcessCQAreaMessage("[CQ:ym_play,file=D:/Code/C#/WPF/SheepSoft/SheepQQBot3/SheepQQBot3.UnitTest/bin/Debug/Se/miao.wav]");
        // 测试听到声音
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementYM_Play3()
    {
        ProcessCQAreaMessage("[CQ:ym_play3,file=D:/Code/C#/WPF/SheepSoft/SheepQQBot3/SheepQQBot3.UnitTest/bin/Debug/Se/miao.wav]");
        // 测试听到声音
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementYM_Play3_Short()
    {
        ProcessCQAreaMessage("[CQ:ym_play3,file=Se/miao.wav]");
        // 测试听到声音
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementJson()
    {
        var element = ProcessCQAreaMessage(
            "[CQ:xml,data=<?xml version=\"1.0\" " +
            "encoding=\"utf-8\"?><msg templateID=\"12345\" action=\"web\" brief=\"RPG\" " +
            "serviceID=\"1\" Url=\"http://pcro.jp/\"><item layout=\"2\"><picture cover=\"\"/>" +
            "<Title>ぷちっとくろにくる</Title><summary>カワイイ</summary></item><source/></msg>,resid=1]");
        ;
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementYM_ifnotidle()
    {
        var element = ProcessCQAreaMessage("[CQ:ym_ifnotidle,data=5000]");
        Assert.AreEqual(ElementType.ym_ifnotidle, element.Type);
        Assert.AreEqual("5000", element.Data.Data);
    }
}