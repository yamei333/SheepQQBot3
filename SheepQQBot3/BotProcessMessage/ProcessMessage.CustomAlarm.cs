using Masuit.Tools;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage;

public static partial class ProcessMessage
{
    /// <summary>
    /// 自定义提醒方法命令的开头
    /// </summary>
    private const string COMMAND_CUSTOM_GROUP_ALARM_LIBRARY = "#TX#";

    private const string ERROR_MESSAGE = $"{ENTER}输入格式有误, 请输入 #tx#ch 查询帮助!";

    private const int COMMAND_CUSTOM_GROUP_ALARM_MINLENGTH = 6;

    private const int COMMAND_CUSTOM_GROUP_ALARM_CONTENT_MINLENGTH = 2;

    private static Regex regCustomAlarmDateTime = RegexGenerator.CustomAlarm_DateTime();
    private static Regex regCustomAlarmTime = RegexGenerator.CustomAlarm_Time();
    private static Regex regCustomAlarmMinutes = RegexGenerator.CustomAlarm_Minutes();

    /// <summary>
    /// 自定义提醒(群版)
    /// <para>可群内设置消息提醒</para>
    /// </summary>
    public static Task<bool> CustomGroupAlarmAsync(
        GroupMessage groupMessage)
        => CustomAlarmAsyncCore(
            true,
            groupMessage.GroupId,
            groupMessage.Sender.UserId,
            groupMessage.Message);

    /// <summary>
    /// 自定义提醒(私聊版)
    /// <para>可私聊设置消息提醒</para>
    /// </summary>
    public static Task<bool> CustomPrivateAlarmAsync(
        PrivateMessage privateMessage)
        => CustomAlarmAsyncCore(
            false,
            privateMessage.Sender.GroupId,
            privateMessage.Sender.UserId,
            privateMessage.Message);

