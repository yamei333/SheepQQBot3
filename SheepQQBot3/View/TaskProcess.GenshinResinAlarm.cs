using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.GenshinHelper;
using Xunkong.Hoyolab;
using Xunkong.Hoyolab.Account;
using Xunkong.Hoyolab.DailyNote;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View;

public static partial class TaskProcess
{
    private static readonly Regex _regRemoveCQAt = RegexGenerator.CQCodeRemoveCQAt();
    private static readonly Regex _regGenshinWBAlarm = RegexGenerator.GenshinWBAlarm();
    private static readonly Regex _regGenshinDailyMission = RegexGenerator.GenshinDailyMission();
    private static readonly Regex _regGenshinPotCoin = RegexGenerator.GenshinPotCoin();
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
    public static void GenshinResinAlarm()
    {
        AddRunLog(new RunLog_SystemInfo("原神每日提醒 模块已运行"));
        while (true)
        {
            try
            {
                if (Api?.IsConnected == true)
                {
                    var dateNow = DateTime.Now;
                    if (dateNow.Hour is not (>= 3 and <= 7))
                    {
                        Vm.SetConfigs?.Values
                            .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Group_GenshinHelper))
                            .ForEach(setConfig =>
                            {
                                setConfig.GenshinHelperConfig?.GenshinResinAlarms?.ToValueList()
                                    .ForEach(SendGenshinDailyNoteAlarmMessageAction);

                                async void SendGenshinDailyNoteAlarmMessageAction(GenshinResinAlarm genshinResinAlarm)
                                {
                                    if (!genshinResinAlarm.IsActive)
                                        return;

                                    DeleteExpiredData(setConfig.GenshinResinAlarmedList, dateNow, 900);
                                    await SendGenshinDailyNoteAlarmMessage(setConfig, genshinResinAlarm, dateNow).ConfigureAwait(false);
                                }
                            });
                    }
                }
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(e);
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
            .Any(each => each.Id == configId))
            return;

        var cookie = genshinResinAlarm.Cookies;
        var client = new HoyolabClient();
        DailyNoteInfo dailyNote;
        GenshinRoleInfo role = default;
        try
        {
            var roles = await client.GetGenshinRoleInfosAsync(cookie).ConfigureAwait(false);
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
            dailyNote = await client.GetDailyNoteAsync(role).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            var errorMessage = e.Message;
            if (errorMessage == RISK_ACCOUNT)
            {
                AddRunLog(new RunLog_SystemWarning($"米游社风控账户({genshinResinAlarm.ConfigName})"));
                return;
            }

            YameiLogExtensions.WriteLog(LogType.Error,
                $"GenshinResinAlarm.GetDailyNoteAsync Exception:{genshinResinAlarm.ConfigName}{e.Message}");
            AddRunLog(new RunLog_SystemError($"GenshinResinAlarm.GetDailyNoteAsync Exception:{genshinResinAlarm.ConfigName}{e.HResult}({e.Message})"));
            return;
        }

        var currentResin = dailyNote.CurrentResin;
        var alarmTargetId = genshinResinAlarm.TargetId;
        var targetId = setConfig.TargetId;
        var targetType = setConfig.TargetType;
        var dateNowStr = now.ToConditionString(HolidayInfo);
        var sendMessage = string.Empty;

        #region WB提醒

        if (_regGenshinWBAlarm.IsMatch(dateNowStr))
        {
            var genshinGachaInfoRequest = await HttpExtensions.GetFromJsonAsync<GenshinGachaInfoRequest>(
                "https://webstatic.mihoyo.com/hk4e/gacha_info/cn_gf01/gacha/list.json").ConfigureAwait(false);
            var gachaInfo = genshinGachaInfoRequest?.Data.List.FirstOrDefault(each => each.GachaName == "角色活动");
            if (gachaInfo != null)
            {
                var diffDays = (int)(now - gachaInfo.BeginTime).TotalDays;
                if (diffDays is 0 or 1 or 2)
                {
                    SendBarkMessageAsync($"[原神WB签到提醒]-WB签到第{diffDays + 1}天!");
                    if (!forceSend)
                        setConfig.GenshinResinAlarmedList[(configId, GenshinDailyNoteAlarmType.Weibo)] = now;
                }
            }
        }

        #endregion WB提醒

        #region 树脂

        if (genshinResinAlarm.Resin)
        {
            switch (currentResin)
            {
                case 160:
                case 155:
                case 140:
                case 120:
                    AddSendMessage($"当前树脂为[{currentResin}/{dailyNote.MaxResin}], {_resinMessage.Random()}",
                        GenshinDailyNoteAlarmType.Resin);
                    break;
                default:
                    if (currentResin >= 85 && _regGenshinDailyMission.IsMatch(dateNowStr))
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

        if (genshinResinAlarm.DailyMission && !dailyNote.IsExtraTaskRewardReceived && _regGenshinDailyMission.IsMatch(dateNowStr))
            AddSendMessage($"今天每日任务还没做, 要血亏了!", GenshinDailyNoteAlarmType.DailyMission);

        #endregion 每日任务

        #region 洞天宝钱

        var potCoin = dailyNote.CurrentHomeCoin;
        if (genshinResinAlarm.PotCoin && _regGenshinPotCoin.IsMatch(dateNowStr) && potCoin >= 2100)
            AddSendMessage($"当前洞天宝钱为[{potCoin}/{dailyNote.MaxHomeCoin}], 快满了!",
                GenshinDailyNoteAlarmType.PotCoin);

        #endregion 洞天宝钱

        #region 参量质变仪

        if (genshinResinAlarm.Transformer
            && dailyNote.Transformer.Obtained
            && dailyNote.Transformer.RecoveryTime.Reached
            && _regGenshinTransformer.IsMatch(dateNowStr))
            AddSendMessage($"参量质变仪可用了!", GenshinDailyNoteAlarmType.Transformer);

        #endregion 参量质变仪

        SendMessage();

        void AddSendMessage(string msg, GenshinDailyNoteAlarmType alarmType)
        {
            if (!string.IsNullOrEmpty(sendMessage))
                sendMessage += ENTER;

            if (!sendMessage!.Contains("CQ:at"))
                sendMessage += CQCode.At(alarmTargetId);

            sendMessage += msg;
            if (!forceSend)
                setConfig.GenshinResinAlarmedList[(configId, alarmType)] = now;
        }

        async void SendMessage()
        {
            if (string.IsNullOrEmpty(sendMessage))
                return;

            SendBarkMessageAsync(_regRemoveCQAt.Replace(sendMessage, string.Empty));
            switch (targetType)
            {
                case BotConfigTargetType.Group:
                    await Api.SendGroupMessageAsync(targetId, sendMessage, Vm.SetConfigs).ConfigureAwait(false);
                    AddRunLog(new RunLog_GenshinDailyNoteAlarm(
                        BotConfigTargetType.Group, targetId, sendMessage));
                    break;
                case BotConfigTargetType.Private:
                    await Api.SendPrivateMessageAsync(targetId, sendMessage).ConfigureAwait(false);
                    AddRunLog(new RunLog_GenshinDailyNoteAlarm(
                        BotConfigTargetType.Private, targetId, sendMessage));
                    break;
                case BotConfigTargetType.Common:
                default:
                    throw new ArgumentOutOfRangeException(
                        $"{nameof(SendGenshinDailyNoteAlarmMessage)}.{nameof(setConfig.TargetType)}", targetType.ToString());
            }
        }

        async void SendBarkMessageAsync(string barkMessage)
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
    }
}