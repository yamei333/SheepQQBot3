using CommonLibrary;
using Masuit.Tools;
using OpenRouter.NET.Models;
using SheepQQBot3.DbModel;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.QQ;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yamei.Common;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Group;

public static partial class ProcessGroupMessage
{
    private static readonly Regex _regReplaceCQCode = RegexGenerator.ReplaceCQCode();
    //private const string PATH_CACHE_WORDCLOUD = "WordCloud";
    //private const string PATH_WORDCLOUD_CONFIG = "WordCloud/Config";

    /// <summary>
    /// 群聊消息总结最低消息数
    /// </summary>
    private const int SUMMARY_MESSAGE_COUNT_LIMIT = 25;

    /// <summary>
    /// 统计忽略词长
    /// </summary>
    private const int CHAT_SUMMARY_LIMIT_BYTE = 100;

    /// <summary>
    /// 群聊总结命令
    /// </summary>
    private const string COMMAND_CHAT_SUMMARY = "#ZJ#";

    /// <summary>
    /// 群聊总结命令CD (60分钟)
    /// </summary>
    private const int CHAT_SUMMARY_TO_FAST_TIMES = 3600;

    /// <summary>
    /// 群聊总结最后一次执行时间
    /// </summary>
    private static readonly ConcurrentDictionary<string, DateTime> _chatSummaryRequestLastTimes = [];

    /// <summary>
    /// 群聊总结
    /// </summary>
    /// <param name="aiGroupConfig"><see cref="AIGroupConfig"/></param>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    /// <returns></returns>
    public static async Task<bool> ChatSummaryAsync(AIGroupConfig aiGroupConfig, GroupMessage groupMessage)
    {
        var groupId = groupMessage.GroupId;
        var senderId = groupMessage.Sender.UserId.ToString();
        var messageId = groupMessage.MessageId;
        var timeStamp = groupMessage.DateTime.ToTimeStamp();
        var message = groupMessage.Message;

        try
        {
            if (senderId is "1395335318" or "664152503")
                return true;

            // MEMO : 命令格式检查
            if (!message.StartsWith(COMMAND_CHAT_SUMMARY, StringComparison.CurrentCultureIgnoreCase))
            {
                if (!NeedRecordMessage(message))
                    return true;

                var addBotGroupMessage = new BotGroupMessage(groupId, senderId, messageId, timeStamp, message);
                // 去掉CQCode之后内容过多的(认定是转发复读)
                if (addBotGroupMessage.IsNullOrEmpty() || addBotGroupMessage.MessageText.Replace(_regReplaceCQCode, string.Empty).GetByteCount() > CHAT_SUMMARY_LIMIT_BYTE)
                    return true;

                // MEMO : 将群聊录入数据库
                lock (BotDb.SyncLock)
                {
                    var botGroupMessage = BotDb.BotGroupMessages.FindAsync(groupId, senderId, messageId, timeStamp).Result;
                    if (botGroupMessage == null)
                        BotDb.AddAsync(addBotGroupMessage);
                }

                return true;
            }
            else
            {
                if (message.Length < 4)
                    return false;

                var dateNow = DateTime.Now;
                if (!BotExtensions.IsAdmin(senderId)
                    && (dateNow - _chatSummaryRequestLastTimes.GetOrAdd(groupId, DateTime.MinValue)).TotalSeconds < CHAT_SUMMARY_TO_FAST_TIMES)
                {
                    await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.Coffee).ConfigureAwait(false);
                    return true;
                }

                _chatSummaryRequestLastTimes.AddOrUpdate(groupId, dateNow, dateNow);

                if (message.Length == 4)
                    return await AISummary(16, "一天").ConfigureAwait(false);

                var summaryType = message.ToUpper().Substring(4, 1);
                switch (summaryType)
                {
                    case "A":
                        return await AISummary(16, "一天").ConfigureAwait(false);
                    default:
                        var match = Regex.Match(message[4..], @"\d+");
                        if (match.Success)
                            return await AISummary(int.Parse(match.Value), $"{match}小时").ConfigureAwait(false);

                        await GlobalBotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
                        return false;
                }

                async Task<bool> AISummary(int aiHour, string description = "")
                {
                    // MEMO : 某些时间不该发消息
                    if (AIExtensions.IsCantSendMessage(groupId, (id, msg) => _ = GlobalBotClient.SendGroupMessageAsync(id, msg)))
                        return true;

                    // MEMO : AI小时统计
                    if (aiHour <= 0)
                    {
                        await GlobalBotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_ParameterRangeError(senderId, messageId)).ConfigureAwait(false);
                        return false;
                    }

                    await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.E_Flash).ConfigureAwait(false);
                    await AISummaryCore(groupId, description, dateNow.AddHours(-aiHour)).ConfigureAwait(false);
                    return true;
                }

