using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View;

public static partial class TaskProcess
{
    /// <summary>
    /// 自定义提醒
    /// </summary>
    public static void CustomAlarm()
    {
        AddTaskRunLog("自定义提醒");
        // MEMO : 清理过期提醒
        ClearHistoryData();

        while (true)
        {
            try
            {
                if (Api.Connected)
                {
                    var dateNow = DateTime.Now;
                    var userConfigs = PublicVar.BotConfig.UserConfigs;
                    var customAlarms = PublicVar.BotConfig.CustomAlarms;
                    if (customAlarms == null)
                        return;

                    var taskList = new List<Task>();
                    customAlarms.Values.ForEach(each =>
                    {
                        var task = new Task(() => ToAction(each));
                        taskList.Add(task);
                        task.Start();
                    });
                    Task.WaitAll(taskList.ToArray());

                    async void ToAction(CustomAlarm customAlarm)
                    {
                        // 3分钟内才提醒, 超时则已经超过有效期了
                        if ((dateNow - customAlarm.AlarmDate).TotalSeconds is < 0 or > 180)
                            return;

                        var alarmMessage = customAlarm.AlarmMessage.ToCqCode().Result;
                        var targetId = customAlarm.TargetId;
                        if (customAlarm.IsBark)
                        {
                            await PushExtensions.PushBarkMessageAsync(
                                userConfigs[targetId][UserConfigType.BarkKey], alarmMessage, PushExtensions.TITLE)
                                .ConfigureAwait(false);
                        }
                        else
                        {
                            if (customAlarm.IsGroup)
                            {
                                await Api.SendGroupMessageAsync(customAlarm.GroupId.GetValueOrDefault(),
                                    $"{(customAlarm.IsAtTarget ? $"{CQCode.At(customAlarm.TargetId)}{PushExtensions.TITLE}{ENTER}[内容] " : string.Empty)}" + $"{alarmMessage}")
                                    .ConfigureAwait(false);
                            }
                            else
                            {
                                await Api.SendPrivateMessageAsync(targetId, customAlarm.GroupId,
                                    $"{(customAlarm.IsAtTarget ? $"{PushExtensions.TITLE}{ENTER}[内容] " : string.Empty)}" + $"{alarmMessage}")
                                    .ConfigureAwait(false);
                            }
                        }

                        if (customAlarm.IsLoop)
                        {
                            customAlarm.AlarmDate = customAlarm.AlarmDate.AddMinutes(5);
                            return;
                        }

                        customAlarms.Remove(customAlarm.Id);
                    }
                }
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(e);
            }

            CommonExtensions.Sleep(1000);
        }
    }

    private static void ClearHistoryData()
    {
        var startDateNow = DateTime.Now;
        var customAlarms = PublicVar.BotConfig?.CustomAlarms;
        customAlarms?.Values
            .ForEach(customAlarm =>
            {
                // MEMO : 超过48小时, 因未知原因未进行提醒的消息将进行删除
                if ((startDateNow - customAlarm.AlarmDate).TotalHours >= 48)
                    customAlarms.Remove(customAlarm.Id);
            });
    }
}