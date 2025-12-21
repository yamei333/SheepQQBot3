using OpenRouter.NET;

namespace SheepQQBot3.UnitTest.Extensions;

[TestClass]
public class CQCodeTest
{
    [TestMethod]
    public void GetFundTest()
    {
        var client = new OpenRouterClient("OPEN_ROUTER_TOKEN");
        var chatResult = client.GenerateObjectAsync<ChatResult>("Test", "google/gemini-3-pro-preview").Result.Object;
        ;

        var cqImage = "Test[CQ:image,file=0000008e4e61704361744f6e65426f747c4d736746696c657c327c31353837333231377c373431323535353739323335303832343938307c37343132353535373932333530383234393739.E41029FAFAF7FEF383B07B67C0580CF8.png,sub_type=1,file_id=0000008e4e61704361744f6e65426f747c4d736746696c657c327c31353837333231377c373431323535353739323335303832343938307c37343132353535373932333530383234393739.E41029FAFAF7FEF383B07B67C0580CF8.png,url=https://multimedia.nt.qq.com.cn/download?appid=1407&amp;fileid=CgkyMDU1NTI2MDcSFDQdhrKniZHi8KuHqPhuAwYB-yuLGJQGIP8KKPnOjaO3tYgDUIC9owE&amp;spec=0&amp;rkey=CAISKKSBekjVG1fMn2OFZBUsWznM-yM-1yLzroA9-sizMX4iZlXWqL0XMwM,file_size=788,file_unique=E41029FAFAF7FEF383B07B67C0580CF8.png]";
        Assert.AreEqual($"Test[CQ:image,file=https://gchat.qpic.cn/download?appid=1407&amp;fileid=CgkyMDU1NTI2MDcSFDQdhrKniZHi8KuHqPhuAwYB-yuLGJQGIP8KKPnOjaO3tYgDUIC9owE&amp;spec=0&amp;rkey=CAISKKSBekjVG1fMn2OFZBUsWznM-yM-1yLzroA9-sizMX4iZlXWqL0XMwM]", CQCode.ReplaceCQImage(cqImage));
    }

    [TestMethod]
    public void ReplaceImageTest()
    {
        var cqImageMessage = "[CQ:image,file={DD1A9182-BF53-2FE2-C9DB-7CB26342EF22}.jpg,sub_type=0,url=https://gchat.qpic.cn/gchatpic_new/0/0-0-DD1A9182BF532FE2C9DB7CB26342EF22/0,file_size=52707]";
        var processMessage = CQCode.ReplaceCQImage(cqImageMessage);
        Assert.AreEqual($"Test[CQ:image,file=https://gchat.qpic.cn/download?appid=1407&amp;fileid=CgkyMDU1NTI2MDcSFDQdhrKniZHi8KuHqPhuAwYB-yuLGJQGIP8KKPnOjaO3tYgDUIC9owE&amp;spec=0&amp;rkey=CAISKKSBekjVG1fMn2OFZBUsWznM-yM-1yLzroA9-sizMX4iZlXWqL0XMwM]", CQCode.ReplaceCQImage(cqImageMessage));
    }
}