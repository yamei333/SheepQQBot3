using System;
using System.Text.Json.Serialization;
using Yamei.Common;

namespace SheepQQBot3.Model.JsonCard;

public class JsonCard_StructMsg
{
    [JsonPropertyName("app")]
    public string App { get; set; }

    [JsonPropertyName("meta")]
    public StructMsg_Meta Meta { get; set; }

    [JsonPropertyName("ver")]
    public string Ver { get; set; }

    [JsonPropertyName("view")]
    public string View { get; set; }

    public JsonCard_StructMsg(
        long targetId,
        string title,
        string content,
        string tag,
        string url,
        string previewImage,
        string tagIcon)
    {
        App = "com.tencent.structmsg";
        Ver = "0.0.0.1";
        View = "news";
        Meta = new StructMsg_Meta
        {
            News = new StructMsg_Meta_News
            {
                AppType = 1,
                AppId = 100951776,
                DateTime = DateTime.Now.ToTimeStamp(),
                Title = title,
                Desc = content,
                Tag = tag,
                JumpUrl = url,
                PreviewImage = previewImage,
                SourceIcon = tagIcon,
                Uin = targetId,
            },
        };
    }
}

public class StructMsg_Meta
{
    [JsonPropertyName("news")]
    public StructMsg_Meta_News News { get; set; }
}

public class StructMsg_Meta_News
{
    [JsonPropertyName("app_type")]
    public int AppType { get; set; }

    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("ctime")]
    public long DateTime { get; set; }

    [JsonPropertyName("desc")]
    public string Desc { get; set; }

    [JsonPropertyName("jumpUrl")]
    public string JumpUrl { get; set; }

    [JsonPropertyName("preview")]
    public string PreviewImage { get; set; }

    [JsonPropertyName("source_icon")]
    public string SourceIcon { get; set; }

    [JsonPropertyName("tag")]
    public string Tag { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("uin")]
    public long Uin { get; set; }
}