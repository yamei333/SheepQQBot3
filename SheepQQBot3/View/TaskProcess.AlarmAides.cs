using System;
using System.Linq;
using System.Threading.Tasks;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    public static partial class TaskProcess
    {
        /// <summary>
        /// 闹钟助手
        /// </summary>
        public static void AlarmAides()
        {
            //AddRunLog(new RunLog_SystemInfo("闹钟助手 已开启"));
            while (true)
            {
                if (Api?.IsConnected == true)
                {
                    var dateNow = DateTime.Now;
                    var dateNowStr = dateNow.ToConditionString(HolidayInfo);
                    Vm.SetConfigs?.Values
                        .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Common_AlarmAide))
                        .ForEach(setConfig =>
                        {
                            setConfig.AlarmAideConfigs.ToValueList().ForEach(DeleteExpiredDataAction);
                            async void DeleteExpiredDataAction(AlarmAideConfig alarmAidesConfig)
                            {
                                if (!alarmAidesConfig.IsActive || !alarmAidesConfig.Condition.IsMatch(dateNowStr))
                                    return;

                                // 删除过期发送内容
                                DeleteExpiredData(setConfig.AlarmAideAlarmedList, dateNow);
                                // 发送闹钟助手消息
                                await SendAlarm(setConfig, alarmAidesConfig, dateNow);
                            }
                        });
                }

                CommonExtensions.Sleep(1000);
            }
        }

        /// <summary>
        /// 发送闹钟助手消息
        /// </summary>
        private static async Task SendAlarm(SetConfig setConfig, AlarmAideConfig alarmAideConfig, DateTime now)
        {
            var alarmInfoKey = alarmAideConfig.Id;
            if (setConfig.AlarmAideAlarmedList.ContainsKey(alarmInfoKey))
                return;

            var alarmTexts = alarmAideConfig.AlarmTexts;
            if (!alarmTexts.IsEmpty)
            {
                var alarmText = alarmTexts.Values.Random();
                var targetId = setConfig.TargetId;
                switch (setConfig.TargetType)
                {
                    case BotConfigTargetType.Group:
                        await Api.SendGroupMessage(targetId, alarmText, Vm.SetConfigs);
                        AddRunLog(new RunLog_AlarmAide(BotConfigTargetType.Group, targetId, alarmText));
                        break;
                    case BotConfigTargetType.Private:
                        await Api.SendPrivateMessage(targetId, alarmText);
                        AddRunLog(new RunLog_AlarmAide(BotConfigTargetType.Private, targetId, alarmText));
                        break;
                    case BotConfigTargetType.Common:
                    default:
                        throw new ArgumentOutOfRangeException(
                            $"{nameof(SendAlarm)}.{nameof(setConfig.TargetType)}",
                            setConfig.TargetType.ToString());
                }

                // MEMO : 追加到已发送列表
                setConfig.AlarmAideAlarmedList.Add(alarmInfoKey, now);
            }
        }
    }
}