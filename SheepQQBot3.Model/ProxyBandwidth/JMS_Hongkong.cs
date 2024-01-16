using System.Text.Json.Serialization;

namespace SheepQQBot3.Model;

public class JMS_Hongkong
{
    [JsonPropertyName("monthly_bw_limit_b")]
    public long MonthLimit { get; set; }

    [JsonPropertyName("bw_counter_b")]
    public long Counter { get; set; }

    [JsonPropertyName("bw_reset_day_of_month")]
    public short ResetDayOfMonth { get; set; }
}