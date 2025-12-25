using Masuit.Tools;
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace SheepQQBot3.Model.Extension;

public static class CQCode
{
    private static readonly Regex _regCQImageFileUrl = RegexGenerator.CQImageFileUrl();

    public static string At(string targetId)
        => $"[CQ:at,qq={targetId}]";

    public static string AtAll()
        => $"[CQ:at,qq=all]";

    public static string Image(string filePath, string fileName = "", string summary = "")
    {
        var url = filePath.StartsWith("http", StringComparison.CurrentCultureIgnoreCase) || filePath.StartsWith("file:///", StringComparison.CurrentCultureIgnoreCase)
            ? filePath : new Uri(filePath).AbsoluteUri;
        return $"[CQ:image,url={url}"
            + $"{(!fileName.IsNullOrEmpty() ? $",file={fileName}" : string.Empty)}"
            + $"{(!summary.IsNullOrEmpty() ? $",summary={summary}" : string.Empty)}]";
    }

    public static string Reply(string targetId, string messageId)
        => $"[CQ:reply,qq={targetId},id={messageId}]";

    public static string Json(string signedJson)
        => $"[CQ:json,data={signedJson}]";

    public static string MarkDown(string content)
        => $"[CQ:markdown,content={content}]";

    public static string Forward(string messageId)
        => $"[CQ:forward,id={messageId}]";

    /// <summary>
    /// 自定义音乐卡片
    /// </summary>
    /// <param name="url">目标地址</param>
    /// <param name="audio">音乐地址</param>
    /// <param name="title">标题</param>
    /// <param name="image">图片</param>
    /// <param name="singer">演唱者</param>
    /// <returns></returns>
    public static string CustomMusic(string url, string audio, string title, string image, string singer)
        => $"[CQ:music,type=custom,url={url},audio={audio},title={title},image={image},singer={singer}]";

    /// <summary>
    /// 将接收到的CQImage段替换为可发送的CQImage段
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static string ReplaceCQImage(string message)
    {
        message = _regCQImageFileUrl.Replace(message, match =>
        {
            // MEMO : 此处不需要Decode, Decode在真正使用时才转换
            var file = match.Groups["file"];
            var url = match.Groups["url"];
            return Image(url.Success ? url.Value : string.Empty,
                file.Success ? file.Value : string.Empty);
        });

        return message;
    }

    public static string GetImageUrl(string message)
    {
        var match = _regCQImageFileUrl.Match(message);
        return match.Success ? WebUtility.HtmlDecode(match.Value) : string.Empty;
    }
}