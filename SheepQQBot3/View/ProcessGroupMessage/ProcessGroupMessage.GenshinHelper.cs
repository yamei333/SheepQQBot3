using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using Xunkong.Hoyolab;
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
            var targetId = groupMessage.Sender.User_Id;
            if (!genshinResinAlarms.Keys.Contains(targetId))
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
                var cookie = genshinResinAlarms[targetId].Cookies;
                var client = new HoyolabClient();
                try
                {
                    var roles = await client.GetGenshinRoleInfosAsync(cookie);
                    var role = roles[0];
                    var dailyNote = await client.GetDailyNoteAsync(role);
                    await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {dailyNote.Nickname}({dailyNote.Uid})" +
                                                        $"{ENTER}树脂: {dailyNote.CurrentResin}/{dailyNote.MaxResin} ({dailyNote.ResinFullTime:yyyy/M/d HH:mm})" +
                                                        $"{ENTER}每日: {(dailyNote.IsExtraTaskRewardReceived ? "已领取" :
                                                            dailyNote.FinishedTaskNumber == dailyNote.TotalTaskNumber
                                                                ? "完成任务但未领取"
                                                                : $"任务未完成 {dailyNote.FinishedTaskNumber}/{dailyNote.TotalTaskNumber}")}" +
                                                        $"{ENTER}宝钱: {dailyNote.CurrentHomeCoin}/{dailyNote.MaxHomeCoin} ({dailyNote.HomeCoinFullTime:yyyy/M/d HH:mm})" +
                                                        $"{ENTER}质变: {(dailyNote.Transformer.Obtained
                                                            ? "可用"
                                                            : $"冷却中({dailyNote.Transformer.RecoveryTime:yyyy/M/d HH:mm})")}");
                }
                catch (Exception e)
                {
                    YameiLogExtensions.WriteLog(LogType.Error, $"GetDailyNoteAsync 失败:{e.Message}");
                    await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] 数据获取失败, 可能是cookie已失效!");
                    return false;
                }
            }

            return false;
        }
    }
}