using System;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model;

public class GenshinEventResponse
{
    [JsonPropertyName("data")]
    public GenshinEventData Data { get; set; }
}

public class GenshinEventData
{
    [JsonPropertyName("list")]
    public GenshinEventList[] List { get; set; }
}

public class GenshinEventList
{
    [JsonPropertyName("list")]
    public GenshinEvent[] List { get; set; }

    [JsonPropertyName("type_id")]
    public int TypeId { get; set; }

    [JsonPropertyName("type_label")]
    public string TypeLabel { get; set; }
}

public class GenshinEvent
{
    [JsonPropertyName("ann_id")]
    public int EventId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("subtitle")]
    public string SubTitle { get; set; }

    [JsonPropertyName("banner")]
    public string Banner { get; set; }

    [JsonPropertyName("type_label")]
    public string TypeLabel { get; set; }

    [JsonPropertyName("tag_label")]
    public string TagLabel { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [JsonPropertyName("start_time")]
    public string BeginTimeString { get; set; }

    [JsonIgnore]
    public DateTime BeginTime => Convert.ToDateTime(BeginTimeString);

    public int GetDaysRemain(DateTime dateNow)
    {
        var totalHours = (EndTime - dateNow).TotalHours;
        if (totalHours >= 0)
            return (int)Math.Ceiling(totalHours / 24);

        return -1;
    }

    /// <summary>
    /// 结束时间
    /// </summary>
    [JsonPropertyName("end_time")]
    public string EndTimeString { get; set; }

    [JsonIgnore]
    public DateTime EndTime => Convert.ToDateTime(EndTimeString);
}