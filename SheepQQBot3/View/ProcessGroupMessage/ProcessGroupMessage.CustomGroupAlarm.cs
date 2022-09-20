using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using Yamei.Common;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    public static partial class ProcessGroupMessage
    {
        /// <summary>
        /// 群提醒方法命令的开头
        /// </summary>
        private const string COMMAND_CUSTOM_GROUP_ALARM_LIBRARY = "#YM#";

        /// <summary>
        /// 自定义群提醒
        /// <para>可在群内设置消息提醒</para>
        /// </summary>
        /// <param name="customGroupAlarms"></param>
        /// <param name="groupMessage"></param>
        /// <returns></returns>
        public static bool CustomGroupAlarm(Dictionary<Guid, CustomGroupAlarm> customGroupAlarms, GroupMessage groupMessage)
        {
            var groupId = groupMessage.GroupId;
            var targetId = groupMessage.Sender.User_Id;
            var message = groupMessage.Message;
            var upperMessage = message.ToUpper();
            if (!upperMessage.StartsWith(COMMAND_CUSTOM_GROUP_ALARM_LIBRARY))
                return false;

            var sendMessage = new StringBuilder();
            var dateNow = DateTime.Now;
            try
            {
                var changedMessageSpace = message
                    .Substring(COMMAND_CUSTOM_GROUP_ALARM_LIBRARY.Length)
                    .Replace(COMMA_FULL, COMMA);

                var changedMessage = changedMessageSpace
                    .Replace(SPACE, string.Empty);

                var (startChar1, startChar2) = GetStartChar(upperMessage.Substring(COMMAND_CUSTOM_GROUP_ALARM_LIBRARY.Length));
                var isNoAt = false;
                var isNoReply = false;
                var groupAlarms = customGroupAlarms.ToValueList();
                switch (startChar1)
                {
                    case 'C':
                        var errorMsg = $"{RN}输入格式有误, 请输入 #ym#ch 查询帮助!";
                        var messageArraySpace = changedMessageSpace.Split(COMMA);
                        //var messageArray = changedMessage.Split(COMMA);
                        //switch (messageArraySpace.Length)
                        //{
                        //    case 4:
                        //        isNoAt = messageArray[3].Equals("1");
                        //        isNoResponse = messageArray[2].Equals("1");
                        //        break;
                        //    case 3:
                        //        isNoResponse = messageArray[2].Equals("1");
                        //        break;
                        //    default:
                        //        break;
                        //}

                        switch (startChar2)
                        {
                            case 'H':
                                isNoReply = false;
                                sendMessage.Append($" 群提醒功能介绍:" +
                                    $"{RN}#ym#ca收菜,60 -> 60分钟后发送提醒消息, 内容为'收菜'" +
                                    $"{RN}#ym#ca收菜,2019-7-13 19:00 -> 在指定时间(只精确到分)发送提醒消息, 内容为'收菜'" +
                                    $"{RN}#ym#ca收菜,19:00 -> 同上, 省略日期时为当天提醒" +
                                    $"{RN}#ym#cd2019-7-13 19:00 -> 删除2019-7-13 19:00的提醒, 如用分钟增加的提醒可以用cl命令查询具体时间" +
                                    $"{RN}#ym#cl -> 列出当前还未提醒的项目" +
                                    $"{RN}特殊参数: (在消息内包含)" +
                                    $"{RN}[at-22222] at某人" +
                                    $"{RN}[-na] 提醒时不at自己" +
                                    $"{RN}[-nr] 添加提醒时不发送反馈");
                                break;
                            case 'A':
                                var isTimeFormat = messageArraySpace[1].Contains(":");
                                //var addMessage = TextExtensions.Replace2CQCode(messageArraySpace[0].Substring(2));
                                string addMessage;
                                (addMessage, isNoAt, isNoReply) = messageArraySpace[0].Substring(2).ToCqCode(targetId);
                                if (isTimeFormat && DateTime.TryParse(messageArraySpace[1], out var addDate))
                                {
                                    AddCustomAlarm(addDate, addMessage);
                                }
                                else
                                {
                                    if (isTimeFormat && DateTime.TryParse($"{dateNow.ToYYYYMDD()} {messageArraySpace[1]}", out var addDate2))
                                    {
                                        AddCustomAlarm(addDate2, addMessage);
                                    }
                                    else
                                    {
                                        if (int.TryParse(messageArraySpace[1], out var addMinute))
                                            AddCustomAlarm(dateNow.AddMinutes(addMinute), addMessage);
                                        else
                                            sendMessage.Append(errorMsg);
                                    }
                                }
                                break;
                            case 'D':
                                var deleteInfo = messageArraySpace[0].Substring(2);
                                if (DateTime.TryParse(deleteInfo, out var deleteDate))
                                {
                                    DeleteUserAlarmCustom(deleteDate);
                                }
                                else
                                {
                                    if (DateTime.TryParse($"{dateNow.ToYYYYMDD()} {deleteInfo}", out var deleteDate2))
                                    {
                                        DeleteUserAlarmCustom(deleteDate2);
                                    }
                                    else
                                    {
                                        sendMessage.Append(errorMsg);
                                    }
                                }
                                break;
                            case 'L':
                                isNoReply = false;
                                var listArray = changedMessageSpace.Split(COMMA);
                                var isShowId = listArray.Length == 2 && listArray[1].Trim() == "1";
                                groupAlarms
                                    .Where(each => each.TargetId == targetId)
                                    .OrderBy(each => each.alarmDate)
                                    .ForEach(customGroupAlarm =>
                                    {
                                        var alarmMessage = customGroupAlarm.alarmMessage.ToNormalText();
                                        alarmMessage = alarmMessage.ByteSubstring(isShowId ? 56 : 20);
                                        sendMessage.Append($"{RN}[{(isShowId ? customGroupAlarm.Id.ToString() : string.Empty)}]"
                                            + $"[{customGroupAlarm.alarmDate.ToYYYYMMDDHHMMSS()}] {alarmMessage}");
                                    });

                                if (string.IsNullOrEmpty(sendMessage.ToString()))
                                    sendMessage.Append($"{RN}无任何提醒记录!");

                                break;
                            case 'T':
                                isNoReply = false;
                                sendMessage.Append($"{RN}{messageArraySpace[0].Substring(2).ToCqCode(targetId).Result}");
                                break;
                            default:
                                sendMessage.Append(errorMsg);
                                break;
                        }
                        break;

                        void AddCustomAlarm(DateTime addDateTime, string addMessage)
                        {
                            var addDateString = addDateTime.ToYYYYMMDDHHMMSS();
                            var groupAlarm = groupAlarms.FirstOrDefault(each => (each.alarmDate - addDateTime).TotalSeconds == 0);
                            if (groupAlarm != null)
                            {
                                // 有记录, 则提示
                                sendMessage.Append($"{RN}已存在 {addDateString} 的提醒记录!" +
                                    $"{RN}提醒内容: {groupAlarm.alarmMessage}");
                            }
                            else
                            {
                                // 无记录, 则添加并发送反馈
                                var newId = Guid.NewGuid();
                                customGroupAlarms.Add(newId, new CustomGroupAlarm(newId, groupId, targetId, addDateTime, addMessage, !isNoAt));
                                sendMessage.Append($"{RN}已添加时间为 {addDateString} 的提醒记录!");
                                ConfigExtensions.SaveConfig();
                            }
                        }

                        void DeleteUserAlarmCustom(DateTime deleteDateTime)
                        {
                            var deleteDateString = deleteDateTime.ToYYYYMMDDHHMMSS();
                            var groupAlarm = groupAlarms.FirstOrDefault(each => (each.alarmDate - deleteDateTime).TotalSeconds <= 1);
                            if (groupAlarm != null)
                            {
                                // 有记录, 则删除并发送反馈
                                var deleteId = groupAlarm.Id;
                                customGroupAlarms.Remove(deleteId);
                                sendMessage.Append($"{RN}已删除 {deleteDateString}({deleteId}) 的提醒记录!");
                            }
                            else
                            {
                                // 无记录, 则提醒错误
                                sendMessage.Append($"{RN}不存在 {deleteDateString} 的提醒记录!");
                            }
                        }
                    default:
                        // 不支持提示
                        isNoReply = false;
                        sendMessage.Append($"{RN}不支持的命令内容!");
                        break;
                }

                if (!isNoReply)
                    Api.SendGroupMessage(groupId, $"{(isNoAt ? string.Empty : $"[CQ:at,qq={targetId}]")}{sendMessage}");
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}