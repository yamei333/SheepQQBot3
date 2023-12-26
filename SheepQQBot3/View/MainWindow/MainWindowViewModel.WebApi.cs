using System.Text.Json;
using SheepQQBot3.Model;
using SheepQQBot3.SDK.WebApi;
using WatsonWebserver.Core;

namespace SheepQQBot3.View;

partial class MainWindowViewModel
{
    private static int _postNum = 0;

    private static void InitWebApi()
    {
        var webServer = new WebServer();
        webServer.AddStaticRoute(HttpMethod.POST, "/DGPDailyNote/", async context =>
        {
            var jsonText = context.Request.DataAsString;
            var dgpDailyNote = JsonSerializer.Deserialize<DGPDailyNote>(jsonText);
            PublicVar.GenshinDailyNote[_postNum == 0 ? 252961222 : 173629299] = dgpDailyNote;
            _postNum = _postNum == 0 ? 1 : 0;
            if (_postNum == 0)
                PublicVar.DGPProcessOK = true;
        });
        webServer.Start();
    }
}