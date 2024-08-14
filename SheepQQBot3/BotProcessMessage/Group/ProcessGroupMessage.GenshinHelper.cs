using CommonLibrary;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yamei.Common;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Group;

public static partial class ProcessGroupMessage
{
    /// <summary>
    /// 原神助手方法命令的开头
    /// </summary>
    private const string COMMAND_GENSHIN_HELPER = "#YS#";

    /// <summary>
    /// 原神助手
    /// </summary>
    /// <param name="genshinResinAlarms">提醒用户</param>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    /// <returns></returns>
    public static async Task<bool> GenshinHelperAsync(
        Dictionary<long, GenshinResinAlarm> genshinResinAlarms,
        GroupMessage groupMessage)
    {
        // MEMO : 非管理员/投稿者
        var targetId = groupMessage.Sender.UserId;
        if (targetId != 252961222)
            return true;

        if (genshinResinAlarms?.Keys.Contains(targetId) != true)
            return false;

        var groupId = groupMessage.GroupId;
        var message = groupMessage.Message;
        // MEMO : 命令格式检查
        var upperMessage = message.ToUpper();
        if (!upperMessage.StartsWith(COMMAND_GENSHIN_HELPER))
            return false;

        upperMessage = upperMessage.Replace(COMMAND_GENSHIN_HELPER, string.Empty);
        if (string.IsNullOrEmpty(upperMessage))
        {
            DGPProcessOK = false;
            DGPExtensions.DailyRefreshNoteDGP();
            var isSuccessed = await 10.TryTimesAsync(async () =>
            {
                if (DGPProcessOK)
                    return true;

                CommonExtensions.Sleep(1000);
                return false;
            }).ConfigureAwait(false);

            if (isSuccessed && GenshinDailyNote.TryGetValue(targetId, out var dailyNote))
            {
                try
                {
                    //var sendMessage = string.Empty;
                    var sendMessage = $"{CQCode.At(targetId)}{ENTER}";
                    var transformer = dailyNote.Transformer;
                    // MEMO : 一句话概括
                    var oneHint = !dailyNote.DailyTask.IsExtraTaskRewardReceived
                        ? $"你每日都没做完, 你怎么睡得着!"
                        : dailyNote.CurrentResin >= 60
                            ? $"你体力都没用完, 要亏辣鸡圣遗物!"
                            : dailyNote.ResinRecoveryDateTime.Hour <= 14
                                ? $"你体力会在{dailyNote.ResinRecoveryDateTime:HH:mm:ss}回满, 我觉得不是很保险"
                                : $"辣鸡任务都搞定了, 今天又是完美的一天";
                    //: abyssTotalStar == -1
                    //    ? $"辣鸡任务都搞定了, 今天又是完美的一天"
                    //    : abyssTotalStar < 36
                    //        ? abyssTotalStar > 0
                    //            ? $"都TM{dayOfMonth}号了, 你深渊才打到{abyssFloor}! 真菜啊!{ENTER}其他辣鸡任务都搞定了, 完美了, 但又没完全完美"
                    //            : $"都TM{dayOfMonth}号了, 你深渊还没打! 还要不要石头了!{ENTER}其他辣鸡任务都搞定了, 完美了, 但又没完全完美"
                    //        : $"辣鸡任务都搞定了, 今天又是完美的一天";
                    var resin = dailyNote.CurrentResin < 40
                        ? string.Empty
                        : $"{ENTER}树脂: {dailyNote.CurrentResin}/{dailyNote.MaxResin} ({dailyNote.ResinRecoveryDateTime:yyyy/M/d HH:mm})";
                    var dailyQuest = dailyNote.DailyTask.IsExtraTaskRewardReceived
                        ? string.Empty
                        : $"{ENTER}每日: {(dailyNote.DailyTask.FinishedTaskNumber == dailyNote.DailyTask.TotalTaskNumber
                            ? "完成任务但未领取奖励"
                            : $"任务还差{dailyNote.DailyTask.TotalTaskNumber - dailyNote.DailyTask.FinishedTaskNumber}个未完成!")}";
                    var potCoin = dailyNote.CurrentHomeCoin < 2040
                        ? string.Empty
                        : $"{ENTER}宝钱: {dailyNote.CurrentHomeCoin}/{dailyNote.MaxHomeCoin} ({dailyNote.HomeCoinRecoveryDateTime:yyyy/M/d HH:mm})";
                    var transformerStr = !transformer.Obtained
                        ? string.Empty
                        : transformer.RecoveryTime.Reached
                            ? $"{ENTER}质变: 已可用"
                            : string.Empty;
                    //$"{ENTER}质变: {(transformer.Obtained
                    //    ? transformer.RecoveryTime.Reached
                    //        ? "已可用"
                    //        : $"冷却中({dateNow.AddDays(transformer.RecoveryTime.Day)
                    //            .AddMinutes(transformer.RecoveryTime.Minute)
                    //            .AddSeconds(transformer.RecoveryTime.Second):yyyy/M/d HH:mm})"
                    sendMessage = sendMessage + oneHint +
                                  resin + dailyQuest + potCoin + transformerStr;
                    await BotServer.SendGroupMessageAsync(groupId, sendMessage).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    YameiLogExtensions.WriteLog(e);
                    await BotServer.SendGroupMessageAsync(groupId, $"{CQCode.At(targetId)}数据获取失败, 可能是cookie已失效!").ConfigureAwait(false);
                    return false;
                }
            }
            else
            {
            }
        }

        return false;
    }
}