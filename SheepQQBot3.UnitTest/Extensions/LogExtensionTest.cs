namespace SheepQQBot3.UnitTest.Extensions
{
    [TestClass]
    public class LogExtensionTest
    {
        [TestMethod]
        public void ByteSubstringTest()
        {
            const string testStr = "我真是投了牌面的马!";
            Assert.AreEqual("我真是投了...", testStr.ByteSubstring(10));
            Assert.AreEqual("我真是投了...", testStr.ByteSubstring(11));
            Assert.AreEqual(testStr, testStr.ByteSubstring(50));
        }
    }
}