                // MEMO : AI群聊总结
                async Task AISummaryCore(string targetGroupId, string description, DateTime fromDate, DateTime? toDate = null)
                {
                    if (IsDebug)
                        targetGroupId = "414774779";

                    var groupMembers = await GlobalBotClient.GetGroupMembersAsync(targetGroupId).ConfigureAwait(false);
                    if (groupMembers == null)
                    {
                        await GlobalBotClient.SendGroupMessageAsync(targetGroupId, "群成员信息获取失败!").ConfigureAwait(false);
                        return;
                    }

                    var thisRequestContentParts = new List<ContentPart>();
                    thisRequestContentParts.AddSystemHint($"[以下是最近{description}的群聊内容]");
                    lock (BotDb.SyncLock)
                    {
                        var fromDateTimeStamp = fromDate.ToTimeStamp();
                        var toDateTimeStamp = (toDate ?? dateNow).ToTimeStamp();
                        BotDb.BotGroupMessages
                            .Where(each => each.GroupId == targetGroupId
                                && each.TimeStamp >= fromDateTimeStamp
                                && each.TimeStamp < toDateTimeStamp)
                            .AsEnumerable()
                            .ForEach(each =>
                            {
                                var historyMessage = each.MessageText;
                                historyMessage = historyMessage.Trim();
                                if (historyMessage.IsNullOrEmpty())
                                    return;

                                // MEMO : 不喜欢的内容直接屏蔽
                                if (_regInjectHurry.IsMatch(historyMessage))
                                    return;

                                //groupMembers[each.TargetId].ToAIChatSender()
                                _ = thisRequestContentParts.AddQQChatMessageAsync(groupMembers[each.TargetId].ToAIChatSender(AIUserInfos), historyMessage, groupMembers, true);
                            });
                    }

                    if (!IsDebug && thisRequestContentParts.Count <= SUMMARY_MESSAGE_COUNT_LIMIT + 1)
                    {
                        await GlobalBotClient.SendGroupMessageAsync(targetGroupId, $"群聊消息过少(少于{SUMMARY_MESSAGE_COUNT_LIMIT}条)! 不需要总结!").ConfigureAwait(false);
                        return;
                    }

                    thisRequestContentParts.AddSystemHint($"[群聊内容到此为止]");

                    var sender = groupMessage.Sender;
                    await thisRequestContentParts.AddQQChatMessageAsync(sender,
                        $@"{CQCode.At(BotId)} {AppSettingExtensions.Get("chatSummaryPrompt")}", groupMembers).ConfigureAwait(false);
                    await thisRequestContentParts.SendAsync($"z{targetGroupId}", targetGroupId, targetGroupId, false, groupMembers.ToSenderDictionary(AIUserInfos), aiGroupConfig,
                            (id, msg) => GlobalBotClient.SendGroupMessageAsync(id, msg).ConfigureAwait(false), GlobalAIConfig.ModelSummary)
                        .ConfigureAwait(false);
                }
            }
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteLog(e);
            return false;
        }
    }

    /// <summary>
    /// 不记录的群聊消息
    /// </summary>
    private static bool NeedRecordMessage(string message)
    {
        if (message.StartsWith("#") || message.ToUpper() == "R")
            return false;

        if (message.Contains($"[CQ:at,qq={BotId}]") || message.Contains($"[CQ:at,qq=3889001246]"))
            return false;

        if (message.Contains("色图") && message.BytesCount() <= 20)
            return false;

        // MEMO : emoji数量超过一定数量
        if (_regEmoji.Matches(message).Count >= 6)
            return false;

        // MEMO : 色图系列
        var uMessage = message.ToUpper();
        if (uMessage.EndsWith("色图")
            || uMessage.EndsWith("色图L")
            || uMessage.EndsWith("色图N")
            || uMessage.EndsWith("色图J")
            || uMessage.EndsWith("色图Y")
            || uMessage.EndsWith("色图S")
            || uMessage.EndsWith("色图C"))
        {
            return false;
        }

        // MEMO : 去除所有CQ码之后无任何内容的消息
        return !_regReplaceCQCode.Replace(message, match =>
        {
            var cqCode = match.Groups["tag"].Value;
            if (cqCode != "image" && cqCode != "at")
                return string.Empty;

            return match.Value;
        }).Trim().IsNullOrEmpty();
    }
}