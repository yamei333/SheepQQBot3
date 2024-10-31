using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.JsonCard;

/// <summary>
/// QQJson卡片Response
/// </summary>
public class JsonCardResponse()
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("data")]
    public JsonCardResponse_Data Data { get; set; }
}

public class JsonCardResponse_Data
{
    [JsonPropertyName("signed_ark")]
    public string SignedArk { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}