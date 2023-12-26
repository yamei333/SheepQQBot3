using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.SDK.Api;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View;

public static partial class TaskProcess
{
    private static readonly Regex _regRemoveCQAt = RegexGenerator.CQCodeRemoveCQAt();
    private static readonly Regex _regGenshinWbAlarm = RegexGenerator.GenshinWbAlarm();
    private static readonly Regex _regGenshinResin = RegexGenerator.GenshinResin();
    private static readonly Regex _regGenshinDailyMission = RegexGenerator.GenshinDailyMission();
    private static readonly Regex _regGenshinTransformer = RegexGenerator.GenshinTransformer();

    private static readonly string[] _resinMessage = {
        "DJLJ!", "体力爆炸了!", "树脂在燃烧!", "反正也是辣鸡圣遗物", "KJ! JS!"
    };

    /// <summary>
    /// 风控账户
    /// </summary>
    private const string RISK_ACCOUNT = " (1034)";

    /// <summary>
    /// 原神每日提醒
    /// </summary>
    public static async Task GenshinResinAlarmAsync()
    {
        AddTaskRunLog("原神每日提醒");
        while (true)
        {
            try
            {
                if (Api?.Connected == true)
                {
                    var dateNow = DateTime.Now;
                    if (dateNow.Hour is <= 2 or >= 8)
                    {
                        var setConfigs = Vm.SetConfigs?.Values
                            .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Group_GenshinHelper))
                            .ToList();
                        if (setConfigs is { Count: > 0 })
                        {
                            DGPProcessOK = false;
                            DGPExtensions.DailyRefreshNoteDGP();
                            var isSuccessed = await 10.TryTimesAsync(async () =>
                            {
                                if (DGPProcessOK)
                                    return true;

                                CommonUtil.Sleep(1000);
                                return false;
                            }).ConfigureAwait(false);

                            if (isSuccessed)
                            {
                                setConfigs.ForEach(setConfig =>
                                {
                                    setConfig.GenshinHelperConfig?.GenshinResinAlarms?.ToValueList()
                                        .ForEach(SendGenshinDailyNoteAlarmMessageAction);
                                    return;

                                    async void SendGenshinDailyNoteAlarmMessageAction(
                                        GenshinResinAlarm genshinResinAlarm)
                                    {
                                        if (!genshinResinAlarm.IsActive)
                                            return;

                                        await SendGenshinDailyNoteAlarmMessageAsync(setConfig, genshinResinAlarm,
                                            dateNow).ConfigureAwait(false);
                                    }
                                });

                                AddRunLog(new RunLog_SystemInfo("刷新原神便笺任务完成"));
                                CommonExtensions.SleepMinutes(30);
                            }
                            else
                            {
                                AddRunLog(new RunLog_SystemInfo("刷新原神便笺任务失败!"));
                                CommonExtensions.SleepMinutes(5);
                            }
                        }
                        else
                        {
                            CommonExtensions.SleepMinutes(1);
                        }
                    }
                    else
                    {
                        CommonExtensions.SleepMinutes(1);
                    }
                }
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(e);
                CommonExtensions.SleepSeconds(30);
            }
        }
    }

    /// <summary>
    /// 发送原神每日提醒消息
    /// </summary>
    public static async Task SendGenshinDailyNoteAlarmMessageAsync(
        SetConfig setConfig,
        GenshinResinAlarm genshinResinAlarm,
        DateTime now)
    {
        var targetId = genshinResinAlarm.TargetId;
        var groupId = setConfig.TargetId;
        var dailyNote = GenshinDailyNote[targetId];
        var currentResin = dailyNote.CurrentResin;
        var targetType = setConfig.TargetType;
        var dateNowStr = now.ToConditionString(HolidayInfo);
        var sendMessage = string.Empty;

        #region 树脂

        if (genshinResinAlarm.Resin)
        {
            switch (currentResin)
            {
                case >= 140:
                    AddSendMessage($"当前树脂为[{currentResin}/{dailyNote.MaxResin}], {_resinMessage.Random()}");
                    break;
                case >= 90 when _regGenshinResin.IsMatch(dateNowStr):
                    AddSendMessage($"当前树脂为[{currentResin}/{dailyNote.MaxResin}], " +
                                   $"体力会在明天10点前爆炸, 你怎么睡得着!");
                    break;
            }
        }

        #endregion 树脂

        #region 每日任务

        if (genshinResinAlarm.DailyMission && !dailyNote.DailyTask.IsExtraTaskRewardReceived && _regGenshinDailyMission.IsMatch(dateNowStr))
            AddSendMessage("今天每日任务还没做, 要血亏了!");

        #endregion 每日任务

        #region 洞天宝钱

        var potCoin = dailyNote.CurrentHomeCoin;
        if (genshinResinAlarm.PotCoin && potCoin >= 2320)
            AddSendMessage($"当前洞天宝钱为[{potCoin}/{dailyNote.MaxHomeCoin}], 快满了!");

        #endregion 洞天宝钱

        #region 参量质变仪

        if (genshinResinAlarm.Transformer
            && dailyNote.Transformer.Obtained
            && dailyNote.Transformer.RecoveryTime.Reached
            && _regGenshinTransformer.IsMatch(dateNowStr))
            AddSendMessage($"参量质变仪可用了!");

        #endregion 参量质变仪

        if (string.IsNullOrEmpty(sendMessage))
            return;

        await SendBarkMessageAsync(_regRemoveCQAt.Replace(sendMessage, string.Empty)).ConfigureAwait(false);
        switch (targetType)
        {
            case BotConfigTargetType.Group:
                await Api.SendGroupMessageAsync(groupId, sendMessage, Vm.SetConfigs).ConfigureAwait(false);
                AddRunLog(new RunLog_GenshinDailyNoteAlarm(
                    BotConfigTargetType.Group, groupId, sendMessage));
                break;
            case BotConfigTargetType.Private:
                await Api.SendPrivateMessageAsync(groupId, sendMessage).ConfigureAwait(false);
                AddRunLog(new RunLog_GenshinDailyNoteAlarm(
                    BotConfigTargetType.Private, groupId, sendMessage));
                break;
            case BotConfigTargetType.Common:
            default:
                throw new ArgumentOutOfRangeException(targetType.ToString());
        }

        void AddSendMessage(string msg)
        {
            if (!string.IsNullOrEmpty(sendMessage))
                sendMessage += ENTER;

            if (!sendMessage!.Contains("CQ:at"))
                sendMessage += $"{CQCode.At(targetId)}{ENTER}";

            sendMessage += msg;
        }

        async Task SendBarkMessageAsync(string barkMessage)
        {
            var barkKey = genshinResinAlarm.BarkKey;
            if (!string.IsNullOrEmpty(barkKey))
            {
                await PushExtensions.PushBarkMessageAsync(
                    barkKey,
                    barkMessage,
                    PushExtensions.TITLE).ConfigureAwait(false);
            }
        }

        Task SendGetErrorMessageAsync() => Api.SendGroupMessageAsync(groupId, $"{CQCode.At(targetId)}原神便笺取得失败!", Vm.SetConfigs);
    }
}