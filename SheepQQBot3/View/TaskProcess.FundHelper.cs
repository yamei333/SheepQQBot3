using System;
using System.Collections.Generic;
using System.Linq;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using Yamei.Common;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    public static partial class TaskProcess
    {
        /// <summary>
        /// 基金助手 (播报/阈值监控)
        /// </summary>
        public static void FundHelper()
        {
            //AddRunLog(new RunLog_SystemInfo("基金助手 已开启"));
            while (true)
            {
                if (Api?.IsConnected == true)
                {
                    var dateNow = DateTime.Now;
                    var dateNowStr = dateNow.ToConditionString(HolidayInfo);
                    Vm.SetConfigs?.Values
                        .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Group_FundHelper))
                        .ForEach(setConfig =>
                        {
                            setConfig.FundAlarmConfigs.ToValueList().ForEach(fundAlarmConfig =>
                            {
                                if (fundAlarmConfig.IsActive
                                    && fundAlarmConfig.Condition.IsMatch(dateNowStr))
                                {
                                    // 删除过期发送内容
                                    DeleteExpiredData(setConfig.FundAlarmedList, dateNow);
                                    // 发送基金播报消息
                                    SendFundAlarmMessage(setConfig, fundAlarmConfig, dateNow);
                                }
                            });

                            setConfig.FundLimitObserveConfigs.ToValueList().ForEach(fundLimitObserveConfig =>
                            {
                                if (fundLimitObserveConfig.IsActive
                                    && fundLimitObserveConfig.Condition.IsMatch(dateNowStr))
                                {
                                    // 删除过期发送内容
                                    DeleteExpiredData(setConfig.FundLimitObservedList, dateNow);
                                    // 发送基金阈值观测消息
                                    SendFundLimitMessage(setConfig, fundLimitObserveConfig, dateNow);
                                }
                            });
                        });
                }

                CommonExtensions.Sleep(5000);
            }
        }

        /// <summary>
        /// 删除过期发送内容
        /// </summary>
        private static void DeleteExpiredData(
            IDictionary<Guid, DateTime> dateTimelist,
            DateTime now,
            int totalSecond = 120)
        {
            var dateTimes = dateTimelist
                .Where(each => (now - each.Value).TotalSeconds > totalSecond)
                .Select(each => each.Key)
                .ToArray();
            dateTimes.ForEach(each => dateTimelist.Remove(each));
        }

        /// <summary>
        /// 发送基金红绿播报消息
        /// </summary>
        public static void SendFundAlarmMessage(
            SetConfig setConfig,
            FundAlarmConfig fundAlarmConfig,
            DateTime now,
            bool forceSend = false)
        {
            var configId = fundAlarmConfig.ConfigId;
            if (!forceSend && setConfig.FundAlarmedList.ContainsKey(configId))
                return;

            var alarmFundConfigs = fundAlarmConfig.AlarmFundConfigs;
            var fundIds = alarmFundConfigs.Values
                .Where(each => each.IsActive)
                .Select(each => each.FundId)
                .ToArray();
            if (fundIds.Length == 0)
                return;

            var fundInfo = FundExtensions.GetFundData(fundIds);
            var sendMessage = FundExtensions.GetFundAlarmString(fundInfo, alarmFundConfigs);
            if (string.IsNullOrEmpty(sendMessage))
                return;

            var targetId = setConfig.TargetId;
            switch (setConfig.TargetType)
            {
                case BotConfigTargetType.Group:
                    Api.SendGroupMessage(targetId, sendMessage, Vm.SetConfigs);
                    LogExtensions.AddRunLog(new RunLog_FundHelper(BotConfigTargetType.Group, targetId, sendMessage));
                    break;
                case BotConfigTargetType.Private:
                    Api.SendPrivateMessage(targetId, sendMessage);
                    LogExtensions.AddRunLog(new RunLog_FundHelper(BotConfigTargetType.Private, targetId, sendMessage));
                    break;
                case BotConfigTargetType.Common:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // MEMO : 追加到已发送列表
            if (!forceSend)
                setConfig.FundAlarmedList.Add(configId, now);
        }

        /// <summary>
        /// 发送基金阈值观测消息
        /// </summary>
        public static void SendFundLimitMessage(
            SetConfig setConfig,
            FundLimitObserveConfig fundLimitObserveConfig,
            DateTime now,
            bool forceSend = false)
        {
            var configId = fundLimitObserveConfig.ConfigId;
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
            if (fundIds.Length == 0)
                return;

            var fundInfo = FundExtensions.GetFundData(fundIds);
            var sendMessage = FundExtensions.GetFundLimitString(fundInfo, activeFundLimitObserveConfigs);
            if (string.IsNullOrEmpty(sendMessage))
                return;

            var targetId = setConfig.TargetId;
            switch (setConfig.TargetType)
            {
                case BotConfigTargetType.Group:
                    Api.SendGroupMessage(targetId, sendMessage, Vm.SetConfigs);
                    LogExtensions.AddRunLog(new RunLog_FundHelper(BotConfigTargetType.Group, targetId, sendMessage));
                    break;
                case BotConfigTargetType.Private:
                    Api.SendPrivateMessage(targetId, sendMessage);
                    LogExtensions.AddRunLog(new RunLog_FundHelper(BotConfigTargetType.Private, targetId, sendMessage));
                    break;
                case BotConfigTargetType.Common:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // MEMO : 追加到已发送列表
            if (!forceSend)
                setConfig.FundLimitObservedList.Add(configId, now);
        }
    }
}