using CommonLibrary;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.JsonCard;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SheepQQBot3.Extensions;

public static class CQCode
{
    public static string At(long targetId)
        => $"[CQ:at,qq={targetId}]";

    public static string AtAll()
        => $"[CQ:at,qq=all]";

    public static string Image(string filePath)
        => $"[CQ:image,file={filePath}]";

    public static string Reply(long targetId, int messageId)
        => $"[CQ:reply,qq={targetId},id={messageId}]";

    /// <summary>
    /// Json卡片消息, 暂时不可用
    /// </summary>
    [Obsolete]
    public static async Task<string> JsonCard_StructMsgAsync(
        string title, string content, string tag,
        string url, string previewIcon, string tagIcon = "")
    {
        var jsonText = JsonSerializer.Serialize(
            new JsonCard_StructMsg(PublicVar.BotId, title, content, tag, url, previewIcon, tagIcon),
            JsonExtensions.DefaultJsonOptions);
        var signedJsonText = await HttpExtensions.HttpClient
            .GetStringAsync($"http://ovoa.cc/api/VIPArk.php?jsonStr={jsonText}")
            .ConfigureAwait(false);
        return $"[CQ:json,data={signedJsonText}]";
    }

    /// <summary>
    /// Json卡片消息, 暂时不可用
    /// </summary>
    [Obsolete]
    public static Task<string> JsonCard_StructMsgAsync(string title, string content, string url, string previewIcon)
        => JsonCard_StructMsgAsync(title, content, PublicVar.BOT_NAME, url, previewIcon, QQExtensions.GetQQImageUrl(PublicVar.BotId));
}