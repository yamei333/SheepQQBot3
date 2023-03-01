using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using Xunkong.Hoyolab;
using Xunkong.Hoyolab.Account;
using Xunkong.Hoyolab.DailyNote;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View;

public static partial class TaskProcess
{
    private static readonly string[] _resinMessage = {
        "DJLJ!", "体力爆炸了!", "树脂在燃烧!", "反正也是辣鸡圣遗物", "KJ! JS!"
    };

    /// <summary>
    /// 原神每日提醒
    /// </summary>
    public static void GenshinResinAlarm()
    {
        AddRunLog(new RunLog_SystemInfo("原神每日提醒 模块已运行"));
        while (true)
        {
            if (Api?.IsConnected == true)
            {
                var dateNow = DateTime.Now;
                if (dateNow.Hour is not (>= 2 and <= 7))
                {
                    Vm.SetConfigs?.Values
                        .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Group_GenshinHelper))
                        .ForEach(setConfig =>
                        {
                            setConfig.GenshinHelperConfig.GenshinResinAlarms?.ToValueList()
                                .ForEach(DeleteExpiredDataAction);

                            async void DeleteExpiredDataAction(GenshinResinAlarm genshinResinAlarm)
                            {
                                if (!genshinResinAlarm.IsActive)
                                    return;

                                setConfig.GenshinResinAlarmedList ??=
                                    new Dictionary<(Guid, GenshinDailyNoteAlarmType), DateTime>();
                                DeleteExpiredData(setConfig.GenshinResinAlarmedList, dateNow, 600);
                                await SendGenshinDailyNoteAlarmMessage(setConfig, genshinResinAlarm, dateNow);
                            }
                        });
                }
            }

            CommonExtensions.Sleep(240000);
        }
    }

    /// <summary>
    /// 发送原神每日提醒消息
    /// </summary>
    public static async Task SendGenshinDailyNoteAlarmMessage(
        SetConfig setConfig,
        GenshinResinAlarm genshinResinAlarm,
        DateTime now,
        bool forceSend = false)
    {
        var configId = genshinResinAlarm.Id;
        if (!forceSend && setConfig.GenshinResinAlarmedList.Keys
            .Any(each => each.Item1 == configId))
            return;

        var cookie = genshinResinAlarm.Cookies;
        var client = new HoyolabClient();
        DailyNoteInfo dailyNote;
        GenshinRoleInfo role;
        try
        {
            var roles = await client.GetGenshinRoleInfosAsync(cookie);
            role = roles[0];
        }
        catch (HoyolabException e)
        {
            YameiLogExtensions.WriteLog(LogType.Error,
                $"GenshinResinAlarm.GetGenshinRoleInfosAsync HoyolabException:{genshinResinAlarm.ConfigName}{e.ReturnCode}-{e.Message}");
            AddRunLog(new RunLog_SystemError($"GenshinResinAlarm.GetGenshinRoleInfosAsync HoyolabException:{genshinResinAlarm.ConfigName}{e.ReturnCode}-{e.Message}"));
            return;
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteLog(LogType.Error,
                $"GenshinResinAlarm.GetGenshinRoleInfosAsync Exception:{genshinResinAlarm.ConfigName}{e.Message}");
            AddRunLog(new RunLog_SystemError($"GenshinResinAlarm.GetGenshinRoleInfosAsync Exception:{genshinResinAlarm.ConfigName}{e.Message}"));
            return;
        }

        try
        {
            dailyNote = await client.GetDailyNoteAsync(role);
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteLog(LogType.Error,
                $"GenshinResinAlarm.GetDailyNoteAsync Exception:{genshinResinAlarm.ConfigName}{e.Message}");
            AddRunLog(new RunLog_SystemError($"GenshinResinAlarm.GetDailyNoteAsync Exception:{genshinResinAlarm.ConfigName}{e.Message}"));
            return;
        }

        var currentResin = dailyNote.CurrentResin;
        var alarmTargetId = genshinResinAlarm.TargetId;
        var targetId = setConfig.TargetId;
        var targetType = setConfig.TargetType;
        var dateNowStr = now.ToConditionString(HolidayInfo);
        var sendMessage = string.Empty;

        #region 树脂

        if (genshinResinAlarm.Resin)
        {
            switch (currentResin)
            {
                case 155:
                case 140:
                case 120:
                    AddSendMessage($"当前树脂为[{currentResin}/{dailyNote.MaxResin}], {_resinMessage.Random()}",
                        GenshinDailyNoteAlarmType.Resin);
                    break;
                default:
                    if (currentResin >= 85 && RegexGenerator.GenshinDailyMission().IsMatch(dateNowStr))
                    {
                        AddSendMessage($"当前树脂为[{currentResin}/{dailyNote.MaxResin}], " +
                                       $"体力会在明天10点前爆炸, 你怎么睡得着!",
                            GenshinDailyNoteAlarmType.Resin);
                    }

                    break;
            }
        }

        #endregion 树脂

        #region 每日任务

        if (genshinResinAlarm.DailyMission && !dailyNote.IsExtraTaskRewardReceived && RegexGenerator.GenshinDailyMission().IsMatch(dateNowStr))
            AddSendMessage($"今天每日任务还没做, 要血亏了!", GenshinDailyNoteAlarmType.DailyMission);

        #endregion 每日任务

        #region 洞天宝钱

        var potCoin = dailyNote.CurrentHomeCoin;
        if (genshinResinAlarm.PotCoin && RegexGenerator.GenshinPotCoin().IsMatch(dateNowStr) && potCoin >= 2000)
            AddSendMessage($"当前洞天宝钱为[{potCoin}/{dailyNote.MaxHomeCoin}], 快满了!",
                GenshinDailyNoteAlarmType.PotCoin);

        #endregion 洞天宝钱

        #region 参量质变仪

        if (genshinResinAlarm.Transformer && dailyNote.Transformer.Obtained && RegexGenerator.GenshinTransformer().IsMatch(dateNowStr))
            AddSendMessage($"参量质变仪可用了!", GenshinDailyNoteAlarmType.Transformer);

        #endregion 参量质变仪

        SendMessage();

        void AddSendMessage(string msg, GenshinDailyNoteAlarmType alarmType)
        {
            if (!string.IsNullOrEmpty(sendMessage))
                sendMessage += ENTER;

            if (!sendMessage!.Contains("CQ:at"))
                sendMessage += $"[CQ:at,qq={alarmTargetId}]";

            sendMessage += msg;
            if (!forceSend)
                setConfig.GenshinResinAlarmedList.Add((configId, alarmType), now);
        }

        async void SendMessage()
        {
            if (string.IsNullOrEmpty(sendMessage))
                return;

            switch (targetType)
            {
                case BotConfigTargetType.Group:
                    await Api.SendGroupMessage(targetId, sendMessage, Vm.SetConfigs);
                    AddRunLog(new RunLog_GenshinDailyNoteAlarm(
                        BotConfigTargetType.Group, targetId, sendMessage));
                    break;
                case BotConfigTargetType.Private:
                    await Api.SendPrivateMessage(targetId, sendMessage);
                    AddRunLog(new RunLog_GenshinDailyNoteAlarm(
                        BotConfigTargetType.Private, targetId, sendMessage));
                    break;
                case BotConfigTargetType.Common:
                default:
                    throw new ArgumentOutOfRangeException(
                        $"{nameof(SendGenshinDailyNoteAlarmMessage)}.{nameof(setConfig.TargetType)}", targetType.ToString());
            }
        }
    }
}