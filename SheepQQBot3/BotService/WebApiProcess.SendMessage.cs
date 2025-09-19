using CommonLibrary;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Model.WebApi;
using WatsonWebserver.Core;

namespace SheepQQBot3.BotService;

public static partial class WebApiProcess
{
    private static void AddRoute_SendMessage()
    {
        // MEMO : 外部调用API发送消息
        _webServer.AddStaticRoute(HttpMethod.POST, "/SendMessage/", async context =>
        {
            var jsonText = context.Request.DataAsString;
            try
            {
                var apiSendMessage = jsonText.JsonDeserialize<WebApi_SendMessage>();
                if (apiSendMessage?.SheepQQBot3 == TOKEN)
                {
                    await PublicVar.BotClient.SendMessageAsync(
                        apiSendMessage.IsGroup ? MessageTargetType.Group : MessageTargetType.Private,
                        apiSendMessage.TargetId,
                        apiSendMessage.Message).ConfigureAwait(false);
                    //LogExtensions.AddRunLog(new RunLog_SystemInfo("[Steam市场监控]状态已刷新"));
                    const string result = @"{Result: 200}";
                    await context.Response.Send(result).ConfigureAwait(false);
                }
                else
                {
                    await context.Response.Send(string.Empty).ConfigureAwait(false);
                }
            }
            catch
            {
                await context.Response.Send(string.Empty).ConfigureAwait(false);
            }
        });
    }
}