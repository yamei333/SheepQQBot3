using CommonLibrary;
using SheepQQBot3.Extensions;
using System;
using System.Text.RegularExpressions;
using static SheepQQBot3.Extensions.LogExtensions;

namespace SheepQQBot3.BotTask;

public static partial class TaskProcess
{
    private static readonly Regex _aiStatusRevocer = new(@"\d{4}-\d{2}-\d{2}-\d{1}-\d{1} (00|01|02|03|04|05|06|07|08|12|16|20):00:\d{2}");

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
                    CommonExtensions.SleepMinutes(1);
                    continue;
                }

                var moodIndexValue = PublicVar.GlobalAIData.AIStatusData.MoodIndexValue;
                if (moodIndexValue == 0)
                {
                    CommonExtensions.SleepMinutes(1);
                    continue;
                }

                PublicVar.GlobalAIData.AIStatusData.MoodIndexValue = (int)(moodIndexValue * 0.9);
                ConfigExtensions.SaveAIData();
                CommonExtensions.SleepMinutes(30);
            }
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteLog(e);
        }
    }
}