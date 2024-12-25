using System;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Fund;

public class FundData
{
    /// <summary>
    /// 基金编号
    /// </summary>
    [JsonPropertyName("fundcode")]
    public string Code { get; set; }

    /// <summary>
    /// 基金名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 当前净值(昨日)
    /// </summary>
    [JsonPropertyName("dwjz")]
    public string NetWorthSource { get; set; }

    /// <summary>
    /// 净值估算(今日)
    /// </summary>
    [JsonPropertyName("gsz")]
    public string ExpectWorthSource { get; set; }

    /// <summary>
    /// 净值估算(涨跌幅)
    /// </summary>
    [JsonPropertyName("gszzl")]
    public string ExpectGrowthSource { get; set; }

    /// <summary>
    /// 更新日期
    /// </summary>
    [JsonPropertyName("gztime")]
    public string UpdateDateSource { get; set; }

    [JsonIgnore]
    public float NetWorth => FloatParse(NetWorthSource);

    [JsonIgnore]
    public float ExpectWorth => FloatParse(ExpectWorthSource);

    [JsonIgnore]
    public float ExpectGrowth => FloatParse(ExpectGrowthSource);

    /// <summary>
    /// 净值估算(涨跌幅)(格式化)
    /// </summary>
    [JsonIgnore]
    public string ExpectGrowthString => FormatGrowth(ExpectGrowth);

    /// <summary>
    /// 净值估算(涨跌幅)(彩色格式化)
    /// </summary>
    [JsonIgnore]
    public string ExpectGrowthColorString => FormatGrowth(ExpectGrowth, "\ud83d\udd3a", "\ud83d\udfe2");

    /// <summary>
    /// 更新时间(格式化)
    /// </summary>
    [JsonIgnore]
    public DateTime UpdateDate => FormatDate(UpdateDateSource);

    private static string FormatGrowth(float growthValue, string plusString = null, string minusString = null) =>
        $"{(growthValue < 0 ? plusString ?? "－" : minusString ?? "＋")}{Math.Abs(growthValue):0.00}";

    private static float FloatParse(string parseValue) => float.TryParse(parseValue, out var floatValue) ? floatValue : 0;

    private static DateTime FormatDate(string dateString)
        => string.IsNullOrEmpty(dateString)
            ? DateTime.MinValue
            : DateTime.TryParse(dateString, out var dateValue)
                ? dateValue
                : DateTime.MinValue;
}

/// <summary>
/// 简单基金信息Json数据类型
/// </summary>
public class FundSimpleData
{
    ///// <summary>
    ///// 日涨跌
    ///// </summary>
    //[JsonPropertyName("dayGrowth")]
    //public string DayGrowthSource { get; set; }

    ///// <summary>
    ///// 最近一周涨跌
    ///// </summary>
    //[JsonPropertyName("lastWeekGrowth")]
    //public string LastWeekGrowthSource { get; set; }

    ///// <summary>
    ///// 最近1个月涨跌
    ///// </summary>
    //[JsonPropertyName("lastMonthGrowth")]
    //public string LastMonthGrowthSource { get; set; }

    ///// <summary>
    ///// 最近3个月涨跌
    ///// </summary>
    //[JsonPropertyName("lastThreeMonthsGrowth")]
    //public string LastThreeMonthsGrowthSource { get; set; }

    ///// <summary>
    ///// 最近半年涨跌
    ///// </summary>
    //[JsonPropertyName("lastSixMonthsGrowth")]
    //public string LastSixMonthsGrowthSource { get; set; }

    ///// <summary>
    ///// 最近一年涨跌
    ///// </summary>
    //[JsonPropertyName("lastYearGrowth")]
    //public string LastYearGrowthSource { get; set; }

    //[JsonIgnore]
    //public float DayGrowth => FloatParse(DayGrowthSource);

    //[JsonIgnore]
    //public float LastMonthGrowth => FloatParse(LastMonthGrowthSource);

    //[JsonIgnore]
    //public float LastWeekGrowth => FloatParse(LastWeekGrowthSource);

    //[JsonIgnore]
    //public float LastThreeMonthsGrowth => FloatParse(LastThreeMonthsGrowthSource);

    //[JsonIgnore]
    //public float LastSixMonthsGrowth => FloatParse(LastSixMonthsGrowthSource);

    //[JsonIgnore]
    //public float LastYearGrowth => FloatParse(LastYearGrowthSource);

    //[JsonIgnore]
    //public DateTime ExpectWorthDate => FormatDate(ExpectWorthDateSource);

    //[JsonIgnore] public string LastWeekGrowthString => FormatGrowth(LastWeekGrowth);

    //[JsonIgnore] public string LastThreeMonthsGrowthString => FormatGrowth(LastThreeMonthsGrowth);

    //[JsonIgnore] public string LastSixMonthsGrowthString => FormatGrowth(LastSixMonthsGrowth);

    //[JsonIgnore] public string LastYearGrowthString => FormatGrowth(LastYearGrowth);
}