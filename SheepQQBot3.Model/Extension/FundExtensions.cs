using CommonLibrary;
using Masuit.Tools;
using Masuit.Tools.Systems;
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
    public static async Task<ConcurrentHashSet<FundData>> GetFundDataAsync(IEnumerable<string> fundIds)
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

        var taskList = new List<Task>();
        var fundDatas = new ConcurrentHashSet<FundData>();
        fundIdArray.ForEach(fundId =>
        {
            var task = new Task(() => GetFundData(fundId));
            taskList.Add(task);
            task.Start();
        });
        Task.WaitAll(taskList.ToArray());
        return fundDatas;

        void GetFundData(string fundId)
        {
            try
            {
                var url = $"https://fundgz.1234567.com.cn/js/{fundId}.js";
                var restResponse = HttpExtensions.Get<string>(url);
                var fundJson = _regGetFundJson.Match(restResponse.Content).Value;
                fundDatas.Add(fundJson.JsonDeserialize<FundData>());
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(e);
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
        ConcurrentHashSet<FundData> fundDatas,
        ConcurrentDictionary<int, AlarmFundConfig> fundAlarmConfigsDic)
    {
        var maxGrowth = -999.0;
        var allGrowth = 0.0;
        var isExistZero = false;
        var sb = new StringBuilder($"=====基金播报=====\r\n");
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

                sb.AppendLine($"{(string.IsNullOrEmpty(fundRemark) ? fundData.Name : fundRemark)}({fundData.Code}) {fundData.ExpectGrowthString}");
            });

        // MEMO : 日期错误, 今日不播报基金
        if (isDateError)
            return string.Empty;

        var avgGrowth = allGrowth / fundDatas!.Count;

        // MEMO : 添加怪话总结
        sb.AppendLine("================");
        switch (true)
        {
            case true when maxGrowth > 1.5 && avgGrowth > 1:
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
                }.Random());
                break;
            case true when maxGrowth > 1.5 && avgGrowth is >= 0 and <= 1:
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
                }.Random());
                break;
            case true when maxGrowth < 1.5 && avgGrowth < 0.5:
                sb.AppendLine(new[]
                {
                    "小绿",
                    "迷你绿",
                    "绿一点相当于没绿, 明天我又是崛起的一天",
                    "1个点也叫跌? 是技术性调整",
                    "不慌, 拿住就是赢",
                    "绿一点好上车",
                    "不要怕, 技术性回调",
                    "别人恐惧我加仓, 别人小亏我破产",
                }.Random());
                break;
            case true when avgGrowth is >= -0.5 and <= 0.5:
                sb.AppendLine(new[]
                {
                    "半吃半吐, 等于没吃",
                    "开始了, 都演起来了",
                    "演了一天, 你们不累吗",
                    "过了一天±0, 稳中向好",
                    "今日+0, 又是超越87%的人的一天",
                }.Random());
                break;
            case true when avgGrowth < 0:
                sb.AppendLine(new[]
                {
                    "绿光",
                    "不要怕, 是技术性调整",
                    "绿疯了",
                    "表面上绿了, 抄底就是现在",
                    "我恐惧成功?",
                    "这不梭一把?",
                    "现在就是抄底的时候!",
                }.Random());
                break;
            case true when isExistZero:
                sb.AppendLine(new[]
                {
                    "今天有人演戏, 我不说是谁",
                    "演了一天好累喔",
                    "演, 使劲演",
                    "又度过了虚无的一天",
                    "都可以演",
                    "演了一天就这???",
                }.Random());
                break;
        }

        return sb.ToString();
    }

    /// <summary>
    /// 获取基金阈值观测结果
    /// </summary>
    public static string GetFundLimitString(
        ConcurrentHashSet<FundData> fundDatas,
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