using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using SheepQQBot3.DbModel;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.View;
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

    /// <summary>
    /// 取得色图成功率
    /// </summary>
    public static string GetSetuSuccessPercent(SetuDoushiInfo setuDoushiInfo, DateTime dateNow)
    {
        var setuDoushiLv = setuDoushiInfo?.CalcSetuDoushiLv(dateNow) ?? 0;
        var setuCd = setuDoushiInfo?.SetuCD ?? 0;
        var timeAdd = setuDoushiLv == 0 && setuCd != 0 ? (int)(dateNow - setuCd.ToDateTime()).TotalMinutes : 0;
        var failedSum = 200 + (int)(150 * Math.Pow(setuDoushiLv, 2))
                            + 200 + (int)(150 * Math.Pow(setuDoushiLv, 2))
                            + 100 + (int)(75 * Math.Pow(setuDoushiLv, 2))
                            + 50 + (int)(40 * Math.Pow(setuDoushiLv, 2));
        return $"色图成功率 {(3190 + timeAdd) / (3190.0 + timeAdd + failedSum):0.00%}";
    }

    /// <summary>
    /// 是否有用户函数
    /// </summary>
    public static bool HasUserConfig(long targetId, UserConfigType userConfigType)
    {
        var userConfigs = PublicVar.BotConfig.UserConfigs;
        return userConfigs != null
            && userConfigs.TryGetValue(targetId, out var userConfig)
            && userConfig != null
            && userConfig.ContainsKey(userConfigType);
    }
}