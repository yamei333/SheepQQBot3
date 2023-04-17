using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using Yamei.Common;

namespace SheepQQBot3.Extensions;

public static class BotExtensions
{
    /// <summary>
    /// 是否使用中的BotFunction
    /// </summary>
    /// <param name="botFunctions">botFunctions</param>
    /// <param name="botFunctionType"><see cref="BotFunctionType"/></param>
    /// <returns></returns>
    public static bool IsUsed(this List<BotFunction> botFunctions, BotFunctionType botFunctionType)
        => botFunctions.FirstOrDefault(each => each.BotFunctionType == botFunctionType)?.IsUsed ?? false;

    /// <summary>
    /// 关闭gocq进程
    /// </summary>
    public static void KillGocqexe()
    {
        var processes = Process.GetProcessesByName(ConfigurationManager.AppSettings["gocqexe"]?.Replace(".exe", string.Empty));
        processes.ForEach(each => each.Kill());
    }

    /// <summary>
    /// 关闭Bark进程
    /// </summary>
    public static void KillBarkexe()
    {
        var processes = Process.GetProcessesByName(ConfigurationManager.AppSettings["barkexe"]?.Replace(".exe", string.Empty));
        processes.ForEach(each => each.Kill());
    }

    public static string GetSetuSuccessPercent(long setuDoushiLv)
    {
        var failedSum = 200 + (int)(150 * Math.Pow(setuDoushiLv, 2))
                            + 200 + (int)(150 * Math.Pow(setuDoushiLv, 2))
                            + 100 + (int)(75 * Math.Pow(setuDoushiLv, 2))
                            + 50 + (int)(40 * Math.Pow(setuDoushiLv, 2));
        return $"色图基础成功率为 {3190 / (3190.0 + failedSum):0.00%}";
    }
}