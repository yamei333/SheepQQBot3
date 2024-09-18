using SheepQQBot3.SDK.WebApi;
using System;

namespace SheepQQBot3.BotService;

public static partial class WebApiProcess
{
    private static WebServer _webServer;
    private const string TOKEN = "yamei";

    static WebApiProcess()
    {
        LastUpdateSteamMarketStatusDate = DateTime.Now;
    }

    public static void InitWebApi()
    {
        _webServer = new WebServer();

        AddRoute_UpdateSteamMarketStatus();
        AddRoute_SendMessage();

        _webServer.Start();
    }
}