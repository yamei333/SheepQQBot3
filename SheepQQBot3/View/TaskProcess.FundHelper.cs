using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.Fund;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    public static partial class TaskProcess
    {
        public const int FUND_MAX_TRYTIMES = 5;

        /// <summary>
        /// 基金助手 (播报/阈值监控)
        /// </summary>
        public static void FundHelper()
        {
            AddRunLog(new RunLog_SystemInfo("基金助手 模块已运行"));
            while (true)
            {
                try
                {
                    if (Api?.IsConnected == true)
                    {
                        var dateNow = DateTime.Now;
                        var dateNowStr = dateNow.ToConditionString(HolidayInfo);
                        Vm.SetConfigs?.Values
                            .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Group_FundHelper))
                            .ForEach(setConfig =>
                            {
                                setConfig.FundAlarmConfigs.ToValueList().ForEach(DeleteExpiredDataAction);
                                async void DeleteExpiredDataAction(FundAlarmConfig fundAlarmConfig)
                                {
                                    if (!fundAlarmConfig.IsActive || !fundAlarmConfig.Condition.IsMatch(dateNowStr))
                                        return;

                                    // 删除过期发送内容
                                    DeleteExpiredData(setConfig.FundAlarmedList, dateNow);
                                    // 发送基金播报消息
                                    await SendFundAlarmMessage(setConfig, fundAlarmConfig, dateNow);
                                }

                                setConfig.FundLimitObserveConfigs.ToValueList().ForEach(DeleteExpiredDataAction2);
                                async void DeleteExpiredDataAction2(FundLimitObserveConfig fundLimitObserveConfig)
                                {
                                    if (!fundLimitObserveConfig.IsActive || !fundLimitObserveConfig.Condition.IsMatch(dateNowStr)) return;

                                    // 删除过期发送内容
                                    DeleteExpiredData(setConfig.FundLimitObservedList, dateNow);
                                    // 发送基金阈值观测消息
                                    await SendFundLimitMessage(setConfig, fundLimitObserveConfig, dateNow);
                                }
                            });
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
        public static async Task SendFundAlarmMessage(
            SetConfig setConfig,
            FundAlarmConfig fundAlarmConfig,
            DateTime now,
            bool forceSend = false)
        {
            var configId = fundAlarmConfig.Id;
            if (!forceSend && setConfig.FundAlarmedList.ContainsKey(configId))
                return;

            var alarmFundConfigs = fundAlarmConfig.AlarmFundConfigs;
            var fundIds = alarmFundConfigs.Values
                .Where(each => each.IsActive)
                .Select(each => each.FundId)
                .ToArray();
            FundData fundInfo = null;
            if (!await FUND_MAX_TRYTIMES.TryTimesAsync(async () =>
            {
                fundInfo = await FundExtensions.GetFundDataAsync(fundIds);
                return fundInfo != null;
            }))
            {
                return;
            }

            var sendMessage = FundExtensions.GetFundAlarmString(fundInfo, alarmFundConfigs);
            if (string.IsNullOrEmpty(sendMessage))
                return;

            var targetId = setConfig.TargetId;
            switch (setConfig.TargetType)
            {
                case BotConfigTargetType.Group:
                    await Api.SendGroupMessage(targetId, sendMessage, Vm.SetConfigs);
                    LogExtensions.AddRunLog(new RunLog_FundHelper(BotConfigTargetType.Group, targetId, sendMessage));
                    break;
                case BotConfigTargetType.Private:
                    await Api.SendPrivateMessage(targetId, sendMessage);
                    LogExtensions.AddRunLog(new RunLog_FundHelper(BotConfigTargetType.Private, targetId, sendMessage));
                    break;
                case BotConfigTargetType.Common:
                default:
                    throw new ArgumentOutOfRangeException(
                        $"{nameof(SendFundAlarmMessage)}.{nameof(setConfig.TargetType)}",
                        setConfig.TargetType.ToString());
            }

            // MEMO : 追加到已发送列表
            if (!forceSend)
                setConfig.FundAlarmedList.Add(configId, now);
        }

        /// <summary>
        /// 发送基金阈值观测消息
        /// </summary>
        public static async Task SendFundLimitMessage(
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

            FundData fundInfo = null;
            if (!await FUND_MAX_TRYTIMES.TryTimesAsync(async () =>
                {
                    fundInfo = await FundExtensions.GetFundDataAsync(fundIds);
                    return fundInfo != null;
                }))
            {
                return;
            }

            var sendMessage = FundExtensions.GetFundLimitString(fundInfo, activeFundLimitObserveConfigs);
            if (string.IsNullOrEmpty(sendMessage))
                return;

            var targetId = setConfig.TargetId;
            switch (setConfig.TargetType)
            {
                case BotConfigTargetType.Group:
                    await Api.SendGroupMessage(targetId, sendMessage, Vm.SetConfigs);
                    LogExtensions.AddRunLog(new RunLog_FundHelper(BotConfigTargetType.Group, targetId, sendMessage));
                    break;
                case BotConfigTargetType.Private:
                    await Api.SendPrivateMessage(targetId, sendMessage);
                    LogExtensions.AddRunLog(new RunLog_FundHelper(BotConfigTargetType.Private, targetId, sendMessage));
                    break;
                case BotConfigTargetType.Common:
                default:
                    throw new ArgumentOutOfRangeException(
                        $"{nameof(SendFundLimitMessage)}.{nameof(setConfig.TargetType)}",
                        setConfig.TargetType.ToString());
            }

            // MEMO : 追加到已发送列表
            if (!forceSend)
                setConfig.FundLimitObservedList.Add(configId, now);
        }
    }
}