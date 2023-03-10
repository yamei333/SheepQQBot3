using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.Json;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Fund;
using Yamei.Common;

namespace SheepQQBot3.Model.Extension
{
    public static class FundExtensions
    {
        /// <summary>
        /// 获取基金基本信息
        /// </summary>
        /// <param name="fundIds"></param>
        /// <returns></returns>
        public static FundData GetFundData(params string[] fundIds)
        {
            if (fundIds.Length == 0)
                return null;

            var fundJsonText = HttpExtensions.GetString($"https://api.doctorxiong.club/v1/fund?code={string.Join(",", fundIds)}");
            if (string.IsNullOrEmpty(fundJsonText))
                return null;

            //var fundJsonText =
            //    "{\"code\":200,\"message\":\"操作成功\",\"traceId\":\"991464084dbc4e22f12188d4d19c1a72\",\"data\":[{\"code\":\"004235\",\"name\":\"中欧价值智选混合C\",\"netWorth\":4.5556,\"expectWorth\":4.5106,\"totalWorth\":4.5556,\"expectGrowth\":\"-0.99\",\"dayGrowth\":\"-0.94\",\"lastWeekGrowth\":\"1.3459\",\"lastMonthGrowth\":\"3.31\",\"lastThreeMonthsGrowth\":\"24.72\",\"lastSixMonthsGrowth\":\"-6.85\",\"lastYearGrowth\":\"-3.87\",\"netWorthDate\":\"2022-07-22\",\"expectWorthDate\":\"2022-07-25 13:19:00\"},{\"code\":\"161725\",\"name\":\"招商中证白酒指数(LOF)A\",\"netWorth\":1.1816,\"expectWorth\":1.1861,\"totalWorth\":2.8977,\"expectGrowth\":\"0.38\",\"dayGrowth\":\"-0.2\",\"lastWeekGrowth\":\"-1.1875\",\"lastMonthGrowth\":\"-0.34\",\"lastThreeMonthsGrowth\":\"11.18\",\"lastSixMonthsGrowth\":\"-4.48\",\"lastYearGrowth\":\"-14.22\",\"netWorthDate\":\"2022-07-22\",\"expectWorthDate\":\"2022-07-25 13:18:00\"}]}";
            var fundData = JsonSerializer.Deserialize<FundData>(fundJsonText);
            return fundData;
        }

        /// <summary>
        /// 获取基金持仓信息
        /// </summary>
        /// <param name="fundId"></param>
        /// <returns></returns>
        public static FundPostionData GetFundPositionData(string fundId)
        {
            if (string.IsNullOrEmpty(fundId))
                return null;

            var fundJsonText = HttpExtensions.GetString($"https://api.doctorxiong.club/v1/fund/position?code={fundId}");
            if (string.IsNullOrEmpty(fundJsonText))
                return null;

            var fundPostionData = JsonSerializer.Deserialize<FundPostionData>(fundJsonText);
            return fundPostionData;
        }

