using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model;

public class SauceNaoRequest
{
    [JsonPropertyName("header")]
    public SauceNaoHeader Header { get; set; }

    [JsonPropertyName("results")]
    public List<SauceNaoResult> Results { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}

public class SauceNaoHeader
{
    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    [JsonPropertyName("account_type")]
    public int AccountType { get; set; }

    [JsonPropertyName("short_limit")]
    public string ShortLimit { get; set; }

    [JsonPropertyName("long_limit")]
    public string LongLimit { get; set; }

    /// <summary>
    /// 当日剩余请求数
    /// </summary>
    [JsonPropertyName("long_remaining")]
    public int LongRemaining { get; set; }

    /// <summary>
    /// 短时间剩余请求数
    /// </summary>
    [JsonPropertyName("short_remaining")]
    public int ShortRemaining { get; set; }

    /// <summary>
    /// 账户状态
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("results_requested")]
    public int ResultsRequested { get; set; }

    [JsonPropertyName("search_depth")]
    public string SearchDepth { get; set; }

    [JsonPropertyName("minimum_similarity")]
    public double MinimumSimilarity { get; set; }

    [JsonPropertyName("query_image_display")]
    public string QueryImageDisplay { get; set; }

    [JsonPropertyName("query_image")]
    public string QueryImage { get; set; }

    /// <summary>
    /// 返回结果总数
    /// </summary>
    [JsonPropertyName("results_returned")]
    public int ResultCount { get; set; }
}

public class SauceNaoResult
{
    [JsonPropertyName("header")]
    public SauceNaoResultHeader Header { get; set; }

    [JsonPropertyName("data")]
    public SauceNaoResultData Data { get; set; }
}

public class SauceNaoResultHeader
{
    [JsonPropertyName("similarity")]
    public double Similarity { get; set; }

    [JsonPropertyName("thumbnail")]
    public string SmallImageUrl { get; set; }

    [JsonPropertyName("index_id")]
    public int IndexId { get; set; }

    [JsonPropertyName("index_name")]
    public string IndexName { get; set; }

    [JsonPropertyName("hidden")]
    public int Hidden { get; set; }
}

public class SauceNaoResultData
{
    [JsonPropertyName("ext_urls")]
    public List<string> ExtUrls { get; set; }
}