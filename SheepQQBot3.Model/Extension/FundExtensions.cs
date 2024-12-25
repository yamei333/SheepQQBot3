using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Fund;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SheepQQBot3.Model.Extension;

public static class FundExtensions
{
    /// <summary>
    /// 取得基金Json的正则
    /// </summary>
    private static readonly Regex _regGetFundJson = new(@"(?<=jsonpgz\().+(?=\);)", RegexOptions.Multiline);

    /// <summary>
    /// 获取基金基本信息
    /// </summary>
    /// <param name="fundIds"></param>
    /// <returns></returns>
    public static async Task<FundData[]> GetFundDatasAsync(IEnumerable<string> fundIds)
    {
        var fundIdArray = fundIds as string[] ?? fundIds.ToArray();
        if (fundIdArray.Any() != true)
            return null;

        // MEMO : doctorxiong 炸了
        //var url = $"https://api.doctorxiong.club/v1/fund?code={string.Join(",", fundIdArray)}"
        // MEMO : 0.14.7.7 cnuseful 炸了
        //var url = $"https://www.cnuseful.com/api/index/fund?code={string.Join(",", fundIdArray)}";
        //var httpResponse = await HttpExtensions.GetFromJsonAsync<FundData>(url).ConfigureAwait(false);
        //return httpResponse.Result == HttpResponseResult.Successed
        //    ? httpResponse.Data : null;

        var tasks = fundIdArray.Select(GetFundDataAsync);
        return await Task.WhenAll(tasks).ConfigureAwait(false);

        async Task<FundData> GetFundDataAsync(string fundId)
        {
            try
            {
                var url = $"https://fundgz.1234567.com.cn/js/{fundId}.js";
                var httpResponse = await HttpExtensions.HttpGetAsync(url).ConfigureAwait(false);
                var fundJson = _regGetFundJson.Match(await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false)).Value;
                return fundJson.JsonDeserialize<FundData>();
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(e);
                return null;
            }
        }

        #region 测试用代码

        //var httpResponse = await HttpExtensions.HttpGetAsync(url).ConfigureAwait(false);
        //var fundJsonText = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        //try
        //{
        //    var fundData = JsonExtensions.Deserialize<FundData>(fundJsonText);
        //}
        //catch (Exception e)
        //{
        //    Console.WriteLine(e);
        //    throw;
        //}
        //return null;

