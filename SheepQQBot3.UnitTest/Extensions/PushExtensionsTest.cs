namespace SheepQQBot3.UnitTest.Extensions
{
    [TestClass]
    public class PushExtensionsTest
    {
        [TestMethod]
        public void PushBarkTest()
        {
            PushExtensions.PushBarkMessage(message: "真是投了牌面的马", title: "PM跑马场");
        }
    }
}