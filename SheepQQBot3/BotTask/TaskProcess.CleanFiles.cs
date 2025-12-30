using CommonLibrary;
using System;
using System.Text.RegularExpressions;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;

namespace SheepQQBot3.BotTask;

public static partial class TaskProcess
{
    private static readonly Regex _regCleanFiles = new(@"\d{4}-\d{2}-\d{2}-\d{1}-\d{1} 00:\d{2}:\d{2}");

    /// <summary>
    /// AI状态恢复
    /// </summary>
    public static void CleanFiles()
    {
        AddTaskRunLog("文件清理");
        try
        {
            while (true)
            {
                var dateNow = DateTime.Now;
                var dateNowStr = dateNow.ToConditionString(PublicVar.HolidayInfo);
                if (!_regCleanFiles.IsMatch(dateNowStr))
                {
                    CommonExtensions.SleepMinutes(30);
                    continue;
                }

                YameiExtensions.DeleteOldFilesAsync("Cache");
                YameiExtensions.DeleteOldFilesAsync("Log", 7);
                CommonExtensions.SleepMinutes(65);
            }
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteLog(e);
        }
    }
}