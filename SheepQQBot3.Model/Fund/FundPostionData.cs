using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Fund;

/// <summary>
/// 基金持仓数据
/// </summary>
public class FundPostionData
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("data")]
    public FundStockData Data { get; set; }
}

/// <summary>
/// 简单基金信息Json数据类型
/// </summary>
public class FundStockData
{
    /// <summary>
    /// 持仓更新日期
    /// </summary>
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    /// <summary>
    /// 基金持仓
    /// </summary>
    [JsonPropertyName("stockList")]
    public List<List<string>> StockList { get; set; }
}