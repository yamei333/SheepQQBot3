using CommonLibrary;
using Masuit.Tools;
using Masuit.Tools.Systems;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.Fund;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotTask;

public static partial class TaskProcess
{
    public const int FUND_MAX_TRYTIMES = 5;

    /// <summary>
    /// 基金助手 (播报/阈值监控)
    /// </summary>
    public static void FundHelper()
    {
        AddTaskRunLog("基金助手");
        while (true)
        {
            try
            {
                if (BotServer?.Connected == true)
                {
                    var dateNow = DateTime.Now;
                    var dateNowStr = dateNow.ToConditionString(HolidayInfo);
                    Vm.SetConfigs?.Values
                        .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Group_FundHelper))
                        .ForEach(setConfig =>
                        {
                            setConfig.FundAlarmConfigs.ToValueList().ForEach(SendFundAlarmMessage);
                            async void SendFundAlarmMessage(FundAlarmConfig fundAlarmConfig)
                            {
                                if (!fundAlarmConfig.IsActive || !fundAlarmConfig.Condition.IsMatch(dateNowStr))
                                    return;

                                // 删除过期发送内容
                                DeleteExpiredData(setConfig.FundAlarmedList, dateNow);
                                // 发送基金播报消息
                                await SendFundAlarmMessageAsync(setConfig, fundAlarmConfig, dateNow).ConfigureAwait(false);
                            }

                            // MEMO : 0.13.3.15 禁用基金阈值观测(API不可用了)
                            //setConfig.FundLimitObserveConfigs.ToValueList().ForEach(SendFundLimitMessage);
                            //async void SendFundLimitMessage(FundLimitObserveConfig fundLimitObserveConfig)
                            //{
                            //    if (!fundLimitObserveConfig.IsActive || !fundLimitObserveConfig.Condition.IsMatch(dateNowStr))
                            //        return;

                            //    // 删除过期发送内容
                            //    DeleteExpiredData(setConfig.FundLimitObservedList, dateNow);
                            //    // 发送基金阈值观测消息
                            //    await SendFundLimitMessageAsync(setConfig, fundLimitObserveConfig, dateNow).ConfigureAwait(false);
                            //}
                        });

                    CommonExtensions.SleepSeconds(30);
                }
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(e);
            }

            CommonExtensions.Sleep(5000);
        }
    }

    /// <summary>
    /// 删除过期发送内容
    /// </summary>
    private static void DeleteExpiredData<T>(
        IDictionary<T, DateTime> dateTimelist,
        DateTime now,
        int totalSecond = 120)
        where T : unmanaged
    {
        var deleteKeys = dateTimelist
            .Where(each => (now - each.Value).TotalSeconds > totalSecond)
            .Select(each => each.Key)
            .ToArray();
        deleteKeys.ForEach(each => dateTimelist.Remove(each));
    }

    /// <summary>
    /// 发送基金红绿播报消息
    /// </summary>
    public static async Task SendFundAlarmMessageAsync(
        SetConfig setConfig,
        FundAlarmConfig fundAlarmConfig,
        DateTime now,
        bool forceSend = false)
    {
        try
        {
            var configId = fundAlarmConfig.Id;
            if (!forceSend && setConfig.FundAlarmedList.ContainsKey(configId))
                return;

            var alarmFundConfigs = fundAlarmConfig.AlarmFundConfigs;
            var fundIds = alarmFundConfigs.Values
                .Where(each => each.IsActive)
                .Select(each => each.FundId)
                .ToArray();
            ConcurrentHashSet<FundData> fundDatas = null;
            if (!await FUND_MAX_TRYTIMES.TryTimesAsync(async () =>
            {
                fundDatas = await FundExtensions.GetFundDataAsync(fundIds).ConfigureAwait(false);
                return fundDatas?.Any() == true;
            }).ConfigureAwait(false))
            {
                return;
            }

            var sendMessage = FundExtensions.GetFundAlarmString(fundDatas, alarmFundConfigs);
            if (string.IsNullOrEmpty(sendMessage))
                return;

            var targetId = setConfig.TargetId;
            switch (setConfig.TargetType)
            {
                case BotConfigTargetType.Group:
                    await BotServer.SendGroupMessageAsync(targetId, sendMessage, Vm.SetConfigs).ConfigureAwait(false);
                    LogExtensions.AddRunLog(new RunLog_FundHelper(BotConfigTargetType.Group, targetId, sendMessage));
                    break;
                case BotConfigTargetType.Private:
                    await BotServer.SendPrivateMessageAsync(targetId, sendMessage).ConfigureAwait(false);
                    LogExtensions.AddRunLog(new RunLog_FundHelper(BotConfigTargetType.Private, targetId, sendMessage));
                    break;
                case BotConfigTargetType.Common:
                default:
                    throw new ArgumentOutOfRangeException(setConfig.TargetType.ToString());
            }

            // MEMO : 追加到已发送列表
            if (!forceSend)
            {
                setConfig.FundAlarmedList.AddOrUpdate(
                    configId,
                    now,
                    (_, __) => now);
            }
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteLog(e);
        }
    }

    /// <summary>
    /// 发送基金阈值观测消息
    /// </summary>
    public static async Task SendFundLimitMessageAsync(
        SetConfig setConfig,
        FundLimitObserveConfig fundLimitObserveConfig,
        DateTime now,
        bool forceSend = false)
    {
        var configId = fundLimitObserveConfig.Id;
        if (!forceSend && setConfig.FundLimitObservedList.ContainsKey(configId))
            return;

        var activeFundLimitObserveConfigs = fundLimitObserveConfig
            .LimitObserveFundConfigs.Values
            .Where(each => each.IsActive)
            .ToArray();
        var fundIds = activeFundLimitObserveConfigs
            .Select(each => each.FundId)
            .Distinct()
            .ToArray();

        ConcurrentHashSet<FundData> fundDatas = null;
        if (!await FUND_MAX_TRYTIMES.TryTimesAsync(async () =>
            {
                fundDatas = await FundExtensions.GetFundDataAsync(fundIds).ConfigureAwait(false);
                return fundDatas?.Any() == true;
            }).ConfigureAwait(false))
        {
            return;
        }

        var sendMessage = FundExtensions.GetFundLimitString(fundDatas, activeFundLimitObserveConfigs);
        if (string.IsNullOrEmpty(sendMessage))
            return;

        var targetId = setConfig.TargetId;
        switch (setConfig.TargetType)
        {
            case BotConfigTargetType.Group:
                await BotServer.SendGroupMessageAsync(targetId, sendMessage, Vm.SetConfigs).ConfigureAwait(false);
                LogExtensions.AddRunLog(new RunLog_FundHelper(BotConfigTargetType.Group, targetId, sendMessage));
                break;
            case BotConfigTargetType.Private:
                await BotServer.SendPrivateMessageAsync(targetId, sendMessage).ConfigureAwait(false);
                LogExtensions.AddRunLog(new RunLog_FundHelper(BotConfigTargetType.Private, targetId, sendMessage));
                break;
            case BotConfigTargetType.Common:
            default:
                throw new ArgumentOutOfRangeException(setConfig.TargetType.ToString());
        }

        // MEMO : 追加到已发送列表
        if (!forceSend)
        {
            setConfig.FundLimitObservedList.AddOrUpdate(
                configId,
                now,
                (_, __) => now);
        }
    }
}