namespace SheepQQBot3.UnitTest.Extensions;

public class ChatResult
{
    public string Result { get; set; }
}

[TestClass]
public class NormalTest
{
    /// <summary>
    /// 测试代码用测试
    /// </summary>
    [TestMethod]
    public void TestTest()
    {
        //var json = "[{\"name\":\"reply_user\",\"parameters\":{\"contents\":[{\"think\":\"用户来测试SystemPrompt优化情况。我是能看到他的身份信息的：aliases是'ﾐﾐﾐ'，name是'咪咪咪'。天气感知也正常。既然他是临时允许测试的号码，我就配合一下。心情一般，但因为是好友，可以活泼一点。\",\"bodyLanguage\":\"懒洋洋地靠在椅子上，尾巴轻轻摆动，耳朵微微抖动，目光落在小平板上。\",\"psychologicalDesc\":\"有点好奇雅美优化了什么，但既然是测试就好好配合吧。\",\"expression\":\"Happy\",\"chatMessageInfo\":{\"text\":\"能看到哦~你是咪咪咪，ﾐﾐﾐ，205552607，好感度是好友喵！\",\"msgInterval\":800}},{\"think\":\"接着回答天气感知的问题。我这里的天气是晴天，但预报说待会儿会转阴。不过我不能假设用户也在同一地点。\",\"bodyLanguage\":\"拿起小平板看了一眼天气信息，猫耳朵竖起来专注地听着。\",\"psychologicalDesc\":\"想着今天天气还不错，但晚上可能会变天。\",\"expression\":\"Serene\",\"chatMessageInfo\":{\"text\":\"天气感知也OK！我这里现在是晴天18.9度，但预报说待会儿会转阴云。\",\"msgInterval\":600}}]}}]";
        //var aizap = json.FromJson<AIStopResponse>();
        //var client = new OpenRouterClient("OPEN_ROUTER_TOKEN");
        //var chatResult = client.GenerateObjectAsync<ChatResult>("Test", "google/gemini-3-pro-preview").Result.Object;
        //;

        //Assert.AreEqual(1, 1);
    }
}