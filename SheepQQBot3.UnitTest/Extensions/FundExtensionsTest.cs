using System.Collections.Concurrent;
using SheepQQBot3.Model.Config;

namespace SheepQQBot3.UnitTest.Extensions
{
    [TestClass]
    public class FundExtensionsTest
    {
        [TestMethod]
        public void GetFundTest()
        {
            var fundData = FundExtensions.GetFundData("004235", "161725")!;
            Assert.AreEqual("中欧价值智选混合C", fundData.Data?[0].Name);
            Assert.AreEqual("招商中证白酒指数(LOF)A", fundData.Data?[1].Name);

            var dic = new ConcurrentDictionary<int, AlarmFundConfig>();
            dic.TryAdd(0, new AlarmFundConfig("004235", "", true));
            dic.TryAdd(1, new AlarmFundConfig("161725", "白酒", true));
            var alarmString = FundExtensions.GetFundAlarmString(fundData, dic);
            ;
        }

        [TestMethod]
        public void GetFundPositionTest()
        {
            //var jsonText =
            //    "{\"code\":200,\"message\":\"操作成功\",\"data\":{\"title\":\"中欧价值智选混合C  2022年2季度股票投资明细\",\"date\":\"2022-06-30\",\"stock\":\"94.78%\",\"bond\":\"4.14%\",\"cash\":\"2.62%\",\"total\":\"134.98\",\"stockList\":[[\"300014\",\"亿纬锂能\",\"8.08%\",\"1,118.86\",\"109,089.29\"],[\"603678\",\"火炬电子\",\"7.80%\",\"2,250.25\",\"105,334.39\"],[\"002180\",\"纳思达\",\"5.90%\",\"1,573.64\",\"79,657.60\"],[\"603267\",\"鸿远电子\",\"5.78%\",\"580.93\",\"78,035.72\"],[\"002643\",\"万润股份\",\"3.44%\",\"2,225.86\",\"46,431.41\"],[\"002048\",\"宁波华翔\",\"3.29%\",\"2,770.81\",\"44,388.42\"],[\"002384\",\"东山精密\",\"2.84%\",\"1,668.88\",\"38,267.50\"],[\"600970\",\"中材国际\",\"2.70%\",\"3,757.34\",\"36,408.62\"],[\"688036\",\"传音控股\",\"2.59%\",\"392.19\",\"34,995.24\"],[\"688198\",\"佰仁医疗\",\"2.53%\",\"280.12\",\"34,152.16\"]]}}";
            //var fundPostionData = JsonConvert.DeserializeObject<FundPostionData>(jsonText);
            var fundPositionData = FundExtensions.GetFundPositionData("004235");
            Assert.AreEqual(200, fundPositionData.Code);
            ;
        }
    }
}