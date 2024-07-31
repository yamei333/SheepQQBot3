using CommonLibrary;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.JsonCard;
using System;
using System.Net;
using System.Net.Http.Json;
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
        //var cookiesJson = await PublicVar.BotServer.GetCookiesAsync().ConfigureAwait(false);
        //var cookies = JsonExtensions.Deserialize<NTQQCookies>(cookiesJson);

        WebClient webClient = new WebClient();
        webClient.BaseAddress = "https://ssl.ptlogin2.qq.com/jump?clientuin=205552607&clientkey=5687c49d6130d7fe32fb3bce0a4da5df7e2ff2c7b6161e1e4ff41f55de22845cc6e9820bdd671edb37d41a8617c9ebe7&u1=https%3A%2F%2Fuser.qzone.qq.com%2F205552607%2Finfocenter";

        var res = await HttpExtensions.HttpClient.GetAsync("https://ssl.ptlogin2.qq.com/jump?clientuin=205552607&clientkey=5687c49d6130d7fe32fb3bce0a4da5df7e2ff2c7b6161e1e4ff41f55de22845cc6e9820bdd671edb37d41a8617c9ebe7&u1=https%3A%2F%2Fuser.qzone.qq.com%2F205552607%2Finfocenter");
        var getRes = await HttpExtensions.HttpClient.GetAsync("https://user.qzone.qq.com/205552607");
        ;

        var jsonText = JsonSerializer.Serialize(
            new JsonCard_TianxuanShare(PublicVar.BotId, title, content, tag, url, previewIcon, tagIcon),
            JsonExtensions.DefaultJsonOptions);
        var jsonCardRequestBody = new JsonCard(jsonText);

        var response = await HttpExtensions.HttpClient
            .PostAsJsonAsync($"https://act.qzone.qq.com/v2/vip/tx/trpc/ark-share/GenSignedArk?g_tk=347803664", jsonCardRequestBody)
            .ConfigureAwait(false);
        var jsonCardResponse = await response.Content.ReadFromJsonAsync<JsonCardResponse>();
        if (jsonCardResponse.Code != 0)
            return $"ark签名失败, 原因是[{jsonCardResponse.Data.Message}]";

        var signedJsonText = jsonCardResponse.Data.SignedArk;
        return $"[CQ:json,data={signedJsonText}]";
    }

    /// <summary>
    /// Json卡片消息, 暂时不可用
    /// </summary>
    [Obsolete]
    public static Task<string> JsonCard_StructMsgAsync(string title, string content, string url, string previewIcon)
        => JsonCard_StructMsgAsync(title, content, PublicVar.BOT_NAME, url, previewIcon, QQExtensions.GetQQImageUrl(PublicVar.BotId));
}