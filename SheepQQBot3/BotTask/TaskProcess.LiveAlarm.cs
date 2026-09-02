using Masuit.Tools;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.LiveAlarm;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        Vm.SetConfigs?.Values
            .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Group_LiveAlarm))
            .ForEach(setConfig =>
            {
                var groupId = setConfig.TargetId;
                var targetType = setConfig.TargetType;
                setConfig.LiveAlarmConfigs.Values
                    .Where(each => each.IsActive)
                    .ForEach(each =>
                    {
                        var monitor = new BilibiliLiveMonitor(each.LiveRoomId.ToString(), targetType, groupId);
                        monitor.OnLiveStart += Monitor_OnLiveStartAsync;
                        monitor.OnLiveStop += Monitor_OnLiveStopAsync;
                        monitor.OnInitCompleted += Monitor_OnInitCompletedAsync;
                        monitor.OnError += Monitor_OnErrorAsync;
                        monitor.Start(CancellationToken.None);
                    });
            });

        //while (true)
        //{
        //    try
        //    {
        //        if (BotServer?.Connected == true)
        //        {
        //            var dateNow = DateTime.Now;
        //            Vm.SetConfigs?.Values
        //                .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Group_LiveAlarm))
        //                .ForEach(setConfig =>
        //                {
        //                    var groupId = setConfig.TargetId;
        //                    setConfig.LiveAlarmConfigs?.ToValueList()
        //                        .Where(each => each.IsActive)
        //                        .ForeachAsync(SendAction);
        //                    return;

        //                    async Task SendAction(LiveAlarmConfig liveAlarmConfig)
        //                    {
        //                        var monitor = new BilibiliLiveMonitor(liveAlarmConfig.LiveRoomId.ToString(), groupId);
        //                        monitor.OnLiveStart += Monitor_OnLiveStart;
        //                        monitor.Start(CancellationToken.None);

        //                        //// 发送直播提醒消息
        //                        //await SendLiveAlarmMessageAsync(setConfig, liveAlarmConfig, dateNow).ConfigureAwait(false);
        //                    }
        //                });
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        YameiLogExtensions.WriteLog(e);
        //    }

        //    CommonExtensions.Sleep(30000);
        //}
    }

    private static Task Monitor_OnErrorAsync(string roomId, string errorMessage)
    {
        AddRunLog(new RunLog_LiveAlarm(BotConfigTargetType.Group, "直播监控", roomId, $"错误原因: {errorMessage}"));
        return Task.CompletedTask;
    }

    private static Task Monitor_OnInitCompletedAsync(string roomId, string userName, bool isLive)
    {
        AddRunLog(new RunLog_LiveAlarm(BotConfigTargetType.Group, userName, roomId, $"当前直播状态: {(isLive ? "直播中" : "摸鱼")}"));
        return Task.CompletedTask;
    }

    private static Task Monitor_OnLiveStopAsync(string title, string userName, string userFace, TimeSpan duration, BotConfigTargetType targetType, string targetId)
    {
        var timeStr = "";
        if (duration.TotalHours >= 1)
            timeStr += $"{(int)duration.TotalHours}小时";

        timeStr += $"{duration.Minutes}分";
        var sendMessage = $"[{userName}]关闭了直播!!{ENTER}本次直播时长: {timeStr}{ENTER}几点了就下播? 全给他摸完了!";
        switch (targetType)
        {
            case BotConfigTargetType.Group:
                GlobalBotClient.SendGroupMessageAsync(targetId, sendMessage, Vm.SetConfigs).ConfigureAwait(false);
                AddRunLog(new RunLog_LiveAlarm(BotConfigTargetType.Group, userName, targetId, sendMessage));
                break;
            case BotConfigTargetType.Private:
                GlobalBotClient.SendPrivateMessageAsync(targetId, sendMessage).ConfigureAwait(false);
                AddRunLog(new RunLog_LiveAlarm(BotConfigTargetType.Private, userName, targetId, sendMessage));
                break;
            case BotConfigTargetType.Common:
            default:
                throw new ArgumentOutOfRangeException(
                    $"{nameof(Monitor_OnLiveStartAsync)}.{nameof(targetType)}",
                    targetType.ToString());
        }

        return Task.CompletedTask;
    }

    private static Task Monitor_OnLiveStartAsync(string title, string roomUrl, string userName, string userFace, string keyFrame, BotConfigTargetType targetType, string targetId)
    {
        var sendMessage = CQCode.CustomMusic(roomUrl, roomUrl, $"[开播] {userName}", userFace, title);
        switch (targetType)
        {
            case BotConfigTargetType.Group:
                GlobalBotClient.SendGroupMessageAsync(targetId, sendMessage, Vm.SetConfigs).ConfigureAwait(false);
                AddRunLog(new RunLog_LiveAlarm(BotConfigTargetType.Group, userName, targetId, sendMessage));
                break;
            case BotConfigTargetType.Private:
                GlobalBotClient.SendPrivateMessageAsync(targetId, sendMessage).ConfigureAwait(false);
                AddRunLog(new RunLog_LiveAlarm(BotConfigTargetType.Private, userName, targetId, sendMessage));
                break;
            case BotConfigTargetType.Common:
            default:
                throw new ArgumentOutOfRangeException(
                    $"{nameof(Monitor_OnLiveStartAsync)}.{nameof(targetType)}",
                    targetType.ToString());
        }

        return Task.CompletedTask;
    }

    ///// <summary>
    ///// 发送直播提醒消息
    ///// </summary>
    //public static async Task SendLiveAlarmMessageAsync(
    //    SetConfig setConfig,
    //    LiveAlarmConfig liveAlarmConfig,
    //    DateTime now,
    //    bool forceSend = false)
    //{
    //    try
    //    {
    //        if ((now - liveAlarmConfig.LastExecuteDate).TotalSeconds <= MIN_REPEAT_EXECUTE_SECONDS && !forceSend)
    //            return;

    //        // MEMO : 设定执行时间
    //        if (!forceSend)
    //            liveAlarmConfig.LastExecuteDate = now;

    //        var liveUserId = liveAlarmConfig.LiveRoomId;
    //        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://api.live.bilibili.com/room/v1/Room/get_status_info_by_uids?uids[]={liveUserId}");
    //        httpRequestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    //        httpRequestMessage.Headers.Add("Referer", "https://live.bilibili.com/");
    //        var httpResponse = await HttpExtensions.CreateHttpClient().SendAsync(httpRequestMessage).ConfigureAwait(false);
    //        if (httpResponse.StatusCode != HttpStatusCode.OK)
    //            return;

    //        var httpResponseData = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
    //        var jsonText = _regLiveRoomData.Match(httpResponseData).Value;
    //        if (string.IsNullOrEmpty(jsonText))
    //        {
    //            AddRunLog(new RunLog_SystemError($"B站直播提醒出错! 用户ID[{liveUserId}], 用户不存在"));
    //            return;
    //        }

    //        var liveRoomData = jsonText.FromJson<LiveRoomData>();
    //        if (liveRoomData == null)
    //        {
    //            AddRunLog(new RunLog_SystemError($"B站直播提醒出错! 用户ID[{liveUserId}], Json解析结果为null"));
    //            return;
    //        }

    //        if (liveRoomData.LiveStatusType != LiveStatusType.Live)
    //            return;

    //        var startTime = liveRoomData.LiveStartTime.ToDateTime();
    //        // MEMO : 开播超过90秒, 则不再提醒
    //        if ((DateTime.Now - startTime).TotalSeconds > 90)
    //            return;

    //        // MEMO : 0.14.9.4 修复直播提醒
    //        var sendMessage = CQCode.CustomMusic(
    //            $"https://live.bilibili.com/{liveRoomData.RoomId}",
    //            $"https://live.bilibili.com/{liveRoomData.RoomId}",
    //            $"{liveRoomData.Name}正在直播!",
    //            $"{liveRoomData.Face}",
    //            $"{liveRoomData.Title}");
    //        //var sendMessage = $"[{liveRoomResponseData.AnchorInfo.UserBaseInfo.Name}]正在直播-{liveRoomResponseData.RoomInfo.Title}"
    //        //        + $"{ENTER}赶紧加入观看吧: https://live.bilibili.com/{liveRoomId}";

    //        var targetId = setConfig.TargetId;
    //        switch (setConfig.TargetType)
    //        {
    //            case BotConfigTargetType.Group:
    //                await BotClient.SendGroupMessageAsync(targetId, sendMessage, Vm.SetConfigs).ConfigureAwait(false);
    //                AddRunLog(new RunLog_LiveAlarm(BotConfigTargetType.Group, liveUserId.ToString(), targetId, sendMessage));
    //                break;
    //            case BotConfigTargetType.Private:
    //                await BotClient.SendPrivateMessageAsync(targetId, sendMessage).ConfigureAwait(false);
    //                AddRunLog(new RunLog_LiveAlarm(BotConfigTargetType.Private, liveUserId.ToString(), targetId, sendMessage));
    //                break;
    //            case BotConfigTargetType.Common:
    //            default:
    //                throw new ArgumentOutOfRangeException(
    //                    $"{nameof(SendLiveAlarmMessageAsync)}.{nameof(setConfig.TargetType)}",
    //                    setConfig.TargetType.ToString());
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        YameiLogExtensions.WriteLog(e);
    //    }
    //}
}