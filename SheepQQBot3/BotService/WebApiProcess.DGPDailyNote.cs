using System.Text.Json;
using SheepQQBot3.Model;
using WatsonWebserver.Core;

namespace SheepQQBot3.BotService;

public static partial class WebApiProcess
{
    private static void AddRoute_DGPDailyNote()
    {
        // MEMO : DGP刷新每日便笺后Webhook调用时使用的Post
        _webServer.AddStaticRoute(HttpMethod.POST, "/DGPDailyNote/", async context =>
        {
            var jsonText = context.Request.DataAsString;
            var dgpDailyNote = JsonSerializer.Deserialize<DGPDailyNote>(jsonText);
            PublicVar.GenshinDailyNote[252961222] = dgpDailyNote;
            PublicVar.DGPProcessOK = true;
        });
    }
}