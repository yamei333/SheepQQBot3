using System;
using System.Text.Json.Serialization;
using Yamei.Common;

namespace SheepQQBot3.Model.JsonCard
{
    public class JsonCard_TianxuanShare
    {
        [JsonPropertyName("app")]
        public string App { get; set; }

        [JsonPropertyName("meta")]
        public TianxuanShare_Meta Meta { get; set; }

        [JsonPropertyName("ver")]
        public string Ver { get; set; }

        [JsonPropertyName("view")]
        public string View { get; set; }

        [JsonPropertyName("config")]
        public TianxuanShare_Meta_Config Config { get; set; }

        public JsonCard_TianxuanShare(
            long targetId,
            string title,
            string content,
            string tag,
            string url,
            string previewImage,
            string tagIcon)
        {
            App = "com.tencent.tianxuan.share";
            Ver = "0.0.0.1";
            View = "news";
            Meta = new TianxuanShare_Meta
            {
                News = new TianxuanShare_Meta_News
                {
                    DateTime = DateTime.Now.ToTimeStamp(),
                    Title = title,
                    Desc = content,
                    Tag = tag,
                    JumpUrl = url,
                    PreviewImage = previewImage,
                    TagIcon = tagIcon,
                    Uin = targetId,
                },
            };
            Config = new TianxuanShare_Meta_Config();
        }
    }

    public class TianxuanShare_Meta
    {
        [JsonPropertyName("news")]
        public TianxuanShare_Meta_News News { get; set; }
    }

    public class TianxuanShare_Meta_News
    {
        [JsonPropertyName("ctime")]
        public long DateTime { get; set; }

        [JsonPropertyName("desc")]
        public string Desc { get; set; }

        [JsonPropertyName("jumpUrl")]
        public string JumpUrl { get; set; }

        [JsonPropertyName("preview")]
        public string PreviewImage { get; set; }

        [JsonPropertyName("tagIcon")]
        public string TagIcon { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("uin")]
        public long Uin { get; set; }
    }

    public class TianxuanShare_Meta_Config
    {
    }
}