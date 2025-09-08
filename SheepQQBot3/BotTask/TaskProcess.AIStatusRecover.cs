using CommonLibrary;
using SheepQQBot3.Extensions;
using System;
using System.Text.RegularExpressions;
using static SheepQQBot3.Extensions.LogExtensions;

namespace SheepQQBot3.BotTask;

public static partial class TaskProcess
{
    private static readonly Regex _aiStatusRevocer = new(@"\d{4}-\d{2}-\d{2}-\d{1}-\d{1} (00|03|04|05|08|12|16|20):00:\d{2}");

    /// <summary>
    /// AI状态恢复
    /// </summary>
    public static void AIStatusRecover()
    {
        AddTaskRunLog("AI状态恢复");
        try
        {
            while (true)
            {
                var dateNow = DateTime.Now;
                var dateNowStr = dateNow.ToConditionString(PublicVar.HolidayInfo);
                if (!_aiStatusRevocer.IsMatch(dateNowStr))
                {
                    CommonExtensions.SleepMinutes(5);
                    continue;
                }

                var moodIndexValue = PublicVar.AIData.AIStatusData.MoodIndexValue;
                if (moodIndexValue is >= -5 and <= 5)
                {
                    CommonExtensions.SleepMinutes(5);
                    continue;
                }

                PublicVar.AIData.AIStatusData.MoodIndexValue = (int)(moodIndexValue * 0.9);
                ConfigExtensions.SaveAIData();
            }
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteLog(e);
        }
    }
}