        public static string GetFundAlarmString(
            FundData fundData,
            ConcurrentDictionary<int, AlarmFundConfig> fundAlarmConfigsDic)
        {
            var sb = new StringBuilder();
            var maxGrowth = -999.0;
            var allGrowth = 0.0;
            var isExistZero = false;
            sb.AppendLine("=====基金播报=====");
            var fundSimpleData = fundData.Data;
            var isDateError = false;
            var fundAlarmConfigs = fundAlarmConfigsDic.Values;
            fundSimpleData?.ForEach(each =>
            {
                if (each.ExpectWorthDate.ToString("yyyy-MM-dd") != DateTime.Now.ToString("yyyy-MM-dd"))
                {
                    isDateError = true;
                    return;
                }

                var fundAlarmConfig = fundAlarmConfigs
                    .First(alarmFundConfig => alarmFundConfig.FundId == each.Code);
                var fundRemark = fundAlarmConfig.FundRemark;
                var growth = each.ExpectGrowth;
                if (growth > maxGrowth)
                    maxGrowth = growth;

                allGrowth += growth;
                if (!isExistZero && Math.Abs(growth) <= 0.1)
                    isExistZero = true;

                sb.AppendLine($"{(string.IsNullOrEmpty(fundRemark) ? each.Name : fundRemark)}({each.Code}) {each.ExpectGrowthString}");
            });

            // MEMO : 日期错误, 今日不播报基金
            if (isDateError)
                return string.Empty;

            var avgGrowth = allGrowth / fundSimpleData.Length;

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
                        "ruojiji2的剧团",
                        "今天就是奥斯卡金像奖诞生之时",
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
                        "今天就是奥斯卡金像奖诞生之时",
                    }.Random());
                    break;
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取基金阈值观测结果
        /// </summary>
        public static string GetFundLimitString(
            FundData fundData,
            LimitObserveFundConfig[] limitObserveFundConfigs)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=====基金阈值观测=====");
            var hasObserveMessage = false;
            var isDateError = false;
            fundData.Data.ForEach(each =>
            {
                if (each.ExpectWorthDate.ToString("yyyy-MM-dd") != DateTime.Now.ToString("yyyy-MM-dd"))
                {
                    isDateError = true;
                    return;
                }

                var fundLimitConfigs = limitObserveFundConfigs
                    .Where(fundLimit => fundLimit.FundId == each.Code)
                    .ToArray();
                fundLimitConfigs.ForEach(fundLimitConfig =>
                {
                    var isPositive = fundLimitConfig.AlertLimit > 0;
                    var alertLimit = fundLimitConfig.AlertLimit;
                    var fundObserveType = fundLimitConfig.FundObserveType;
                    // MEMO : 添加怪话
                    switch (fundObserveType)
                    {
                        case FundObserveType.Week when isPositive
                            ? each.LastWeekGrowth > alertLimit
                            : each.LastWeekGrowth < alertLimit:
                        case FundObserveType.Month when isPositive
                            ? each.LastMonthGrowth > alertLimit
                            : each.LastMonthGrowth < alertLimit:
                        case FundObserveType.ThreeMonths when isPositive
                            ? each.LastThreeMonthsGrowth > alertLimit
                            : each.LastThreeMonthsGrowth < alertLimit:
                        case FundObserveType.SixMonths when isPositive
                            ? each.LastSixMonthsGrowth > alertLimit
                            : each.LastSixMonthsGrowth < alertLimit:
                        case FundObserveType.Year when isPositive
                            ? each.LastYearGrowth > alertLimit
                            : each.LastYearGrowth < alertLimit:
                            sb.AppendLine($"{each.Name}({each.Code}) 在过去[{GetObserveTypeString(fundObserveType)}]波动超过{(alertLimit >= 0 ? "＋" : "－")}{Math.Abs(alertLimit)}, {GetMessage(isPositive)}");
                            hasObserveMessage = true;
                            break;
                    }
                });
            });

            // MEMO : 日期错误, 今日不观测
            if (isDateError)
                return string.Empty;

            if (hasObserveMessage)
            {
                sb.AppendLine("==================");
                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }

            string GetObserveTypeString(FundObserveType fundObserveType)
                => fundObserveType switch
                {
                    FundObserveType.Week => "1周",
                    FundObserveType.Month => "1个月",
                    FundObserveType.ThreeMonths => "3个月",
                    FundObserveType.SixMonths => "6个月",
                    FundObserveType.Year => "1年",
                    _ => "未知时间"
                };

            string GetMessage(bool isPositive) => isPositive ? GetPositiveMessage() : GetNegativeMessage();

            string GetPositiveMessage()
            {
                return new[]
                {
                    "该恐惧了?",
                    "现在我就是恐惧魔王?",
                    "我究极恐惧",
                    "恐惧魔王徐州芃",
                    "我觉得吃得差不多了",
                }.Random();
            }

            string GetNegativeMessage()
            {
                return new[]
                {
                    "再不逃裤子都没了",
                    "现在就是抄底之时?",
                    "别人恐惧我加仓",
                    "我成功恐惧?",
                    "就是现在! 满仓, 满仓!",
                }.Random();
            }
        }
    }
}