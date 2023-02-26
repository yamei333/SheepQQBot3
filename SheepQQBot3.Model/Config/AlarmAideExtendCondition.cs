using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Config
{
    public class AlarmAideExtendCondition
    {
        [JsonPropertyName("dayofmonthoffset")]
        public int? DayOfMonthOffset { get; set; }
    }
}