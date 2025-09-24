using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using System;
using System.Linq;
using System.Threading.Tasks;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotTask;

public static partial class TaskProcess
{
    /// <summary>
    /// 最短重复执行间隔(秒)
    /// </summary>
    private static int MIN_REPEAT_EXECUTE_SECONDS = 120;

    /// <summary>
    /// 闹钟助手
    /// </summary>
    public static void AlarmAides()
    {
        AddTaskRunLog("闹钟助手");
        try
        {
            while (true)
            {
                if (BotServer?.Connected == true)
                {
                    var dateNow = DateTime.Now;
                    var dateNowStr = dateNow.ToConditionString(HolidayInfo);
                    Vm.SetConfigs?.Values
                        .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Common_AlarmAide))
                        .ForEach(setConfig =>
                        {
                            setConfig.AlarmAideConfigs.ToValueList()
                                .Where(each => each.IsActive && (dateNow - each.LastExecuteDate).TotalSeconds > MIN_REPEAT_EXECUTE_SECONDS)
                                .ForeachAsync(SendAction);
                            return;

                            async Task SendAction(AlarmAideConfig alarmAideConfig)
                            {
                                var condition = alarmAideConfig.Condition;
                                var jsonCondition = RegexGenerator.ConditionJsonText();
                                var match = jsonCondition.Match(condition);
                                if (match.Success)
                                {
                                    var matchValue = match.Value;
                                    condition = condition.Replace(matchValue, string.Empty);
                                    var extendCondition = matchValue.Replace("$", string.Empty).FromJson<AlarmAideExtendCondition>();
                                    if (extendCondition.DayOfMonthOffset.HasValue)
                                    {
                                        var dayOfMonthOffsetValue = extendCondition.DayOfMonthOffset.GetValueOrDefault();
                                        var (dayOfMonth, lastDayOfMonth) = DateTime.Now.GetDayOfMonthAndLastDayOfMonth();
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

                                // MEMO : 设置最终执行时间
                                alarmAideConfig.LastExecuteDate = dateNow;
                                // MEMO : 发送闹钟助手消息
                                await SendAlarmAsync(setConfig, alarmAideConfig).ConfigureAwait(false);
                            }
                        });
                }

                CommonExtensions.Sleep(1000);
            }
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteLog(e);
        }
    }

    /// <summary>
    /// 发送闹钟助手消息
    /// </summary>
    private static async Task SendAlarmAsync(SetConfig setConfig, AlarmAideConfig alarmAideConfig)
    {
        var alarmTexts = alarmAideConfig.AlarmTexts;
        if (!alarmTexts.IsEmpty)
        {
            var alarmText = alarmTexts.Values.Random();
            var targetId = setConfig.TargetId;
            switch (setConfig.TargetType)
            {
                case BotConfigTargetType.Group:
                    await BotClient.SendGroupMessageAsync(targetId, alarmText, Vm.SetConfigs).ConfigureAwait(false);
                    AddRunLog(new RunLog_AlarmAide(BotConfigTargetType.Group, targetId, alarmText));
                    break;
                case BotConfigTargetType.Private:
                    await BotClient.SendPrivateMessageAsync(targetId, alarmText).ConfigureAwait(false);
                    AddRunLog(new RunLog_AlarmAide(BotConfigTargetType.Private, targetId, alarmText));
                    break;
                case BotConfigTargetType.Common:
                default:
                    throw new ArgumentOutOfRangeException(
                        $"{nameof(SendAlarmAsync)}.{nameof(setConfig.TargetType)}",
                        setConfig.TargetType.ToString());
            }
        }
    }
}