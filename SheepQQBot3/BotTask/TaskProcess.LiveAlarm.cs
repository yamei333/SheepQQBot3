using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.LiveAlarm;
using System;
using System.Linq;
using System.Threading.Tasks;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotTask;

public static partial class TaskProcess
{
    /// <summary>
    /// 直播提醒
    /// </summary>
    public static void LiveAlarm()
    {
        AddTaskRunLog("直播提醒");
        while (true)
        {
            try
            {
                if (BotServer?.Connected == true)
                {
                    var dateNow = DateTime.Now;
                    Vm.SetConfigs?.Values
                        .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Group_LiveAlarm))
                        .ForEach(setConfig =>
                        {
                            setConfig.LiveAlarmConfigs?.ToValueList().ForEach(DeleteExpiredDataAction);
                            return;

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

        var liveRoomId = liveAlarmConfig.LiveRoomId;
        var httpResponse = await HttpExtensions.GetFromJsonAsync<LiveRoomResponse>(
                $"https://api.live.bilibili.com/xlive/web-room/v1/index/getInfoByRoom?room_id={liveRoomId}")
            .ConfigureAwait(false);
        if (httpResponse.Result != HttpResponseResult.Successed)
            return;

        var liveRoomResponse = httpResponse.Data;
        if (liveRoomResponse == null)
            return;

        var liveRoomResponseData = liveRoomResponse.Data;
        if (liveRoomResponseData.RoomInfo.LiveStatusType != LiveStatusType.Live)
            return;

        var startTime = liveRoomResponse.Data.RoomInfo.LiveStartTime.ToDateTime();
        if ((DateTime.Now - startTime).TotalSeconds > 90)
            return;

        var roomInfo = liveRoomResponseData.RoomInfo;
        var userBaseInfo = liveRoomResponseData.AnchorInfo.UserBaseInfo;
        var sendMessage = await CQCode.JsonCard_TianxuanShareAsync(
            "正在直播!",
            $"主播:{userBaseInfo.Name}{ENTER}标题:{roomInfo.Title}",
            $"https://live.bilibili.com/{liveRoomId}",
            userBaseInfo.Face)
            .ConfigureAwait(false);
        //var sendMessage = $"[{liveRoomResponseData.AnchorInfo.UserBaseInfo.Name}]正在直播-{liveRoomResponseData.RoomInfo.Title}" +
        //                  $"{ENTER}赶紧加入观看吧: https://live.bilibili.com/{liveRoomId}";

        var targetId = setConfig.TargetId;
        switch (setConfig.TargetType)
        {
            case BotConfigTargetType.Group:
                await BotServer.SendGroupMessageAsync(targetId, sendMessage, Vm.SetConfigs).ConfigureAwait(false);
                LogExtensions.AddRunLog(new RunLog_LiveAlarm(BotConfigTargetType.Group, liveRoomId.ToString(), targetId, sendMessage));
                break;
            case BotConfigTargetType.Private:
                await BotServer.SendPrivateMessageAsync(targetId, sendMessage).ConfigureAwait(false);
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