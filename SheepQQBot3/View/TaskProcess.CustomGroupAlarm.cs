using System;
using System.Collections.Generic;
using System.Linq;
using CommonLibrary;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    public static partial class TaskProcess
    {
        /// <summary>
        /// 自定义群消息
        /// </summary>
        public static void CustomGroupAlarm()
        {
            AddRunLog(new RunLog_SystemInfo("自定义群提醒 模块已运行"));
            // MEMO : 清理过期提醒
            ClearHistoryData();

            while (true)
            {
                try
                {
                    if (Api?.IsConnected == true)
                    {
                        var dateNow = DateTime.Now;
                        Vm.SetConfigs?.Values
                            .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Group_CustomGroupAlarm))
                            .Select(each => each.CustomGroupAlarms)
                            .ForEach(groupAlarms =>
                            {
                                var removeIds = new HashSet<Guid>();
                                groupAlarms.Values.ForEach(AlertMessage);
                                // MEMO : 移除需要删除的消息
                                removeIds.ForEach(removeId => groupAlarms.Remove(removeId));

                                async void AlertMessage(CustomGroupAlarm groupAlarm)
                                {
                                    var overSeconds = (dateNow - groupAlarm.AlarmDate).TotalSeconds;
                                    // MEMO : 超过3分钟内都可以提醒
                                    if (overSeconds is >= 0 and <= 180)
                                    {
                                        var alarmMessage = groupAlarm.AlarmMessage.ToCqCode(0).Result;
                                        var sendCustomMessage = $"{(groupAlarm.IsAtTarget ? $"[CQ:at,qq={groupAlarm.TargetId}] 小助手提醒!{ENTER}[内容] " : string.Empty)}" + $"{alarmMessage}";
                                        await Api.SendGroupMessage(groupAlarm.GroupId, sendCustomMessage, Vm.SetConfigs);
                                        // MEMO : 添加为删除消息
                                        removeIds.Add(groupAlarm.Id);
                                    }
                                }
                            });
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
            Vm.SetConfigs?.Values
                .Select(each => each.CustomGroupAlarms)
                .ForEach(groupAlarms =>
                {
                    var removeIds = new HashSet<Guid>();
                    groupAlarms.Values.ForEach(groupAlarm =>
                    {
                        var overHours = (startDateNow - groupAlarm.AlarmDate).TotalHours;
                        // MEMO : 超过48小时, 因未知原因未进行提醒的消息将进行删除
                        if (overHours >= 48)
                            removeIds.Add(groupAlarm.Id);
                    });
                    // MEMO : 移除需要删除的消息
                    removeIds.ForEach(removeId => groupAlarms.Remove(removeId));
                });
        }
    }
}