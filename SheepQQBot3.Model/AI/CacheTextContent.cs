using System.Text.Json.Serialization;

#nullable enable

namespace OpenRouter.NET.Models;

public class CacheTextContent : ContentPart
{
    [JsonPropertyName("type")]
    public override string Type => "text";

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("cache_control")]
    public string? CacheControl { get; set; }

    public CacheTextContent(string text)
    {
        Text = text;
        CacheControl = "ephemeral";
    }
}