using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.Fund;
using System;
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
                            setConfig.FundAlarmConfigs.Values.ForEach(SendFundAlarmMessage);
                            return;

                            async void SendFundAlarmMessage(FundAlarmConfig fundAlarmConfig)
                            {
                                if (!fundAlarmConfig.IsActive || !fundAlarmConfig.Condition.IsMatch(dateNowStr))
                                    return;

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
            if ((now - fundAlarmConfig.LastExecuteDate).TotalSeconds <= MIN_REPEAT_EXECUTE_SECONDS && !forceSend)
                return;

            // MEMO : 设定执行时间
            if (!forceSend)
                fundAlarmConfig.LastExecuteDate = now;

            var alarmFundConfigs = fundAlarmConfig.AlarmFundConfigs;
            var fundIds = alarmFundConfigs.Values
                .Where(each => each.IsActive)
                .Select(each => each.FundId);
            var fundDatas = Array.Empty<FundData>();
            if (!await FUND_MAX_TRYTIMES.TryTimesAsync(async () =>
            {
                fundDatas = await FundExtensions.GetFundDatasAsync(fundIds).ConfigureAwait(false);
                return fundDatas?.Any(each => each != null) == true;
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
                    await BotClient.SendGroupMessageAsync(targetId, sendMessage, Vm.SetConfigs).ConfigureAwait(false);
                    AddRunLog(new RunLog_FundHelper(BotConfigTargetType.Group, targetId, sendMessage));
                    break;
                case BotConfigTargetType.Private:
                    await BotClient.SendPrivateMessageAsync(targetId, sendMessage).ConfigureAwait(false);
                    AddRunLog(new RunLog_FundHelper(BotConfigTargetType.Private, targetId, sendMessage));
                    break;
                case BotConfigTargetType.Common:
                default:
                    throw new ArgumentOutOfRangeException(setConfig.TargetType.ToString());
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
        if ((now - fundLimitObserveConfig.LastExecuteDate).TotalSeconds <= MIN_REPEAT_EXECUTE_SECONDS && !forceSend)
            return;

        // MEMO : 设定执行时间
        fundLimitObserveConfig.LastExecuteDate = now;
        var activeFundLimitObserveConfigs = fundLimitObserveConfig
            .LimitObserveFundConfigs.Values
            .Where(each => each.IsActive)
            .ToArray();
        var fundIds = activeFundLimitObserveConfigs
            .Select(each => each.FundId)
            .Distinct()
            .ToArray();

        FundData[] fundDatas = null;
        if (!await FUND_MAX_TRYTIMES.TryTimesAsync(async () =>
            {
                fundDatas = await FundExtensions.GetFundDatasAsync(fundIds).ConfigureAwait(false);
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
                await BotClient.SendGroupMessageAsync(targetId, sendMessage, Vm.SetConfigs).ConfigureAwait(false);
                LogExtensions.AddRunLog(new RunLog_FundHelper(BotConfigTargetType.Group, targetId, sendMessage));
                break;
            case BotConfigTargetType.Private:
                await BotClient.SendPrivateMessageAsync(targetId, sendMessage).ConfigureAwait(false);
                LogExtensions.AddRunLog(new RunLog_FundHelper(BotConfigTargetType.Private, targetId, sendMessage));
                break;
            case BotConfigTargetType.Common:
            default:
                throw new ArgumentOutOfRangeException(setConfig.TargetType.ToString());
        }
    }
}