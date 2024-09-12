using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.JsonCard;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SheepQQBot3.Extensions;

public static class CQCode
{
    private static Regex _regCQImage = RegexGenerator.CQImage();
    private static Regex _regCQImageUrl = RegexGenerator.CQImageUrl();

    public static string At(long targetId)
        => $"[CQ:at,qq={targetId}]";

    public static string AtAll()
        => $"[CQ:at,qq=all]";

    public static string Image(string filePath)
        => $"[CQ:image,file={filePath}]";

    public static string Reply(long targetId, int messageId)
        => $"[CQ:reply,qq={targetId},id={messageId}]";

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
        var cqImageUrl = _regCQImageUrl.Match(message).Value;
        return cqImageUrl.Replace("&amp;","&").Replace("multimedia.nt.qq.com.cn", "gchat.qpic.cn");
    }

    /// <summary>
    /// Json卡片消息(天选转发)
    /// </summary>
    public static async Task<string> JsonCard_TianxuanShareAsync(
        string title, string content, string tag,
        string url, string previewImage, string tagIcon = null)
        => await HttpExtensions.GetSignedArkAsync(
            PublicVar.BotServer.GetCookiesAsync,
            new JsonCard_TianxuanShare(PublicVar.BotId, title, content, tag, url, previewImage,
                tagIcon ?? QQExtensions.GetQQImageUrl(PublicVar.BotId))).ConfigureAwait(false);

    /// <summary>
    /// Json卡片消息(天选转发)
    /// </summary>
    public static Task<string> JsonCard_TianxuanShareAsync(string title, string content, string url, string previewImage)
        => JsonCard_TianxuanShareAsync(title, content, PublicVar.BOT_NAME, url, previewImage, QQExtensions.GetQQImageUrl(PublicVar.BotId));
}