        #endregion 测试用代码
    }

    /// <summary>
    /// 获取基金持仓信息
    /// </summary>
    /// <param name="fundId"></param>
    /// <returns></returns>
    public static async Task<FundPostionData> GetFundPositionDataAsync(string fundId)
    {
        if (string.IsNullOrEmpty(fundId))
            return null;

        var httpResponse = await HttpExtensions
            .GetFromJsonAsync<FundPostionData>($"https://api.doctorxiong.club/v1/fund/position?code={fundId}")
            .ConfigureAwait(false);
        return httpResponse.Result == HttpResponseResult.Successed
            ? httpResponse.Data : null;
    }

    public static string GetFundAlarmString(
        FundData[] fundDatas,
        ConcurrentDictionary<int, AlarmFundConfig> fundAlarmConfigsDic)
    {
        var maxGrowth = -999.0;
        var allGrowth = 0.0;
        var isExistZero = false;
        var sb = new StringBuilder($"========基金播报========\r\n");
        var isDateError = false;
        var fundAlarmConfigs = fundAlarmConfigsDic.Values;

        fundDatas.OrderBy(each => each.Code)
            .ForEach(fundData =>
            {
                if (fundData.UpdateDate.ToString("yyyy-MM-dd") != DateTime.Now.ToString("yyyy-MM-dd"))
                {
                    isDateError = true;
                    return;
                }

                var fundAlarmConfig = fundAlarmConfigs
                    .First(alarmFundConfig => alarmFundConfig.FundId == fundData.Code);
                var fundRemark = fundAlarmConfig.FundRemark;
                var growth = fundData.ExpectGrowth;
                if (growth > maxGrowth)
                    maxGrowth = growth;

                allGrowth += growth;
                if (!isExistZero && Math.Abs(growth) <= 0.1)
                    isExistZero = true;

                //sb.AppendLine($"{(string.IsNullOrEmpty(fundRemark) ? fundData.Name : fundRemark)}({fundData.Code}) {fundData.ExpectGrowthString}");
                sb.AppendLine($"{fundData.Code}|{fundData.ExpectGrowthString} {(string.IsNullOrEmpty(fundRemark) ? fundData.Name : fundRemark)}");
            });

        // MEMO : 日期错误, 今日不播报基金
        if (isDateError)
            return string.Empty;

        var fundCount = fundDatas.Length;
        var midGrowth = fundDatas.OrderBy(each => each.ExpectGrowth).Skip(fundCount / 2 - 1).First().ExpectGrowth;
        // MEMO : 中位平均值
        var avgGrowth = allGrowth / fundCount;
        // MEMO : 中位平均值
        var midAvgGrowth = (avgGrowth + midGrowth) / 2;

        // MEMO : 添加怪话总结
        sb.AppendLine("=======================");
        switch (true)
        {
            case true when midAvgGrowth > 1:
                sb.AppendLine(new[]
                {
                    "红枫!",
                    "超级红枫!",
                    "洪峰!",
                    "xzp大吃特吃",
                    "xzp血吃一波",
                    "日赚1w就在今日",
                    "今天我就是恐惧魔王?",
                    "我贪婪成功?",
                    "立即买入, 我又是专家了",
                    "上涨的恐惧!",
                    "今天要把昨天失去的全部拿回来",
                    "震荡期就赚震荡的钱",
                    "资金都跑了, 今天也就反抽一下",
                    "尾盘涨的你头晕目眩!",
                    "我就是亚洲t王",
                }.Random());
                break;
            case true when midAvgGrowth is >= 0.2 and <= 1:
                sb.AppendLine(new[]
                {
                    "迷你吃",
                    "一般, 迷你吃",
                    "小吃一波",
                    "小吃不算吃",
                    "1个点也叫涨?",
                    "吃这点不够塞牙缝的",
                    "我大贪特贪",
                    "我抄底成功?",
                    "大A就是你的提款机",
                    "牛回速归",
                    "蜗牛也是牛",
                }.Random());
                break;
            case true when midAvgGrowth is > -0.2 and < 0.2:
                sb.AppendLine(new[]
                {
                    "半吃半吐, 等于没吃",
                    "开始了, 都演起来了",
                    "演了一天, 你们不累吗",
                    "过了一天±0, 稳中向好",
                    "今日+0, 又是超越87%的人的一天",
                    "今天有人演戏, 我不说是谁",
                    "演了一天好累喔",
                    "演, 使劲演",
                    "又度过了虚无的一天",
                    "都可以演",
                    "演了一天就这???",
                    "今天有上涨的恐惧吗?",
                    "要的就是这种无风险的感觉",
                    "股票之道在于人弃我取",
                    "相信今天是大奇迹日",
                }.Random());
                break;
            case true when midAvgGrowth is >= -1 and <= -0.2:
                sb.AppendLine(new[]
                {
                    "小绿",
                    "迷你绿",
                    "绿一点相当于没绿, 明天我又是崛起的一天",
                    "1个点也叫跌? 是技术性调整",
                    "不慌, 拿住就是赢",
                    "绿一点好上车",
                    "别人恐惧我加仓, 别人小亏我破产",
                    "这玩意有一天不是全绿的吗",
                    "低位怕个鸟",
                    "今天的流出都是明天的追高",
                    "站在青铜仰望钻石",
                    "起视四境, 而空头又至矣",
                    "做好人, 买好股, 得好报",
                    "是时候团结起来让外资看看我们的力量!",
                    "明天有上涨的恐惧吗?",
                    "不跌还就不进了",
                    "把钱当欢乐豆玩了",
                    "我觉得现在是黎明前的黑暗",
                }.Random());
                break;
            case true when midAvgGrowth < -1:
                sb.AppendLine(new[]
                {
                    "一片绿光!",
                    "不要怕, 是技术性调整",
                    "绿疯了",
                    "表面上绿了, 抄底就是现在",
                    "我恐惧成功?",
                    "这不梭一把?",
                    "现在就是抄底的时候!",
                    "这是倒车接人的信号!",
                    "持仓的至暗时刻",
                    "今日割五万, 明日割十万",
                    "散户之力有限, 空头之欲无厌",
                    "经典大面夹小肉",
                    "大A诈骗市场",
                    "大家一起下地狱",
                    "相信牛市, 会有救的",
                    "保卫大盘!",
                    "明天阳上影, 后天反包",
                    "老乡别走, 在V了",
                    "回到家, 煮了点面吃, 没有放盐",
                    "都是外资搞得鬼",
                    "空头没吃饭吗? 就这!",
                    "满屏绿色",
                    "爱是一道光",
                }.Random());
                break;
        }

        return sb.ToString();
    }

    /// <summary>
    /// 获取基金阈值观测结果
    /// </summary>
    public static string GetFundLimitString(
        FundData[] fundDatas,
        LimitObserveFundConfig[] limitObserveFundConfigs)
    {
        // TODO : 基金取历史数据坏了, 暂时禁用该功能
        return string.Empty;
        //var sb = new StringBuilder();
        //sb.AppendLine("=====基金阈值观测=====");
        //var hasObserveMessage = false;
        //var isDateError = false;
        //fundData.Data.ForEach(each =>
        //{
        //    if (each.ExpectWorthDate.ToString("yyyy-MM-dd") != DateTime.Now.ToString("yyyy-MM-dd"))
        //    {
        //        isDateError = true;
        //        return;
        //    }

        //    var fundLimitConfigs = limitObserveFundConfigs
        //        .Where(fundLimit => fundLimit.FundId == each.Code)
        //        .ToArray();
        //    fundLimitConfigs.ForEach(fundLimitConfig =>
        //    {
        //        var isPositive = fundLimitConfig.AlertLimit > 0;
        //        var alertLimit = fundLimitConfig.AlertLimit;
        //        var fundObserveType = fundLimitConfig.FundObserveType;
        //        // MEMO : 添加怪话
        //        switch (fundObserveType)
        //        {
        //            case FundObserveType.Week when isPositive
        //                ? each.LastWeekGrowth > alertLimit
        //                : each.LastWeekGrowth < alertLimit:
        //            case FundObserveType.Month when isPositive
        //                ? each.LastMonthGrowth > alertLimit
        //                : each.LastMonthGrowth < alertLimit:
        //            case FundObserveType.ThreeMonths when isPositive
        //                ? each.LastThreeMonthsGrowth > alertLimit
        //                : each.LastThreeMonthsGrowth < alertLimit:
        //            case FundObserveType.SixMonths when isPositive
        //                ? each.LastSixMonthsGrowth > alertLimit
        //                : each.LastSixMonthsGrowth < alertLimit:
        //            case FundObserveType.Year when isPositive
        //                ? each.LastYearGrowth > alertLimit
        //                : each.LastYearGrowth < alertLimit:
        //                sb.AppendLine($"{each.Name}({each.Code}) 在过去[{GetObserveTypeString(fundObserveType)}]波动超过{(alertLimit >= 0 ? "＋" : "－")}{Math.Abs(alertLimit)}, {GetMessage(isPositive)}");
        //                hasObserveMessage = true;
        //                break;
        //        }
        //    });
        //});

        //// MEMO : 日期错误, 今日不观测
        //if (isDateError)
        //    return string.Empty;

        //if (hasObserveMessage)
        //{
        //    sb.AppendLine("==================");
        //    return sb.ToString();
        //}
        //else
        //{
        //    return string.Empty;
        //}

        //string GetObserveTypeString(FundObserveType fundObserveType)
        //    => fundObserveType switch
        //    {
        //        FundObserveType.Week => "1周",
        //        FundObserveType.Month => "1个月",
        //        FundObserveType.ThreeMonths => "3个月",
        //        FundObserveType.SixMonths => "6个月",
        //        FundObserveType.Year => "1年",
        //        _ => "未知时间"
        //    };

        //string GetMessage(bool isPositive) => isPositive ? GetPositiveMessage() : GetNegativeMessage();

        //string GetPositiveMessage()
        //{
        //    return new[]
        //    {
        //        "该恐惧了?",
        //        "现在我就是恐惧魔王?",
        //        "我究极恐惧",
        //        "恐惧魔王徐州芃",
        //        "我觉得吃得差不多了",
        //    }.Random();
        //}

        //string GetNegativeMessage()
        //{
        //    return new[]
        //    {
        //        "再不逃裤子都没了",
        //        "现在就是抄底之时?",
        //        "别人恐惧我加仓",
        //        "我成功恐惧?",
        //        "就是现在! 满仓, 满仓!",
        //    }.Random();
        //}
    }
}