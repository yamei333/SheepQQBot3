using System;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Fund;

public class FundData
{
    [JsonPropertyName("code")] public int Code { get; set; }

    [JsonPropertyName("message")] public string Message { get; set; }

    [JsonPropertyName("data")] public FundSimpleData[] Data { get; set; }
}

/// <summary>
/// 简单基金信息Json数据类型
/// </summary>
public class FundSimpleData
{
    /// <summary>
    /// 基金编号
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; }

    /// <summary>
    /// 基金名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 当前净值
    /// </summary>
    [JsonPropertyName("netWorth")]
    public float NetWorth { get; set; }

    /// <summary>
    /// 净值估算
    /// </summary>
    [JsonPropertyName("expectWorth")]
    public float ExpectWorth { get; set; }

    /// <summary>
    /// 净值估算(涨跌幅)
    /// </summary>
    [JsonPropertyName("expectGrowth")]
    public string ExpectGrowthSource { get; set; }

    /// <summary>
    /// 日涨跌
    /// </summary>
    [JsonPropertyName("dayGrowth")]
    public string DayGrowthSource { get; set; }

    /// <summary>
    /// 最近一周涨跌
    /// </summary>
    [JsonPropertyName("lastWeekGrowth")]
    public string LastWeekGrowthSource { get; set; }

    /// <summary>
    /// 最近1个月涨跌
    /// </summary>
    [JsonPropertyName("lastMonthGrowth")]
    public string LastMonthGrowthSource { get; set; }

    /// <summary>
    /// 最近3个月涨跌
    /// </summary>
    [JsonPropertyName("lastThreeMonthsGrowth")]
    public string LastThreeMonthsGrowthSource { get; set; }

    /// <summary>
    /// 最近半年涨跌
    /// </summary>
    [JsonPropertyName("lastSixMonthsGrowth")]
    public string LastSixMonthsGrowthSource { get; set; }

    /// <summary>
    /// 最近一年涨跌
    /// </summary>
    [JsonPropertyName("lastYearGrowth")]
    public string LastYearGrowthSource { get; set; }

    /// <summary>
    /// 净值更新日期
    /// </summary>
    [JsonPropertyName("netWorthDate")]
    public string NetWorthDateSource { get; set; }

    /// <summary>
    /// 估算净值更新日期
    /// </summary>
    [JsonPropertyName("expectWorthDate")]
    public string ExpectWorthDateSource { get; set; }

    [JsonIgnore] public float ExpectGrowth => FloatParse(ExpectGrowthSource);

    [JsonIgnore] public float DayGrowth => FloatParse(DayGrowthSource);

    [JsonIgnore] public float LastMonthGrowth => FloatParse(LastMonthGrowthSource);

    [JsonIgnore] public float LastWeekGrowth => FloatParse(LastWeekGrowthSource);

    [JsonIgnore] public float LastThreeMonthsGrowth => FloatParse(LastThreeMonthsGrowthSource);

    [JsonIgnore] public float LastSixMonthsGrowth => FloatParse(LastSixMonthsGrowthSource);

    [JsonIgnore] public float LastYearGrowth => FloatParse(LastYearGrowthSource);

    [JsonIgnore] public string ExpectGrowthString => FormatGrowth(ExpectGrowth);

    [JsonIgnore] public DateTime NetWorthDate => FormatDate(NetWorthDateSource);

    [JsonIgnore] public DateTime ExpectWorthDate => FormatDate(ExpectWorthDateSource);

    //[JsonIgnore] public string LastWeekGrowthString => FormatGrowth(LastWeekGrowth);

    //[JsonIgnore] public string LastThreeMonthsGrowthString => FormatGrowth(LastThreeMonthsGrowth);

    //[JsonIgnore] public string LastSixMonthsGrowthString => FormatGrowth(LastSixMonthsGrowth);

    //[JsonIgnore] public string LastYearGrowthString => FormatGrowth(LastYearGrowth);

    private static string FormatGrowth(float growthValue) =>
        $"{(growthValue < 0 ? "－" : "＋")}{Math.Abs(growthValue):0.00}";

    private static float FloatParse(string parseValue) => float.TryParse(parseValue, out var floatValue) ? floatValue : 0;

    private static DateTime FormatDate(string dateString)
        => string.IsNullOrEmpty(dateString)
            ? DateTime.MinValue
            : DateTime.TryParse(dateString, out var dateValue)
                ? dateValue
                : DateTime.MinValue;
}