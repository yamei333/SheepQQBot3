using Masuit.Tools;
using System;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Fund;

public class FundDataResponse
{
    /// <summary>
    /// 基金数据
    /// </summary>
    [JsonPropertyName("data")]
    public FundData[] FundDatas { get; set; }
    
    /// <summary>
    /// 错误代码
    /// </summary>
    [JsonPropertyName("ErrCode")]
    public int ErrorCode { get; set; }
}

public class FundData
{
    /// <summary>
    /// 基金编号
    /// </summary>
    [JsonPropertyName("FCODE")]
    public string Code { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    [JsonPropertyName("GZTIME")]
    public string UpdateDateSource { get; set; }
    
    /// <summary>
    /// 基金名称
    /// </summary>
    [JsonPropertyName("SHORTNAME")]
    public string Name { get; set; }

    /// <summary>
    /// 当前净值(昨日)
    /// </summary>
    [JsonPropertyName("NAV")]
    public float YesterdayValue { get; set; }

    /// <summary>
    /// 净值估算(今日)
    /// </summary>
    [JsonPropertyName("GSZ")]
    public float? ExpectValue { get; set; }

    /// <summary>
    /// 净值估算(涨跌幅)
    /// </summary>
    [JsonPropertyName("GSZZL")]
    public float? ExpectGrowth { get; set; }

    /// <summary>
    /// 更新日期
    /// </summary>
    [JsonIgnore]
    public DateTime UpdateDate => FormatDate(UpdateDateSource);

    /// <summary>
    /// 净值估算(涨跌幅)(格式化)
    /// </summary>
    [JsonIgnore]
    public string ExpectGrowthString => FormatGrowth(ExpectGrowth);

    // /// <summary>
    // /// 净值估算(涨跌幅)(彩色格式化)
    // /// </summary>
    // [JsonIgnore]
    // public string ExpectGrowthColorString => FormatGrowth(ExpectGrowth, "\ud83d\udd3a", "\ud83d\udfe2");

    private static string FormatGrowth(float? growthValue, string plusString = null, string minusString = null) 
        => growthValue == null
            ? "无数据"
            : $"{(growthValue.GetValueOrDefault() < 0 ? plusString ?? "－" : minusString ?? "＋")}{Math.Abs(growthValue.GetValueOrDefault()):0.00}";
    
    // private static float FloatParse(string parseValue) => float.TryParse(parseValue, out var floatValue) ? floatValue : 0;
    
    private static DateTime FormatDate(string dateString)
        => dateString.IsNullOrEmpty()
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