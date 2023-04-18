using System;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.LiveAlarm;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View;

public static partial class TaskProcess
{
    /// <summary>
    /// 直播提醒
    /// </summary>
    public static void LiveAlarm()
    {
        AddRunLog(new RunLog_SystemInfo("直播提醒 模块已运行"));
        while (true)
        {
            try
            {
                if (Api?.IsConnected == true)
                {
                    var dateNow = DateTime.Now;
                    Vm.SetConfigs?.Values
                        .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Group_LiveAlarm))
                        .ForEach(setConfig =>
                        {
                            setConfig.LiveAlarmConfigs?.ToValueList().ForEach(DeleteExpiredDataAction);
                            async void DeleteExpiredDataAction(LiveAlarmConfig liveAlarmConfig)
                            {
                                if (!liveAlarmConfig.IsActive)
                                    return;

                                // 删除过期记录
                                DeleteExpiredData(setConfig.LiveAlarmedList, dateNow);
                                // 发送直播提醒消息
                                await SendLiveAlarmMessage(setConfig, liveAlarmConfig, dateNow).ConfigureAwait(false);
                            }
                        });
                }
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(e);
            }

            CommonExtensions.Sleep(30000);
        }
    }

    /// <summary>
    /// 发送直播提醒消息
    /// </summary>
    public static async Task SendLiveAlarmMessage(
        SetConfig setConfig,
        LiveAlarmConfig liveAlarmConfig,
        DateTime now,
        bool forceSend = false)
    {
        var configId = liveAlarmConfig.Id;
        if (!forceSend && setConfig.LiveAlarmedList.ContainsKey(configId))
            return;

        //var liveType = liveAlarmConfig.LiveType;
        var liveRoomId = liveAlarmConfig.LiveRoomId;
        var liveRoomResponse = await HttpExtensions.GetFromJsonAsync<LiveRoomResponse>(
            $"https://api.live.bilibili.com/xlive/web-room/v1/index/getInfoByRoom?room_id={liveRoomId}");
        if (liveRoomResponse == null)
            return;

        var liveRoomResponseData = liveRoomResponse.Data;
        if (liveRoomResponseData.RoomInfo.LiveStatusType != LiveStatusType.Live)
            return;

        var startTime = liveRoomResponse.Data.RoomInfo.LiveStartTime.ToDateTime();
        if ((DateTime.Now - startTime).TotalSeconds > 90)
            return;

        var sendMessage = $"[{liveRoomResponseData.AnchorInfo.UserBaseInfo.Name}]正在直播-{liveRoomResponseData.RoomInfo.Title}" +
                          $"{ENTER}赶紧加入观看吧: https://live.bilibili.com/{liveRoomId}";

        var targetId = setConfig.TargetId;
        switch (setConfig.TargetType)
        {
            case BotConfigTargetType.Group:
                await Api.SendGroupMessageAsync(targetId, sendMessage, Vm.SetConfigs);
                LogExtensions.AddRunLog(new RunLog_LiveAlarm(BotConfigTargetType.Group, liveRoomId.ToString(), targetId, sendMessage));
                break;
            case BotConfigTargetType.Private:
                await Api.SendPrivateMessageAsync(targetId, sendMessage);
                LogExtensions.AddRunLog(new RunLog_LiveAlarm(BotConfigTargetType.Private, liveRoomId.ToString(), targetId, sendMessage));
                break;
            case BotConfigTargetType.Common:
            default:
                throw new ArgumentOutOfRangeException(
                    $"{nameof(SendLiveAlarmMessage)}.{nameof(setConfig.TargetType)}",
                    setConfig.TargetType.ToString());
        }

        // MEMO : 追加到已发送列表
        if (!forceSend)
            setConfig.LiveAlarmedList.Add(configId, now);
    }
}