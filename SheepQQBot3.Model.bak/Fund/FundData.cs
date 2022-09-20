using Newtonsoft.Json;

namespace SheepQQBot3.Model.Fund
{
    public class FundData
    {
        public string Code { get; set; }

        public string Message { get; set; }

        public FundSimpleData[] Data { get; set; }
    }

    /// <summary>
    /// 简单基金信息Json数据类型
    /// </summary>
    public class FundSimpleData
    {
        /// <summary>
        /// 基金编号
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 基金名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 当前净值
        /// </summary>
        public float NetWorth { get; set; }

        /// <summary>
        /// 净值估算
        /// </summary>
        public float ExpectWorth { get; set; }

        /// <summary>
        /// 净值估算(涨跌幅)
        /// </summary>
        public float ExpectGrowth { get; set; }

        /// <summary>
        /// 日涨跌
        /// </summary>
        public float DayGrowth { get; set; }

        /// <summary>
        /// 最近一周涨跌
        /// </summary>
        public float LastWeekGrowth { get; set; }

        /// <summary>
        /// 最近1个月涨跌
        /// </summary>
        public float LastMonthGrowth { get; set; }

        /// <summary>
        /// 最近3个月涨跌
        /// </summary>
        public float LastThreeMonthsGrowth { get; set; }

        /// <summary>
        /// 最近半年涨跌
        /// </summary>
        public float LastSixMonthsGrowth { get; set; }

        /// <summary>
        /// 最近一年涨跌
        /// </summary>
        public float LastYearGrowth { get; set; }

        /// <summary>
        /// 净值更新日期
        /// </summary>
        public DateTime NetWorthDate { get; set; }

        /// <summary>
        /// 估算净值更新日期
        /// </summary>
        public DateTime ExpectWorthDate { get; set; }

        [JsonIgnore]
        public string ExpectGrowthString => FormatGrowth(ExpectGrowth);

        [JsonIgnore]
        public string LastWeekGrowthString => FormatGrowth(LastWeekGrowth);

        [JsonIgnore]
        public string LastMonthGrowthString => FormatGrowth(LastMonthGrowth);

        [JsonIgnore]
        public string LastThreeMonthsGrowthString => FormatGrowth(LastThreeMonthsGrowth);

        [JsonIgnore]
        public string LastSixMonthsGrowthString => FormatGrowth(LastSixMonthsGrowth);

        [JsonIgnore]
        public string LastYearGrowthString => FormatGrowth(LastYearGrowth);

        private static string FormatGrowth(float growthValue) => $"{(growthValue < 0 ? "－" : "＋")}{Math.Abs(growthValue):0.00}";
    }
}