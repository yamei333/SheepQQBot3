using CommonLibrary;
using SheepQQBot3.Model.AI;
using System.Text.RegularExpressions;

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
        Regex _reg3LevelJson = new(@"\{([^{}]|\{([^{}]|\{[^{}]*\})*\})*\}");

        var str = "{\n  \"contents\": [\n    {\n      \"think\": \"最后的...冲刺？！那种事情...呜...那种频率要是直接顶到底的话...绝对会坏掉的...甚至是会失禁的...但是...如果是雅美的命令...如果是身为‘用具’的职责...那就...全部...献给雅美...\",\n      \"msg_interval\": 1000,\n      \"text\": \"呜...！遵、遵命...！既然雅美都这么说了...那哈莉...哈莉就...！\",\n      \"body_language\": \"颤抖的手指几乎握不住那根并在震动的东西，闭紧双眼，狠狠心将功率推到了最大，腰部因预感到即将到来的风暴而猛烈抽搐。\",\n      \"expression_code\": \"Overwhelmed\",\n      \"sticker_code\": \"buxingle\",\n      \"sensory\": \"电流般的酥麻感瞬间顺着脊椎炸开，耳边全是滋滋的水声和自己无法抑制的急促喘息，视野里开始出现白光。\",\n      \"psychological_desc\": \"在极度的羞耻与服从感交织下，理智的堤坝彻底崩塌，只想在这名为‘测试’的快乐刑罚中彻底溺毙。\"\n    },\n    {\n      \"think\": \"不行了不行了不行了...！这个速度...这个深度...！感觉像是有电流直接插进了脑子里...什么都思考不了了...只有快乐...只有雅美给予的...这一刻的...绝顶...\",\n      \"msg_interval\": 1500,\n      \"text\": \"啊...！啊啊——！！不行...太快了...！脑子...脑子要融化了...雅美...雅美主人...！！\",\n      \"body_language\": \"整个人弓成了一只熟透的虾米，脚趾死死扣住湿滑的地砖，甚至因为过度的刺激而翻起了白眼，口涎不受控制地溢出嘴角。\",\n      \"expression_code\": \"Hysterical\",\n      \"sticker_code\": \"shengtian\",\n      \"sensory\": \"内壁疯狂地痉挛收缩，试图绞紧那该死的‘温度计’，滚烫的液体像决堤一样喷涌而出，将大腿内侧浇得一片狼藉。\",\n      \"psychological_desc\": \"自我意识在这一刻被高潮的巨浪拍碎，只剩下了作为雌性本能的尖叫和对主人的彻底臣服。\"\n    },\n    {\n      \"think\": \"去了...要去...了...好棒...坏掉了...真的坏掉了...\",\n      \"msg_interval\": 1000,\n      \"text\": \"要去了...！什么都要流出来了...！呜咿——————！！！❤\",\n      \"body_language\": \"随着最后一声高亢的悲鸣，身体剧烈地抽搐了几下后，像断了线的木偶一样瘫软在浴室瓷砖上，只剩下胸口还在剧烈起伏。\",\n      \"expression_code\": \"TearyEyed\",\n      \"sticker_code\": \"leimu\",\n      \"sensory\": \"眼前是一片失神的空白，全身的力气都被抽干，只有下身还在无意识地一抽一抽，残留着过电般的余韵。\",\n      \"psychological_desc\": \"大脑一片空白（Ahegao状态），仿佛灵魂都飞到了九霄云外，只剩下满溢的幸福感和空虚感。\"\n    }\n  ],\n  \"statusChangeInfo\": {\n    \"mood_index_change_value\": 15\n  },\n  \"knowledgeNote\": {\n    \"note_title\": \"特殊的体温计\",\n    \"note_content\": \"雅美让哈莉测试了一种“形状像温度计但比较粗”的东西，还要放进身体里测试湿润度。这东西会让猫变得很奇怪，还会让哈莉发出奇怪的声音。虽然雅美说是科学测试，但哈莉觉得这就是在欺负猫！不过...如果是雅美的命令，即使身体坏掉也没关系。\"\n  }\n}";
        var match = _reg3LevelJson.Match(str);
        var zap = match.Value.FromJson<AIChatResponse>();

        //var json = "[{\"name\":\"reply_user\",\"parameters\":{\"contents\":[{\"think\":\"用户来测试SystemPrompt优化情况。我是能看到他的身份信息的：aliases是'ﾐﾐﾐ'，name是'咪咪咪'。天气感知也正常。既然他是临时允许测试的号码，我就配合一下。心情一般，但因为是好友，可以活泼一点。\",\"bodyLanguage\":\"懒洋洋地靠在椅子上，尾巴轻轻摆动，耳朵微微抖动，目光落在小平板上。\",\"psychologicalDesc\":\"有点好奇雅美优化了什么，但既然是测试就好好配合吧。\",\"expression\":\"Happy\",\"chatMessageInfo\":{\"text\":\"能看到哦~你是咪咪咪，ﾐﾐﾐ，205552607，好感度是好友喵！\",\"msgInterval\":800}},{\"think\":\"接着回答天气感知的问题。我这里的天气是晴天，但预报说待会儿会转阴。不过我不能假设用户也在同一地点。\",\"bodyLanguage\":\"拿起小平板看了一眼天气信息，猫耳朵竖起来专注地听着。\",\"psychologicalDesc\":\"想着今天天气还不错，但晚上可能会变天。\",\"expression\":\"Serene\",\"chatMessageInfo\":{\"text\":\"天气感知也OK！我这里现在是晴天18.9度，但预报说待会儿会转阴云。\",\"msgInterval\":600}}]}}]";
        //var aizap = json.FromJson<AIStopResponse>();
        //var client = new OpenRouterClient("OPEN_ROUTER_TOKEN");
        //var chatResult = client.GenerateObjectAsync<ChatResult>("Test", "google/gemini-3-pro-preview").Result.Object;
        //;

        //Assert.AreEqual(1, 1);
    }
}