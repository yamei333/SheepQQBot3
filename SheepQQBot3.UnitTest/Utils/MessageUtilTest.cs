using System.Text.Encodings.Web;
using System.Text.Json;
using SheepQQBot3.Model;
using static SheepQQBot3.SDK.Client.MessageUtil;

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
        var jsonData = JsonSerializer.Serialize(new SendData("send_group_msg", new ParamData
        {
            group_id = "11111",
            message = new List<Element>
            {
                new Element("text", new ElementBaseData("测试消息"))
            }
        }), new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        //var jsonData = JsonSerializer.Serialize(new TestClass
        //{
        //    Text1 = "text1",
        //});
        ;

        const int jsTimeStamp = 1610090450;
        //var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        var startTime = new DateTime(1970, 1, 1).Add(TimeZoneInfo.Local.BaseUtcOffset);
        var dt = startTime.AddSeconds(jsTimeStamp);
        ;

        //var receiveJson = @"{""anonymous"":null,""font"":0,""group_id"":675106101,
        //    ""message"":""test for tlpmdm"",""message_id"":-1144564185,
        //    ""message_type"":""group"",""post_type"":""message"",
        //    ""raw_message"":""test for tlpmdm"",""self_id"":1366869256,""sender"":{
        //        ""age"":0,""area"":"""",""card"":""一只哈"",""level"":"""",
        //        ""nickname"":""淘气の雅美"",""role"":""owner"",""sex"":""unknown"",""Title"":"""",""user_id"":252961222
        //        },
        //    ""sub_type"":""normal"",""time"":1610090450,""user_id"":252961222}";
        //var receiveData = JsonConvert.DeserializeObject<ReceiveData>(receiveJson);
        ;
    }

    [TestMethod]
    public void ProcessCQMessageTest_Common()
    {
        var message = ProcessCQMessage("zstlpmdm[CQ:at,qq=22222][CQ:face,id=123,kd=456]sheep");
        var element1 = message[0];
        Assert.AreEqual("text", element1.Type);
        Assert.AreEqual("zstlpmdm", element1.Data.Text);

        var element2 = message[1];
        Assert.AreEqual("at", element2.Type);
        Assert.AreEqual("22222", element2.Data.QQ);

        var element3 = message[2];
        Assert.AreEqual("face", element3.Type);
        Assert.AreEqual("123", element3.Data.Id);

        var element4 = message[3];
        Assert.AreEqual("text", element4.Type);
        Assert.AreEqual("sheep", element4.Data.Text);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementAt()
    {
        var element = ProcessCQAreaMessage("[CQ:at,qq=22222]");
        Assert.AreEqual("at", element.Type);
        Assert.AreEqual("22222", element.Data.QQ);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementText()
    {
        var element = ProcessCQAreaMessage("zstlpmdm");
        Assert.AreEqual("text", element.Type);
        Assert.AreEqual("zstlpmdm", element.Data.Text);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementFace()
    {
        var element = ProcessCQAreaMessage("[CQ:face,id=123,kd=456]");
        Assert.AreEqual("face", element.Type);
        Assert.AreEqual("123", element.Data.Id);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementImage()
    {
        var element = ProcessCQAreaMessage("[CQ:image,File=http://baidu.com/1.jpg]");
        Assert.AreEqual("image", element.Type);
        Assert.AreEqual("http://baidu.com/1.jpg", element.Data.File);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementRecord()
    {
        var element = ProcessCQAreaMessage("[CQ:record,File=http://baidu.com/1.mp3]");
        Assert.AreEqual("record", element.Type);
        Assert.AreEqual("http://baidu.com/1.mp3", element.Data.File);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementVideo()
    {
        var element = ProcessCQAreaMessage("[CQ:video,File=http://baidu.com/1.mp4]");
        Assert.AreEqual("video", element.Type);
        Assert.AreEqual("http://baidu.com/1.mp4", element.Data.File);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementRps()
    {
        var element = ProcessCQAreaMessage("[CQ:rps]");
        Assert.AreEqual("rps", element.Type);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementDice()
    {
        var element = ProcessCQAreaMessage("[CQ:dice]");
        Assert.AreEqual("dice", element.Type);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementShake()
    {
        var element = ProcessCQAreaMessage("[CQ:shake]");
        Assert.AreEqual("shake", element.Type);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementPoke()
    {
        var element = ProcessCQAreaMessage("[CQ:poke,Type=126,id=2003]");
        Assert.AreEqual("poke", element.Type);
        Assert.AreEqual("126", element.Data.Type);
        Assert.AreEqual("2003", element.Data.Id);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementAnonymous()
    {
        var element = ProcessCQAreaMessage("[CQ:anonymous,Ignore=0]");
        Assert.AreEqual("anonymous", element.Type);
        Assert.AreEqual("0", element.Data.Ignore);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementShare()
    {
        var element = ProcessCQAreaMessage("[CQ:share,Url=http://baidu.com,Title=百度]");
        Assert.AreEqual("share", element.Type);
        Assert.AreEqual("http://baidu.com", element.Data.Url);
        Assert.AreEqual("百度", element.Data.Title);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementContact()
    {
        var element = ProcessCQAreaMessage("[CQ:contact,Type=qq,id=10001000]");
        Assert.AreEqual("contact", element.Type);
        Assert.AreEqual("qq", element.Data.Type);
        Assert.AreEqual("10001000", element.Data.Id);

        element = ProcessCQAreaMessage("[CQ:contact,Type=group,id=100100]");
        Assert.AreEqual("contact", element.Type);
        Assert.AreEqual("group", element.Data.Type);
        Assert.AreEqual("100100", element.Data.Id);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementLocation()
    {
        var element = ProcessCQAreaMessage("[CQ:location,Lat=39.8969426,Lon=116.3109099,Title=牌面,Content=马厩]");
        Assert.AreEqual("location", element.Type);
        Assert.AreEqual("39.8969426", element.Data.Lat);
        Assert.AreEqual("116.3109099", element.Data.Lon);
        Assert.AreEqual("牌面", element.Data.Title);
        Assert.AreEqual("马厩", element.Data.Content);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementMusic()
    {
        var element = ProcessCQAreaMessage("[CQ:music,Type=163,id=28949129]");
        Assert.AreEqual("music", element.Type);
        Assert.AreEqual("163", element.Data.Type);
        Assert.AreEqual("28949129", element.Data.Id);

        element = ProcessCQAreaMessage("[CQ:music,Type=custom,Url=http://baidu.com,Audio=http://baidu.com/1.mp3,Title=音乐标题]");
        Assert.AreEqual("music", element.Type);
        Assert.AreEqual("custom", element.Data.Type);
        Assert.AreEqual("http://baidu.com", element.Data.Url);
        Assert.AreEqual("http://baidu.com/1.mp3", element.Data.Audio);
        Assert.AreEqual("音乐标题", element.Data.Title);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementReply()
    {
        var element = ProcessCQAreaMessage("[CQ:reply,id=123456]");
        Assert.AreEqual("reply", element.Type);
        Assert.AreEqual("123456", element.Data.Id);
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementYM_Play()
    {
        ProcessCQAreaMessage("[CQ:ym_play,File=D:/Code/C#/WPF/SheepSoft/SheepQQBot3/SheepQQBot3.UnitTest/bin/Debug/Se/miao.wav]");
        // 测试听到声音
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementYM_Play3()
    {
        ProcessCQAreaMessage("[CQ:ym_play3,File=D:/Code/C#/WPF/SheepSoft/SheepQQBot3/SheepQQBot3.UnitTest/bin/Debug/Se/miao.wav]");
        // 测试听到声音
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementYM_Play3_Short()
    {
        ProcessCQAreaMessage("[CQ:ym_play3,File=Se/miao.wav]");
        // 测试听到声音
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementJson()
    {
        var element = ProcessCQAreaMessage("[CQ:xml,Data=<?xml version=\"1.0\" + " +
                                           "encoding=\"utf-8\"?><msg templateID=\"12345\" action=\"web\" brief=\"RPG\" " +
                                           "serviceID=\"1\" Url=\"http://pcro.jp/\"><item layout=\"2\"><picture cover=\"\"/>" +
                                           "<Title>ぷちっとくろにくる</Title><summary>カワイイ</summary></item><source/></msg>,resid=1]");
        ;
    }

    [TestMethod]
    public void ProcessCQAreaMessageTest_ElementYM_ifnotidle()
    {
        var element = ProcessCQAreaMessage("[CQ:ym_ifnotidle,Data=5000]");
        Assert.AreEqual("ym_ifnotidle", element.Type);
        Assert.AreEqual("5000", element.Data.Data);
    }
}