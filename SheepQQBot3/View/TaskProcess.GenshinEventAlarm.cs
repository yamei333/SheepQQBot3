using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using Yamei.Common;
using static SheepQQBot3.Extensions.LogExtensions;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View;

public static partial class TaskProcess
{
    private static readonly Regex _regGenshinEventAlarm = RegexGenerator.GenshinEventAlarm();

    /// <summary>
    /// 原神活动提醒
    /// </summary>
    public static async Task GenshinEventAlarmAsync()
    {
        AddTaskRunLog("原神活动提醒");

        const string KEYWORD_EVENT_DOUBLE = "移涌";
        //const string KEYWORD_VERSIONUPDATE = "版本更新说明";

        var ignoreEventKeywords = new[]
        {
            "纪行", "传说任务说明", "限时上架", "限时折扣"
        };

        var ignoreAnnouncementKeywords = new[]
        {
            "展示页", "周边", "米游社原神区", "调研问卷", "游戏常用FAQ", "交流平台", "手办", "印象服饰", "专属工具更新"
        };

        while (true)
        {
            try
            {
                if (Api?.IsConnected == true)
                {
                    var dateNow = DateTime.Now;
                    var isAlarm = false;
                    var (alarmEvents, alarmAnns) = await GetNeedAlarmEventAsync().ConfigureAwait(false);

                    if ((dateNow.Hour >= 17 || dateNow.Hour < 17 && dateNow.IsHoliday(HolidayInfo))
                        && alarmEvents.Any() && _regGenshinEventAlarm.IsMatch(dateNow.ToConditionString(HolidayInfo)))
                    {
                        isAlarm = true;
                        var sendMessage = string.Empty;
                        alarmEvents.ForEach(each =>
                        {
                            if (each.Title.Contains(KEYWORD_EVENT_DOUBLE))
                                sendMessage += $"\r\n注意今天有{each.SubTitle}, 小便宜必须占!";
                            else
                                sendMessage += $"\r\n{each.SubTitle}, 只剩 {each.GetDaysRemain(dateNow)} 天不到了!";
                        });
                        sendMessage = $"{CQCode.AtAll()}[原神活动提醒]{sendMessage}";
                        Vm.SetConfigs?.Values
                            .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Group_GenshinHelper))
                            .ForEach(ToAction);

                        async void ToAction(SetConfig setConfig)
                        {
                            var targetId = setConfig.TargetId;
                            await Api.SendGroupMessageAsync(targetId, sendMessage, Vm.SetConfigs).ConfigureAwait(false);
                            AddRunLog(new RunLog_GenshinDailyNoteAlarm(BotConfigTargetType.Group, targetId, sendMessage));
                        }
                    }

                    if (alarmAnns.Any())
                    {
                        isAlarm = true;
                        var sendMessage = string.Empty;
                        //if (alarmAnns.Any(each => each.SubTitle.Contains(KEYWORD_VERSIONUPDATE)))
                        //{
                        //    sendMessage = $"{CQCode.AtAll()}[原神mys兑换提醒]\r\n今天更新, 晚上第一波双兑换, 有jo本的记得开着";
                        //}
                        //else
                        //{
                        alarmAnns.ForEach(each => sendMessage += $"\r\n{each.SubTitle}");
                        sendMessage = $"{CQCode.AtAll()}[原神辣鸡页游提醒]{sendMessage}";
                        //}
                        Vm.SetConfigs?.Values
                            .Where(each => each.BotFunctions.IsUsed(BotFunctionType.Group_GenshinHelper))
                            .ForEach(ToAction);

                        async void ToAction(SetConfig setConfig)
                        {
                            var targetId = setConfig.TargetId;
                            await Api.SendGroupMessageAsync(targetId, sendMessage, Vm.SetConfigs).ConfigureAwait(false);
                            AddRunLog(new RunLog_GenshinDailyNoteAlarm(BotConfigTargetType.Group, targetId, sendMessage));
                        }
                    }

                    CommonExtensions.SleepSeconds(isAlarm ? 210 : 60);

                    async Task<(List<GenshinEvent>, List<GenshinEvent>)> GetNeedAlarmEventAsync()
                    {
                        var needAlarmEvents = new List<GenshinEvent>();
                        var needAlarmAnnouncements = new List<GenshinEvent>();
                        var httpResponse = await HttpExtensions.GetFromJsonAsync<GenshinEventResponse>(
                            "https://hk4e-api.mihoyo.com/common/hk4e_cn/announcement/api/getAnnList?game=hk4e&game_biz=hk4e_cn&lang=zh-cn&bundle_id=hk4e_cn&platform=pc&region=cn_gf01&level=55&uid=100000000")
                            .ConfigureAwait(false);
                        if (httpResponse.Result != HttpResponseResult.Successed)
                            return (needAlarmEvents, needAlarmAnnouncements);

                        var genshinEventInfoList = httpResponse.Data?.Data.List;
                        if (genshinEventInfoList == null)
                            return (needAlarmEvents, needAlarmAnnouncements);

                        var genshinGameEvents = genshinEventInfoList.First(each => each.TypeId == 1).List;
                        var genshinGameAnnouncements = genshinEventInfoList.First(each => each.TypeId == 2).List;
                        genshinGameEvents
                            .Where(each => each.TagLabel == "活动"
                                && !each.Title.ContainsAny(ignoreEventKeywords)
                                && ((each.EndTime - dateNow).TotalHours is >= 0 and <= 72
                                    || (each.Title.Contains(KEYWORD_EVENT_DOUBLE) && dateNow > each.BeginTime && dateNow < each.EndTime)))
                            .ForEach(needAlarmEvents.Add);
                        genshinGameAnnouncements
                            .Where(each =>
                            {
                                return false;
                                //var elapsedMinutes = (dateNow - each.BeginTime).TotalMinutes;
                                //return (each.SubTitle.Contains(KEYWORD_VERSIONUPDATE)
                                //    && elapsedMinutes is >= 60 * 11 + 30 and <= 60 * 11 + 33);

                                //|| (each.TagLabel == "活动"
                                //    && !each.Title.ContainsAny(ignoreAnnouncementKeywords)
                                //    && elapsedMinutes <= 3);
                            })
                            .ForEach(needAlarmAnnouncements.Add);

                        return (needAlarmEvents, needAlarmAnnouncements);
                    }
                }
            }
            catch (Exception e)
            {
                YameiLogExtensions.WriteLog(e);
                CommonExtensions.SleepSeconds(30);
            }
        }
    }
}