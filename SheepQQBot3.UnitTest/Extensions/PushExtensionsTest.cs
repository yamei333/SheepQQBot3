namespace SheepQQBot3.UnitTest.Extensions;

[TestClass]
public class PushExtensionsTest
{
    [TestMethod]
    public void PushBarkTest()
    {
        var type = PushExtensions.PushBarkMessageAsync("真是投了牌面的马", "PM跑马场").Result;
    }
}