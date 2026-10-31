using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Masuit.Tools;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.LiveAlarm;
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

    private static Task Monitor_OnLiveStopAsync(
        string title,
        string userName,
        string userFace,
        TimeSpan duration,
        BotConfigTargetType targetType,
        string targetId)
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

    private static Task Monitor_OnLiveStartAsync(
        string title,
        string roomUrl,
        string userName,
        string userFace,
        string keyFrame,
        BotConfigTargetType targetType,
        string targetId)
    {
        // MEMO : 0.16.7.0 直播提示通知改为使用 SnowLuma 的 SendGroupTuwenAsync
        var summary = $"[直播] {userName}";
        var logMessage = $"{summary} - {title}";
        switch (targetType)
        {
            case BotConfigTargetType.Group:
                GlobalBotClient.SendGroupTuwenAsync(targetId, summary, title, summary, userFace, roomUrl).ConfigureAwait(false);
                AddRunLog(new RunLog_LiveAlarm(BotConfigTargetType.Group, userName, targetId, logMessage));
                break;
            case BotConfigTargetType.Private:
                GlobalBotClient.SendPrivateTuwenAsync(targetId, summary, title, summary, userFace, roomUrl).ConfigureAwait(false);
                AddRunLog(new RunLog_LiveAlarm(BotConfigTargetType.Private, userName, targetId, logMessage));
                break;
            case BotConfigTargetType.Common:
            default:
                throw new ArgumentOutOfRangeException(
                    $"{nameof(Monitor_OnLiveStartAsync)}.{nameof(targetType)}",
                    targetType.ToString());
        }

        return Task.CompletedTask;
    }
}