using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Setu;

public class SetuData_NekosiaCat
{
    [JsonPropertyName("status")]
    public int Code { get; set; }

    [JsonPropertyName("pics")]
    public string[] Urls { get; set; }

    [JsonPropertyName("image")]
    public SetuData_NekosiaCat_Image Image { get; set; }

    [JsonPropertyName("source")]
    public SetuData_NekosiaCat_Image_Source Source { get; set; }

    [JsonPropertyName("attribution")]
    public SetuData_NekosiaCat_Image_Attribution Attribution { get; set; }
}

public class SetuData_NekosiaCat_Image
{
    [JsonPropertyName("original")]
    public SetuData_NekosiaCat_Image_Info Original { get; set; }

    [JsonPropertyName("compressed")]
    public SetuData_NekosiaCat_Image_Info Compressed { get; set; }
}

public class SetuData_NekosiaCat_Image_Info
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("extension")]
    public string Extension { get; set; }
}

public class SetuData_NekosiaCat_Image_Source
{
    [JsonPropertyName("Direct")]
    public string Direct { get; set; }
}

public class SetuData_NekosiaCat_Image_Attribution
{
    [JsonPropertyName("artist")]
    public SetuData_NekosiaCat_Image_Artist Artist { get; set; }
}

public class SetuData_NekosiaCat_Image_Artist
{
    [JsonPropertyName("username")]
    public string UserName { get; set; }
}