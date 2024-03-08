using System.Text.Json.Serialization;
using Yamei.Common;

namespace SheepQQBot3.Model;

public class BWH_LosAngeles
{
    [JsonPropertyName("plan_monthly_data")]
    public long MonthLimit { get; set; }

    [JsonPropertyName("data_counter")]
    public long Counter { get; set; }

    [JsonPropertyName("data_next_reset")]
    public int ResetTimeStamp { get; set; }

    [JsonIgnore]
    public int ResetDayOfMonth => ResetTimeStamp.ToDateTime().Day;

    [JsonPropertyName("ip_addresses")]
    public string[] IPAddresses { get; set; }

    [JsonPropertyName("hostname")]
    public string HostName { get; set; }
}