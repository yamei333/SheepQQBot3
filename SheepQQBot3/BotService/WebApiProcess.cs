using System;
using SheepQQBot3.SDK.WebApi;

namespace SheepQQBot3.BotService;

public static partial class WebApiProcess
{
    private const string TOKEN = "yamei";
    private const string USERAGENT_TOKEN = "SheepQQBot3_token_webserver_yamei";
    private static WebServer _webServer;

    static WebApiProcess()
    {
        LastUpdateSteamMarketStatusDate = DateTime.Now;
    }

    public static void InitWebApi()
    {
        _webServer = new WebServer();

        // AddRoute_UpdateSteamMarketStatus();
        AddRoute_SendMessage();

        _webServer.Start();
    }
}