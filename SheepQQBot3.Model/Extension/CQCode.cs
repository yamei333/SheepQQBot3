using Masuit.Tools;
using System.Text.RegularExpressions;

namespace SheepQQBot3.Model.Extension;

public static class CQCode
{
    private static Regex _regCQImage = RegexGenerator.CQImage();
    private static Regex _regCQImageUrl = RegexGenerator.CQImageUrl();
    private static Regex _regCQFormattedImageUrl = RegexGenerator.CQFormattedImageUrl();

    public static string At(long targetId)
        => $"[CQ:at,qq={targetId}]";

    public static string AtAll()
        => $"[CQ:at,qq=all]";

    public static string Image(string filePath)
        => $"[CQ:image,file={filePath}]";

    public static string Reply(long targetId, int messageId)
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
    /// 将收到的CQImage段替换为可发送的CQImage段
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static string ReplaceCQImage(string message)
    {
        var matches = _regCQImage.Matches(message);
        matches.ForEach(match =>
        {
            var cqImageMessage = match.Value;
            message = message.Replace(cqImageMessage, Image(GetImageUrl(cqImageMessage)));
        });

        return message;
    }

    public static string GetImageUrl(string message)
    {
        var formattedUrlMatch = _regCQFormattedImageUrl.Match(message);
        if (formattedUrlMatch.Success)
            return formattedUrlMatch.Value;

        var cqImageUrl = _regCQImageUrl.Match(message).Value;
        return cqImageUrl.Replace("&amp;", "&").Replace("multimedia.nt.qq.com.cn", "gchat.qpic.cn");
    }
}