using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SheepQQBot3.Extensions;

public static partial class BotExtensions
{
    private static readonly Regex _regCQCode = RegexGenerator.CQCode();
    private static readonly Regex _regSetuCommand = new(@"^(?!.*\[CQ:).*色图[a-zA-Z]?$", RegexOptions.Singleline);
    private static readonly Regex _regEmoji = new(@"\p{Cs}");

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

    /// <summary>
    /// 不记录的群聊消息
    /// </summary>
    public static bool NeedNotRecordMessage(string message, Func<string, bool> otherCheckFunc = null)
    {
        // MEMO : Bot命令
        if (message.StartsWith("#") || message.StartsWith("r", StringComparison.CurrentCultureIgnoreCase))
            return true;

        // MEMO : 色图命令
        if (_regSetuCommand.IsMatch(message))
            return true;

        // MEMO : 包含特定人员的at
        if (message.Contains($"[CQ:at,qq=3889001246]"))
            return true;

        // MEMO : emoji数量超过一定数量
        if (_regEmoji.Matches(message).Count >= 6)
            return true;

        // MEMO : 其他自定义检测
        if (otherCheckFunc?.Invoke(message) == true)
            return true;

        // MEMO : 去除个别CQ码之后无任何内容的消息(转发, 小程序)
        return _regCQCode.Replace(message, match =>
        {
            var cqCode = match.Groups["tag"].Value;
            if (cqCode != "image" && cqCode != "at")
                return string.Empty;

            return match.Value;
        }).Trim().IsNullOrEmpty();
    }
}