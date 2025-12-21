using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.DbModel;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SheepQQBot3.Extensions;

public static partial class BotExtensions
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
    /// 关闭NapCat进程
    /// </summary>
    public static void KillServerExe()
    {
        var napCatPath = AppSettingExtensions.Get("napcat");
        var napCatKill = AppSettingExtensions.Get("napcatkill");
        if (!napCatPath.IsNullOrEmpty() && !napCatKill.IsNullOrEmpty())
        {
            new Process
            {
                StartInfo =
                {
                    WorkingDirectory = napCatPath!,
                    FileName = Path.Combine(napCatPath, napCatKill),
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = true,
                },
            }.Start();
        }
    }

    /// <summary>
    /// 关闭Bark进程
    /// </summary>
    public static void KillBarkExe()
    {
        var processes = Process.GetProcessesByName(AppSettingExtensions.Get("barkexe").Replace(".exe", string.Empty));
        processes.ForEach(each => each.Kill());
    }

    /// <summary>
    /// 取得色图成功率
    /// </summary>
    public static string GetSetuSuccessPercent(SetuDoushiInfo setuDoushiInfo, DateTime dateNow)
    {
        var setuDoushiLv = setuDoushiInfo?.CalcSetuDoushiLv(dateNow) ?? 0;
        var failedSum = (int)(900 * Math.Pow(setuDoushiLv, 2.5))
            + (int)(900 * Math.Pow(setuDoushiLv, 2.5))
            + (int)(450 * Math.Pow(setuDoushiLv, 2.5))
            + (int)(240 * Math.Pow(setuDoushiLv, 2.5));
        return $"色图成功率 {(26426) / (26426.0 + failedSum):0.00%}";
    }

    /// <summary>
    /// 是否有用户函数
    /// </summary>
    public static bool HasUserConfig(string targetId, UserConfigType userConfigType)
    {
        var userConfigs = PublicVar.GlobalBotConfig.UserConfigs;
        return userConfigs != null
            && userConfigs.TryGetValue(targetId, out var userConfig)
            && userConfig != null
            && userConfig.ContainsKey(userConfigType);
    }

    public static bool IsAdmin(string targetId) => PublicVar.AdminIds.Contains(targetId);
}