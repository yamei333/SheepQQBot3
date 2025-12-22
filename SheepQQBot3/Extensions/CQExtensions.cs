using CommonLibrary;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.JsonCard;
using System.Threading.Tasks;

namespace SheepQQBot3.Extensions;

public static class CQExtensions
{
    /// <summary>
    /// Json卡片消息(天选转发)
    /// </summary>
    public static async Task<string> JsonCard_TianxuanShareAsync(
        string title, string content, string tag,
        string url, string previewImage, string tagIcon = null)
        => await HttpExtensions.GetSignedArkAsync(
            PublicVar.GlobalBotClient.GetCookiesAsync,
            new JsonCard_TianxuanShare(PublicVar.BotId, title, content, tag, url, previewImage,
                tagIcon ?? QQExtensions.GetQQHeadImageUrl(PublicVar.BotId))).ConfigureAwait(false);

    /// <summary>
    /// Json卡片消息(天选转发)
    /// </summary>
    public static Task<string> JsonCard_TianxuanShareAsync(string title, string content, string url, string previewImage)
        => JsonCard_TianxuanShareAsync(title, content, PublicVar.BOT_NAME, url, previewImage, QQExtensions.GetQQHeadImageUrl(PublicVar.BotId));

    public static string CQCodeToMessageText(string cqCode)
    {
        return cqCode switch
        {
            "forward" => "[转发消息]",
            "video" => "[视频]",
            "record" => "[语音]",
            "reply" => "[引用消息]",
            "file" => "[文件]",
            "face" => "[表情]",
            "json" => "[APP卡片消息]",
            _ => string.Empty,
        };
    }
}