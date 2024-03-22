using System;
using SheepQQBot3.SDK.WebApi;

namespace SheepQQBot3.BotService;

public static partial class WebApiProcess
{
    private static WebServer _webServer;

    static WebApiProcess()
    {
        LastUpdateSteamMarketStatusDate = DateTime.Now;
    }

    public static void InitWebApi()
    {
        _webServer = new WebServer();

        //AddRoute_DGPDailyNote();
        AddRoute_UpdateSteamMarketStatus();

        _webServer.Start();
    }
}