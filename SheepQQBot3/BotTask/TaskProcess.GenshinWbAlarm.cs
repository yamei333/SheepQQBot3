using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.GenshinHelper;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotTask;

public static partial class TaskProcess
{
    /// <summary>
    /// 原神微博签到提醒
    /// </summary>
    public static async Task GenshinWbAlarmAsync()
    {
        AddTaskRunLog("原神微博签到提醒");
        while (true)
        {
            try
            {
                if (BotServer?.Connected == true)
                {
                    var dateNow = DateTime.Now;
                    if (_regGenshinWbAlarm.IsMatch(dateNow.ToConditionString(HolidayInfo)))
                    {
                        var wbSignDay = await GetWbSignDayAsync().ConfigureAwait(false);
                        // MEMO : 值为-1时取得失败
                        if (wbSignDay is >= 0 and <= 2 or >= 20 and <= 22)
                        {
                            if (wbSignDay >= 20)
                                wbSignDay = wbSignDay - 20;

                            Vm.SetConfigs?.Values
                                .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Group_GenshinHelper))
                                .ForEach(ToAction);

                            async void ToAction(SetConfig setConfig)
                            {
                                var targetId = setConfig.TargetId;
                                var sendMessage = $"{CQCode.AtAll()}[原神WB签到提醒]-WB签到第{wbSignDay + 1}天!";
                                await BotServer.SendGroupMessageAsync(targetId, sendMessage, Vm.SetConfigs).ConfigureAwait(false);
                                AddRunLog(new RunLog_GenshinDailyNoteAlarm(BotConfigTargetType.Group, targetId, sendMessage));

                                setConfig.GenshinHelperConfig?.GenshinResinAlarms?.ToValueList()
                                    .ForEach(SendGenshinWbAlarmMessageAction);

                                async void SendGenshinWbAlarmMessageAction(GenshinResinAlarm genshinResinAlarm)
                                {
                                    await SendGenshinWbAlarmMessageAsync(genshinResinAlarm, wbSignDay).ConfigureAwait(false);
                                }
                            }
                        }

                        CommonExtensions.SleepMinutes(30);
                    }
                    else
                    {
                        CommonExtensions.SleepMinutes(1);
                    }

                    async Task<int> GetWbSignDayAsync()
                    {
                        var httpResponse = await HttpExtensions.GetFromJsonAsync<GenshinGachaInfoResponse>(
                            "https://webstatic.mihoyo.com/hk4e/gacha_info/cn_gf01/gacha/list.json").ConfigureAwait(false);
                        if (httpResponse.Result != HttpResponseResult.Successed)
                            return -1;

                        var genshinGachaInfoResponse = httpResponse.Data;
                        var gachaInfo = genshinGachaInfoResponse?.Data.List.FirstOrDefault(each => each.GachaName == "角色活动");
                        if (gachaInfo != null)
                            return (int)(dateNow - gachaInfo.BeginTime).TotalDays;

                        return -1;
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

    /// <summary>
    /// 发送原神微博签到提醒消息
    /// </summary>
    public static async Task SendGenshinWbAlarmMessageAsync(GenshinResinAlarm genshinResinAlarm, int wbSignDay)
    {
        var barkKey = genshinResinAlarm.BarkKey;
        if (!string.IsNullOrEmpty(barkKey))
        {
            await PushExtensions.PushBarkMessageAsync(
                barkKey,
                $"[原神WB签到提醒]-WB签到第{wbSignDay + 1}天!",
                PushExtensions.TITLE).ConfigureAwait(false);
        }
    }
}