using System;
using System.Text.Json;
using SheepQQBot3.Model.Model;
using WatsonWebserver.Core;

namespace SheepQQBot3.BotService;

public static partial class WebApiProcess
{
    public static DateTime LastUpdateSteamMarketStatusDate { get; set; }

    private static void AddRoute_UpdateSteamMarketStatus()
    {
        // MEMO : Steam市场状态上报时使用的POST
        _webServer.AddStaticRoute(HttpMethod.POST, "/UpdateSteamMarketStatus/", async context =>
        {
            var jsonText = context.Request.DataAsString;
            var steamMarketStatus = JsonSerializer.Deserialize<SteamMarketStatus>(jsonText);
            if (steamMarketStatus?.SheepQQBot3 == "yamei")
            {
                LastUpdateSteamMarketStatusDate = DateTime.Now;
                //LogExtensions.AddRunLog(new RunLog_SystemInfo("[Steam市场监控]状态已刷新"));
                const string result = @"{Result: 400}";
                await context.Response.Send(result);
            }
            else
            {
                await context.Response.Send(string.Empty);
            }
        });
    }
}