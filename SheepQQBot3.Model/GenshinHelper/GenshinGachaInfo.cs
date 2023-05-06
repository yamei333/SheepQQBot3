using System;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.GenshinHelper;

public class GenshinGachaInfoResponse
{
    [JsonPropertyName("data")]
    public GenshinGachaInfoData Data { get; set; }
}

public class GenshinGachaInfoData
{
    [JsonPropertyName("list")]
    public GenshinGachaInfo[] List { get; set; }
}

public class GenshinGachaInfo
{
    /// <summary>
    /// 开始时间
    /// </summary>
    [JsonPropertyName("begin_time")]
    public string BeginTimeString { get; set; }

    [JsonIgnore]
    public DateTime BeginTime => Convert.ToDateTime(BeginTimeString);

    /// <summary>
    /// 结束时间
    /// </summary>
    [JsonPropertyName("end_time")]
    public string EndTimeString { get; set; }

    [JsonIgnore]
    public DateTime EndTime => Convert.ToDateTime(EndTimeString);

    /// <summary>
    /// 卡池ID
    /// </summary>
    [JsonPropertyName("gacha_id")]
    public string GachaId { get; set; }

    /// <summary>
    /// 卡池名称
    /// </summary>
    [JsonPropertyName("gacha_name")]
    public string GachaName { get; set; }

    /// <summary>
    /// 卡池名称种类
    /// </summary>
    [JsonPropertyName("gacha_type")]
    public int GachaType { get; set; }
}