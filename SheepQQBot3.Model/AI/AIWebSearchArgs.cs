using System.ComponentModel;
using System.Text.Json.Serialization;
using Tavily;

namespace SheepQQBot3.Model.AI;

public class AIWebSearchArgs
{
    [Description("搜索内容的关键词")]
    [JsonPropertyName("query")]
    public required string Query { get; init; }

    [Description("搜索分类：General(通用事实/技术/百科，默认)、News(突发时事热点新闻)、Finance(金融财经)")]
    [JsonPropertyName("topic")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required CreateSearchRequestTopic Topic { get; init; } = CreateSearchRequestTopic.General;

    [Description("时间范围过滤：Day(近24小时)、Week(近7天)、Month(近1个月)、Year(近1年)。查当天的内容必须指定Day，查即时新闻强烈建议指定 Day 或 Week")]
    [JsonPropertyName("time_range")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CreateSearchRequestTimeRange? TimeRange { get; init; }
}