namespace SheepQQBot3.UnitTest.Extensions;

[TestClass]
public class SetuExtensionsTest
{
    [TestMethod]
    public void GetLoliconSetuTest()
    {
        //var setuUrl = SetuExtensions.GetSetu_Yuban("").Result;
        //var json = "{\"code\":\"200\",\"message\":\"OK\",\"time\":1640068142,\"version\":\"1.0.0\",\"Data\":{\"num\":\"1\",\"function\":\"1\",\"Url\":[\"https:\\/\\/floral-disk-7293.h123hh.workers.dev\\/img-original\\/img\\/2019\\/05\\/31\\/00\\/00\\/02\\/74980758_p0.png\"]}}";
        //var setuRes = JsonSerializer.Deserialize<SetuResponse_NyanCatda>(json);
        var setuRes = SetuExtensions.GetSetu_NyanCatda("").Result;
        ;
    }
}