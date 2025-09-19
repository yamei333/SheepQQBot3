using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.LiveAlarm;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotTask;

public static partial class TaskProcess
{
    private static readonly Regex _regLiveRoomData = new(@"{\""title"".+?}", RegexOptions.IgnoreCase | RegexOptions.Multiline);

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
                                await SendLiveAlarmMessageAsync(setConfig, liveAlarmConfig, dateNow).ConfigureAwait(false);
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
    public static async Task SendLiveAlarmMessageAsync(
        SetConfig setConfig,
        LiveAlarmConfig liveAlarmConfig,
        DateTime now,
        bool forceSend = false)
    {
        try
        {
            var configId = liveAlarmConfig.Id;
            if (!forceSend && setConfig.LiveAlarmedList.ContainsKey(configId))
                return;

            var liveUserId = liveAlarmConfig.LiveRoomId;
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://api.live.bilibili.com/room/v1/Room/get_status_info_by_uids?uids[]={liveUserId}");
            httpRequestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36");
            var httpResponse = await HttpExtensions.CreateHttpClient().SendAsync(httpRequestMessage).ConfigureAwait(false);
            if (httpResponse.StatusCode != HttpStatusCode.OK)
                return;

            var httpResponseData = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var jsonText = _regLiveRoomData.Match(httpResponseData).Value;
            if (string.IsNullOrEmpty(jsonText))
            {
                AddRunLog(new RunLog_SystemError($"B站直播提醒出错! 用户ID[{liveUserId}], 用户不存在"));
                return;
            }

            var liveRoomData = jsonText.JsonDeserialize<LiveRoomData>();
            if (liveRoomData == null)
            {
                AddRunLog(new RunLog_SystemError($"B站直播提醒出错! 用户ID[{liveUserId}], Json解析结果为null"));
                return;
            }

            if (liveRoomData.LiveStatusType != LiveStatusType.Live)
                return;

            var startTime = liveRoomData.LiveStartTime.ToDateTime();
            // MEMO : 开播超过90秒, 则不再提醒
            if ((DateTime.Now - startTime).TotalSeconds > 90)
                return;

            // MEMO : 0.14.9.4 修复直播提醒
            var sendMessage = CQCode.CustomMusic(
                $"https://live.bilibili.com/{liveRoomData.RoomId}",
                $"https://live.bilibili.com/{liveRoomData.RoomId}",
                $"{liveRoomData.Name}正在直播!",
                $"{liveRoomData.Face}",
                $"{liveRoomData.Title}");
            //var sendMessage = $"[{liveRoomResponseData.AnchorInfo.UserBaseInfo.Name}]正在直播-{liveRoomResponseData.RoomInfo.Title}"
            //        + $"{ENTER}赶紧加入观看吧: https://live.bilibili.com/{liveRoomId}";

            var targetId = setConfig.TargetId;
            switch (setConfig.TargetType)
            {
                case BotConfigTargetType.Group:
                    await BotClient.SendGroupMessageAsync(targetId, sendMessage, Vm.SetConfigs).ConfigureAwait(false);
                    AddRunLog(new RunLog_LiveAlarm(BotConfigTargetType.Group, liveUserId.ToString(), targetId, sendMessage));
                    break;
                case BotConfigTargetType.Private:
                    await BotClient.SendPrivateMessageAsync(targetId, sendMessage).ConfigureAwait(false);
                    AddRunLog(new RunLog_LiveAlarm(BotConfigTargetType.Private, liveUserId.ToString(), targetId, sendMessage));
                    break;
                case BotConfigTargetType.Common:
                default:
                    throw new ArgumentOutOfRangeException(
                        $"{nameof(SendLiveAlarmMessageAsync)}.{nameof(setConfig.TargetType)}",
                        setConfig.TargetType.ToString());
            }

            // MEMO : 追加到已发送列表
            if (!forceSend)
                setConfig.LiveAlarmedList.Add(configId, now);
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteLog(e);
        }
    }
}