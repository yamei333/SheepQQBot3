using CommonLibrary;
using SheepQQBot3.Model.Model;
using System;
using WatsonWebserver.Core;

namespace SheepQQBot3.BotService;

public static partial class WebApiProcess
{
    /// <summary>
    /// steam市场状态最终更新时间
    /// </summary>
    public static DateTime LastUpdateSteamMarketStatusDate { get; set; }

    private static void AddRoute_UpdateSteamMarketStatus()
    {
        // MEMO : Steam市场状态上报时使用的POST
        _webServer.AddStaticRoute(HttpMethod.POST, "/UpdateSteamMarketStatus/", async context =>
        {
            var jsonText = context.Request.DataAsString;
            var steamMarketStatus = JsonExtensions.Deserialize<SteamMarketStatus>(jsonText);
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