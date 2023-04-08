using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using Xunkong.Hoyolab;
using Yamei.Common;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
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
        public static async Task<bool> GenshinHelper(
            Dictionary<long, GenshinResinAlarm> genshinResinAlarms,
            GroupMessage groupMessage)
        {
            // MEMO : 非管理员/投稿者
            var targetId = groupMessage.Sender.UserId;
            if (genshinResinAlarms?.Keys.Contains(targetId) != true)
                return false;

            var groupId = groupMessage.GroupId;
            var message = groupMessage.Message;
            // MEMO : 命令格式检查
            var upperMessage = message.ToUpper();
            if (!upperMessage.StartsWith(COMMAND_GENSHIN_HELPER))
                return false;

            upperMessage = upperMessage.Replace(COMMAND_GENSHIN_HELPER, string.Empty);
            var dateNow = DateTime.Now;
            if (string.IsNullOrEmpty(upperMessage))
            {
                var cookie = genshinResinAlarms[targetId].Cookies;
                var client = new HoyolabClient();
                try
                {
                    var roles = await client.GetGenshinRoleInfosAsync(cookie);
                    var role = roles[0];
                    var sendMessage = string.Empty;
                    var (dayOfMonth, lastDayOfMonth) = dateNow.GetDayOfMonthAndLastDayOfMonth();
                    var abyssTotalStar = -1;
                    var abyssFloor = string.Empty;
                    if (dayOfMonth is 13 or 14 or 15
                        || lastDayOfMonth is 1 or 2 or 3)
                    {
                        var spiralAbyssInfo = await client.GetSpiralAbyssInfoAsync(role, 1, CancellationToken.None);
                        if (spiralAbyssInfo != null)
                        {
                            abyssTotalStar = spiralAbyssInfo.TotalStar;
                            abyssFloor = spiralAbyssInfo.MaxFloor;
                        }
                    }

                    var dailyNote = await client.GetDailyNoteAsync(role);
                    var transformer = dailyNote.Transformer;
                    // MEMO : 一句话概括
                    var oneHint = !dailyNote.IsExtraTaskRewardReceived
                        ? $"你每日都没做完, 你怎么睡得着!"
                        : dailyNote.CurrentResin >= 60
                            ? $"你体力都没用完, 要亏辣鸡圣遗物!"
                            : dailyNote.ResinFullTime.Hour <= 14
                                ? $"你体力会在{dailyNote.ResinFullTime:HH:mm:ss}回满, 我觉得不是很保险"
                                : abyssTotalStar == -1
                                    ? $"辣鸡任务都搞定了, 今天又是完美的一天"
                                    : abyssTotalStar < 36
                                        ? abyssTotalStar > 0
                                            ? $"都TM{dayOfMonth}号了, 你深渊才打到{abyssFloor}! 真菜啊!{ENTER}其他辣鸡任务都搞定了, 完美了, 但又没完全完美"
                                            : $"都TM{dayOfMonth}号了, 你深渊还没打! 还要不要石头了!{ENTER}其他辣鸡任务都搞定了, 完美了, 但又没完全完美"
                                        : $"辣鸡任务都搞定了, 今天又是完美的一天";
                    var resin = dailyNote.CurrentResin < 40
                        ? string.Empty
                        : $"{ENTER}树脂: {dailyNote.CurrentResin}/{dailyNote.MaxResin} ({dailyNote.ResinFullTime:yyyy/M/d HH:mm})";
                    var dailyQuest = dailyNote.IsExtraTaskRewardReceived
                        ? string.Empty
                        : $"{ENTER}每日: {(dailyNote.FinishedTaskNumber == dailyNote.TotalTaskNumber
                            ? "完成任务但未领取"
                            : $"任务还差{dailyNote.TotalTaskNumber - dailyNote.FinishedTaskNumber}个未完成!")}";
                    var potCoin = dailyNote.CurrentHomeCoin < 2040
                        ? string.Empty
                        : $"{ENTER}宝钱: {dailyNote.CurrentHomeCoin}/{dailyNote.MaxHomeCoin} ({dailyNote.HomeCoinFullTime:yyyy/M/d HH:mm})";
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
                    await Api.SendGroupMessage(groupId, sendMessage);
                }
                catch (Exception e)
                {
                    YameiLogExtensions.WriteLog(e);
                    await Api.SendGroupMessage(groupId, $"{CQCode.At(targetId)} 数据获取失败, 可能是cookie已失效!");
                    return false;
                }
            }

            return false;
        }
    }
}