using System;
using System.Threading.Tasks;
using CommonLibrary;
using Masuit.Tools.DateTimeExt;
using SheepQQBot3.BotService;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotTask;

public static partial class TaskProcess
{
    /// <summary>
    /// Steam市场监控状态
    /// </summary>
    public static async Task SteamMarketWatchAsync()
    {
        if (!Vm.IsBarkUsed)
        {
            AddRunLog(new RunLog_SystemWarning("Steam市场监控状态 启动失败! [由于未配置Bark]"));
            return;
        }

        AddTaskRunLog("Steam市场监控状态");
        while (true)
        {
            try
            {
                if (BotServer?.Connected == true)
                {
                    var dateNowSeconds = DateTime.Now.GetTotalSeconds();
                    var lastUpdateSteamMarketStatusSeconds = WebApiProcess.LastUpdateSteamMarketStatusDate.GetTotalSeconds();
                    if (dateNowSeconds - lastUpdateSteamMarketStatusSeconds > 600)
                    {
                        const string errorMessage = "[Steam市场监控]出现问题!请检查!";
                        //await Api.SendPrivateMessageAsync(AdminId, errorMessage).ConfigureAwait(false);
                        await PushExtensions.PushBarkMessageAsync(errorMessage, "系统问题").ConfigureAwait(false);
                        AddRunLog(new RunLog_SystemWarning(errorMessage));
                        CommonExtensions.SleepMinutes(60);
                    }
                    else
                    {
                        CommonExtensions.SleepMinutes(1);
                    }
                }
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(e);
                CommonExtensions.SleepSeconds(30);
            }
        }
    }
}