    /// <summary>
    /// 自定义提醒
    /// <para>可在群内设置消息提醒</para>
    /// </summary>
    public static async Task<bool> CustomAlarmAsyncCore(
        bool isGroup,
        long? groupId,
        long targetId,
        string message)
    {
        if (message.Length < COMMAND_CUSTOM_GROUP_ALARM_MINLENGTH
            || !message.StartsWith(COMMAND_CUSTOM_GROUP_ALARM_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
        {
            return false;
        }

        message = message[COMMAND_CUSTOM_GROUP_ALARM_LIBRARY.Length..];

        try
        {
            var dateNow = DateTime.Now;
            var sendMessage = string.Empty;
            var (startChar1, startChar2) = GetStartChar(message);
            message = message[COMMAND_CUSTOM_GROUP_ALARM_CONTENT_MINLENGTH..];
            var isNoAt = false;
            var isNoReply = false;
            var customAlarms = PublicVar.BotConfig.CustomAlarms;
            var customAlarmValues = PublicVar.BotConfig.CustomAlarms.ToValueList();
            switch (startChar1)
            {
                case 'C':
                    switch (startChar2)
                    {
                        case 'H':
                            sendMessage += $"自定义提醒功能命令:" +
                                $"{ENTER}#tx#ca收菜#60# -> 60分钟后发送提醒消息, 内容为'收菜'" +
                                $"{ENTER}#tx#ca收菜#2019-7-13 19:00# -> 在指定时间发送提醒消息, 内容为'收菜'" +
                                $"{ENTER}#tx#ca收菜#19:00# -> 同上, 省略日期时为当天提醒" +
                                $"{ENTER}#tx#cd2019-7-13 19:00 -> 删除2019-7-13 19:00的提醒" +
                                $"{ENTER}#tx#cd[id] -> 使用ID删除提醒" +
                                $"{ENTER}#tx#cl -> 列出当前还未提醒的项目" +
                                $"{ENTER}特殊参数: (在消息内包含)" +
                                $"{ENTER}[at-22222] at某人" +
                                $"{ENTER}[-na] 提醒时不at自己" +
                                $"{ENTER}[-nr] 添加提醒时不发送反馈" +
                                $"{ENTER}[-loop] 提醒发送后会每5分钟再次发送,直到提醒被删除" +
                                $"{ENTER}[-bark] 发送bark推送(需要配置BarkKey)";
                            break;
                        case 'A':
                            if (GetAddCustomAlarmDate(out var addAlarmDateTime))
                            {
                                (var addMessage, isNoAt, isNoReply, var isLoop, var isBark) = message.ToCqCode(targetId);
                                if (isBark && !BotExtensions.HasUserConfig(targetId, UserConfigType.BarkKey))
                                {
                                    const string barkKeyError = "BarkKey未正确配置, 无法使用[-bark]!";
                                    if (isGroup)
                                    {
                                        await BotServer.SendGroupMessageAsync(groupId.GetValueOrDefault(),
                                            $"{CQCode.At(targetId)}{barkKeyError}").ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        await BotServer.SendPrivateMessageAsync(targetId, groupId, barkKeyError).ConfigureAwait(false);
                                    }

                                    return true;
                                }

                                var addDateString = addAlarmDateTime.ToYYYYMDHHMMSS();
                                var customAlarm = customAlarmValues.FirstOrDefault(each => (each.AlarmDate - addAlarmDateTime).TotalSeconds == 0);
                                if (customAlarm != null)
                                {
                                    // 有记录, 则提示
                                    sendMessage += $"{ENTER}已存在 {addDateString} 的提醒记录!" +
                                        $"{ENTER}提醒内容: {customAlarm.AlarmMessage}";
                                }
                                else
                                {
                                    // 无记录, 则添加并发送反馈
                                    var newId = Guid.NewGuid();
                                    customAlarms.Add(newId, new CustomAlarm(
                                        newId, isGroup, groupId, targetId, addAlarmDateTime, addMessage, !isNoAt, isLoop, isBark));
                                    sendMessage += $"{ENTER}已添加时间为 {addDateString} 的提醒记录!";
                                    ConfigExtensions.SaveConfig();
                                }
                            }
                            else
                            {
                                sendMessage += ERROR_MESSAGE;
                            }

                            break;
                        case 'D':
                            if (Guid.TryParse(message, out var deleteId))
                            {
                                DeleteUserAlarmCustomById(deleteId);
                            }
                            else
                            {
                                message = $"${message}$";
                                if (GetAddCustomAlarmDate(out var deleteDateTime))
                                    DeleteUserAlarmCustomByDate(deleteDateTime);
                                else
                                    sendMessage += ERROR_MESSAGE;
                            }

                            break;
                        case 'L':
                            if (isGroup)
                            {
                                customAlarmValues
                                    .Where(each => each.IsGroup && each.GroupId == groupId && each.TargetId == targetId)
                                    .OrderBy(each => each.AlarmDate)
                                    .ForEach(customGroupAlarm =>
                                    {
                                        var alarmMessage = customGroupAlarm.AlarmMessage
                                            .ToNormalText().ByteSubstring(68);
                                        sendMessage += $"{ENTER}" +
                                            $"({customGroupAlarm.Id})" +
                                            $"{(customGroupAlarm.IsBark ? "[推]" : string.Empty)}" +
                                            $"{(customGroupAlarm.IsLoop ? "[loop]" : string.Empty)}" +
                                            $"[{customGroupAlarm.AlarmDate.ToYYYYMDHHMMSS()}]{alarmMessage}";
                                    });
                            }
                            else
                            {
                                customAlarmValues
                                    .Where(each => each.TargetId == targetId)
                                    .OrderBy(each => each.AlarmDate)
                                    .ForEach(customGroupAlarm =>
                                    {
                                        var alarmMessage = customGroupAlarm.AlarmMessage
                                            .ToNormalText().ByteSubstring(68);
                                        sendMessage += ENTER +
                                            $"({customGroupAlarm.Id})" +
                                            $"{(customGroupAlarm.IsGroup ? $"[群{customGroupAlarm.GroupId}]" : "[私]")}" +
                                            $"{(customGroupAlarm.IsBark ? "[推]" : string.Empty)}" +
                                            $"{(customGroupAlarm.IsLoop ? "[loop]" : string.Empty)}" +
                                            $"[{customGroupAlarm.AlarmDate.ToYYYYMDHHMMSS()}]{alarmMessage}";
                                    });
                            }

                            if (sendMessage.IsNullOrEmpty())
                                sendMessage += $"{ENTER}无任何提醒记录!";

                            break;
                        case 'T':
                            sendMessage += $"{ENTER}{message.ToCqCode(targetId).Result}";
                            break;
                        default:
                            sendMessage += ERROR_MESSAGE;
                            break;
                    }
                    break;

                    void DeleteUserAlarmCustomByDate(DateTime deleteDateTime)
                    {
                        var deleteDateString = deleteDateTime.ToYYYYMDHHMMSS();
                        var customAlarm = customAlarmValues.FirstOrDefault(each => (each.AlarmDate - deleteDateTime).TotalSeconds <= 1);
                        if (customAlarm != null)
                        {
                            // 有记录, 则删除并发送反馈
                            var deleteId = customAlarm.Id;
                            customAlarms.Remove(deleteId);
                            sendMessage += $"{ENTER}已删除提醒记录! [{deleteDateString}]({deleteId})";
                        }
                        else
                        {
                            // 无记录, 则提醒错误
                            sendMessage += $"{ENTER}不存在时间为 [{deleteDateString}] 的提醒记录!";
                        }
                    }

                    void DeleteUserAlarmCustomById(Guid deleteId)
                    {
                        sendMessage += customAlarms.Remove(deleteId)
                            ? $"{ENTER}已删除提醒记录! ({deleteId})"
                            : $"{ENTER}不存在ID为 ({deleteId}) 的提醒记录!";
                    }
                default:
                    // 不支持提示
                    sendMessage += $"不支持的命令内容!";
                    break;
            }

            if (!isNoReply)
            {
                if (sendMessage.StartsWith(ENTER))
                    sendMessage = sendMessage[ENTER.Length..];

                if (isGroup)
                {
                    await BotServer.SendGroupMessageAsync(groupId.GetValueOrDefault(),
                        $"{(isNoAt ? string.Empty : $"{CQCode.At(targetId)}{ENTER}")}{sendMessage}").ConfigureAwait(false);
                }
                else
                {
                    await BotServer.SendPrivateMessageAsync(targetId, groupId, $"{sendMessage}").ConfigureAwait(false);
                }
            }

            bool GetAddCustomAlarmDate(out DateTime alarmDateTime)
            {
                var match = regCustomAlarmDateTime.Match(message);
                if (match.Success && DateTime.TryParse(match.Groups[1].Value, out alarmDateTime))
                {
                    message = message.Replace(match.Value, string.Empty);
                    return true;
                }

                match = regCustomAlarmTime.Match(message);
                if (match.Success && DateTime.TryParse($"{dateNow.ToYYYYMD()} {match.Groups[1].Value}", out alarmDateTime))
                {
                    message = message.Replace(match.Value, string.Empty);
                    return true;
                }

                match = regCustomAlarmMinutes.Match(message);
                if (match.Success)
                {
                    alarmDateTime = dateNow.AddMinutes(int.Parse(match.Groups[1].Value));
                    message = message.Replace(match.Value, string.Empty);
                    return true;
                }

                alarmDateTime = default;
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}