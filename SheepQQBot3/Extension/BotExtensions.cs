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
    public static bool IsUsed(this List<BotFunction> botFunctions, BotFunctionType botFunctionType)
        => botFunctions.FirstOrDefault(each => each.BotFunctionType == botFunctionType)?.IsUsed ?? false;

    public static void KillGocqexe()
    {
        var processes = Process.GetProcessesByName(ConfigurationManager.AppSettings["gocqexe"]?.Replace(".exe", string.Empty));
        processes.ForEach(each => each.Kill());
    }
}