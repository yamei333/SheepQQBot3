using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    public static partial class TaskProcess
    {
        /// <summary>
        /// 闹钟助手
        /// </summary>
        public static void AlarmAides()
        {
            AddRunLog(new RunLog_SystemInfo("闹钟助手 模块已运行"));
            while (true)
            {
                try
                {
                    if (Api?.IsConnected == true)
                    {
                        var dateNow = DateTime.Now;
                        var dateNowStr = dateNow.ToConditionString(HolidayInfo);
                        Vm.SetConfigs?.Values
                            .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Common_AlarmAide))
                            .ForEach(setConfig =>
                            {
                                setConfig.AlarmAideConfigs.ToValueList().ForEach(DeleteExpiredDataAction);
                                async void DeleteExpiredDataAction(AlarmAideConfig alarmAidesConfig)
                                {
                                    if (!alarmAidesConfig.IsActive)
                                        return;

                                    var condition = alarmAidesConfig.Condition;
                                    var jsonCondition = RegexGenerator.ConditionJsonText();
                                    var match = jsonCondition.Match(condition);
                                    if (match.Success)
                                    {
                                        var matchValue = match.Value;
                                        condition = condition.Replace(matchValue, string.Empty);
                                        var extendCondition = JsonSerializer.Deserialize<AlarmAideExtendCondition>(
                                            matchValue.Replace("$", string.Empty));
                                        if (extendCondition.DayOfMonthOffset.HasValue)
                                        {
                                            var dayOfMonthOffsetValue = extendCondition.DayOfMonthOffset.GetValueOrDefault();
                                            var (dayOfMonth, lastDayOfMonth) = YameiExtensions.GetDayOfMonthAndLastDayOfMonth(DateTime.Now);
                                            if (dayOfMonthOffsetValue > 0)
                                            {
                                                if (dayOfMonthOffsetValue != dayOfMonth)
                                                    return;
                                            }
                                            else
                                            {
                                                if (dayOfMonthOffsetValue != -lastDayOfMonth)
                                                    return;
                                            }
                                        }
                                    }

                                    if (!condition.IsMatch(dateNowStr))
                                        return;

                                    setConfig.AlarmAideAlarmedList ??= new Dictionary<Guid, DateTime>();
                                    // 删除过期发送内容
                                    DeleteExpiredData(setConfig.AlarmAideAlarmedList, dateNow);
                                    // 发送闹钟助手消息
                                    await SendAlarm(setConfig, alarmAidesConfig, dateNow);
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

        /// <summary>
        /// 发送闹钟助手消息
        /// </summary>
        private static async Task SendAlarm(SetConfig setConfig, AlarmAideConfig alarmAideConfig, DateTime now)
        {
            var alarmInfoKey = alarmAideConfig.Id;
            if (setConfig.AlarmAideAlarmedList.ContainsKey(alarmInfoKey))
                return;

            var alarmTexts = alarmAideConfig.AlarmTexts;
            if (!alarmTexts.IsEmpty)
            {
                var alarmText = alarmTexts.Values.Random();
                var targetId = setConfig.TargetId;
                switch (setConfig.TargetType)
                {
                    case BotConfigTargetType.Group:
                        await Api.SendGroupMessage(targetId, alarmText, Vm.SetConfigs);
                        AddRunLog(new RunLog_AlarmAide(BotConfigTargetType.Group, targetId, alarmText));
                        break;
                    case BotConfigTargetType.Private:
                        await Api.SendPrivateMessage(targetId, alarmText);
                        AddRunLog(new RunLog_AlarmAide(BotConfigTargetType.Private, targetId, alarmText));
                        break;
                    case BotConfigTargetType.Common:
                    default:
                        throw new ArgumentOutOfRangeException(
                            $"{nameof(SendAlarm)}.{nameof(setConfig.TargetType)}",
                            setConfig.TargetType.ToString());
                }

                // MEMO : 追加到已发送列表
                setConfig.AlarmAideAlarmedList.Add(alarmInfoKey, now);
            }
        }
    }
}