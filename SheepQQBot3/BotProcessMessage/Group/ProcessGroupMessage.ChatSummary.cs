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
    private const int SUMMARY_MESSAGE_COUNT_LIMIT = 50;

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
                if (message.Length <= 4)
                    return false;

                var dateNow = DateTime.Now;
                if (!BotExtensions.IsAdmin(senderId)
                    && (dateNow - _chatSummaryRequestLastTimes.GetOrAdd(groupId, DateTime.MinValue)).TotalSeconds < CHAT_SUMMARY_TO_FAST_TIMES)
                {
                    await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.Coffee).ConfigureAwait(false);
                    return true;
                }

                _chatSummaryRequestLastTimes.AddOrUpdate(groupId, dateNow, dateNow);
                var summaryType = message.ToUpper().Substring(4, 1);
                //var summaryWords = new Dictionary<string, int>();
                //var wordCloudWidth = 1000;
                //var wordNums = 100;

                //var chatSummaryGroupConfigFilePath = Path.Combine(PATH_WORDCLOUD_CONFIG, $"{groupId}.json");
                //var chatSummaryGroupConfig = JsonExtensions.FromJsonFile<ChatSummaryGroupConfig>(chatSummaryGroupConfigFilePath);
                //var regNumber = new Regex("[0-9]+");

                switch (summaryType)
                {
                    case "A":
                        // MEMO : 某些时间不该发消息
                        if (AIExtensions.IsCantSendMessage(groupId, (id, msg) => _ = GlobalBotClient.SendGroupMessageAsync(id, msg)))
                            return true;

                        // MEMO : AI小时统计
                        var aiHourStr = message[5..];
                        var aiHour = 16;
                        if (!aiHourStr.IsNullOrEmpty())
                        {
                            if (!int.TryParse(aiHourStr, out aiHour))
                            {
                                await GlobalBotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
                                return false;
                            }

                            if (aiHour is <= 0 or >= 24)
                            {
                                await GlobalBotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_ParameterRangeError(senderId, messageId)).ConfigureAwait(false);
                                return false;
                            }
                        }

                        //await BotClient.SendGroupMessageAsync(groupId, $"{CQCode.At(senderId)} 小助手正在收集聊天记录进行总结，请稍等片刻!").ConfigureAwait(false);
                        await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.E_Flash).ConfigureAwait(false);
                        await AISummary(groupId, dateNow.AddHours(-aiHour)).ConfigureAwait(false);

                        return true;
                    //case "B":
                    //    if (!BotExtensions.IsAdmin(senderId))
                    //    {
                    //        await BotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CanOnlyAdminUseError(senderId, messageId)).ConfigureAwait(false);
                    //        return false;
                    //    }

                    //    var dataMessage = message[5..];
                    //    if (dataMessage.IsNullOrEmpty())
                    //    {
                    //        await BotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
                    //        return false;
                    //    }

                    //    if (await JiebaDb.StopWords.FindAsync(dataMessage).ConfigureAwait(false) != null)
                    //    {
                    //        await BotClient.SendGroupMessageAsync(groupId, $"关键字[{dataMessage}]已存在于StopWords").ConfigureAwait(false);
                    //        return false;
                    //    }

                    //    if (!chatSummaryGroupConfig.ExcludeWords.Add(dataMessage))
                    //    {
                    //        await BotClient.SendGroupMessageAsync(groupId, $"关键字[{dataMessage}]已存在于群配置").ConfigureAwait(false);
                    //        return false;
                    //    }

                    //    await File.WriteAllTextAsync(chatSummaryGroupConfigFilePath, chatSummaryGroupConfig.ToJsonIgnoreNull()).ConfigureAwait(false);
                    //    await BotClient.SendGroupMessageAsync(groupId, $"已添加统计屏蔽词[{dataMessage}]").ConfigureAwait(false);
                    //    return true;
                    //case "H":
                    //    // MEMO : 小时统计
                    //    var hourStr = message[5..];
                    //    var hour = 12;
                    //    if (!hourStr.IsNullOrEmpty())
                    //    {
                    //        if (!int.TryParse(hourStr, out hour))
                    //        {
                    //            await BotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
                    //            return false;
                    //        }

                    //        if (hour is <= 0 or >= 24)
                    //        {
                    //            await BotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_ParameterRangeError(senderId, messageId)).ConfigureAwait(false);
                    //            return false;
                    //        }
                    //    }

                    //    await BotClient.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                    //    CalcWordCloud(groupId, dateNow.AddHours(-hour));
                    //    wordCloudWidth = 1200;
                    //    wordNums = 200;
                    //    break;
                    //case "D":
                    //    // MEMO : 日统计
                    //    await BotClient.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                    //    CalcWordCloud(groupId, dateNow.AddDays(-1));
                    //    wordCloudWidth = 1500;
                    //    wordNums = 250;
                    //    break;
                    //case "W":
                    //    // MEMO : 周统计
                    //    await BotClient.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                    //    CalcWordCloud(groupId, dateNow.AddDays(-7));
                    //    wordCloudWidth = 1800;
                    //    wordNums = 300;
                    //    break;
                    //case "M":
                    //    // MEMO : 月统计
                    //    await BotClient.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                    //    CalcWordCloud(groupId, dateNow.AddMonths(-1));
                    //    wordCloudWidth = 2400;
                    //    wordNums = 400;
                    //    break;
                    //case "Y":
                    //    // MEMO : 年统计
                    //    await BotClient.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                    //    CalcWordCloud(groupId, dateNow.AddYears(-1));
                    //    wordCloudWidth = 3000;
                    //    wordNums = 500;
                    //    break;
                    default:
                        await GlobalBotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
                        return false;
                }

                //var wordCloudImage = $"{groupId}.png";
                //var maskFilePath = CommonExtensions.GetPath(PATH_WORDCLOUD_CONFIG, wordCloudImage, GetPathType.Normal);
                //var wordCloudWords = summaryWords.OrderByDescending(each => each.Value)
                //    .Take(wordNums)
                //    .ToDictionary(each => each.Key, each => each.Value);

                //wordCloudWords.GenerateWordCloud(wordCloudWidth, wordCloudWidth,
                //    CommonExtensions.GetPath(PATH_CACHE_WORDCLOUD, wordCloudImage, GetPathType.Normal), true, maskFilePath);
                //await File.WriteAllTextAsync(Path.Combine(PATH_CACHE_WORDCLOUD, $"{groupId}.txt"), string.Join("\r\n", wordCloudWords.Keys)).ConfigureAwait(false);
                //await BotClient.SendGroupMessageAsync(groupId,
                //    CQCode.Image(CommonExtensions.GetPath(PATH_CACHE_WORDCLOUD, wordCloudImage, GetPathType.CQCodePath))).ConfigureAwait(false);
                //if (IsDebug)
                //    await BotClient.SendGroupMessageAsync(groupId, "聊天记录统计已发送!").ConfigureAwait(false);

                //return true;

                //void CalcWordCloud(string targetGroupId, DateTime fromDate, DateTime? toDate = null)
                //{
                //    lock (BotDb.SyncLock)
                //    {
                //        var fromDateTimeStamp = fromDate.ToTimeStamp();
                //        var toDateTimeStamp = (toDate ?? dateNow).ToTimeStamp();
                //        var repeatSkipQueue = new Queue<string>();
                //        BotDb.BotGroupMessages
                //            .Where(each => each.GroupId == targetGroupId
                //                && each.TimeStamp >= fromDateTimeStamp
                //                && each.TimeStamp < toDateTimeStamp)
                //            .AsEnumerable()
                //            .ForEach(each =>
                //            {
                //                var historyMessage = each.MessageText;
                //                historyMessage = historyMessage.Trim().ToUpper();
                //                if (historyMessage.IsNullOrEmpty())
                //                    return;

                //                // MEMO : 不喜欢的内容直接屏蔽
                //                if (_regInjectHurry.IsMatch(historyMessage))
                //                    return;

                //                // MEMO : REPEAT_SKIP_SUMMARY 句以内复读则忽略
                //                if (repeatSkipQueue.Contains(historyMessage))
                //                    return;

                //                repeatSkipQueue.Enqueue(historyMessage);
                //                if (repeatSkipQueue.Count > REPEAT_SKIP_SUMMARY)
                //                    repeatSkipQueue.Dequeue();

                //                var segmenterResult = historyMessage.ExtractTagsWithWeight_Idf();
                //                var excludeWords = chatSummaryGroupConfig?.ExcludeWords;
                //                segmenterResult
                //                    .Where(wordWeightPair =>
                //                    {
                //                        var word = wordWeightPair.Word;
                //                        if (regNumber.Match(word).Value == word)
                //                            return false;

                //                        return excludeWords == null || !excludeWords.Contains(word);
                //                    })
                //                    .ForEach(wordWeightPair => summaryWords
                //                        .AddOrUpdate(wordWeightPair.Word, (int)(wordWeightPair.Weight * 100), (_, oldValue) => oldValue + (int)(wordWeightPair.Weight * 100)));
                //            });
                //    }
                //}

                // MEMO : AI群聊总结
                async Task AISummary(string targetGroupId, DateTime fromDate, DateTime? toDate = null)
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
                    thisRequestContentParts.AddSystemHint($"[以下是今天的群聊内容]");
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
                                _ = thisRequestContentParts.AddQQChatMessageAsync(groupMembers[each.TargetId].ToAIChatSender(AIUserInfos), historyMessage, groupMembers);
                            });
                    }

                    if (!IsDebug && thisRequestContentParts.Count <= SUMMARY_MESSAGE_COUNT_LIMIT + 1)
                    {
                        await GlobalBotClient.SendGroupMessageAsync(targetGroupId, "群聊消息过少! 不需要总结!").ConfigureAwait(false);
                        return;
                    }

                    thisRequestContentParts.AddSystemHint($"[群聊内容到此为止]");

                    var sender = groupMessage.Sender;
                    //await thisRequestMessages.AddQQChatMessageAsync(sender, $"{CQCode.At(BotId)} 总结一下大家都聊了什么，先对不同内容进行总结，最后再简短的一句话描述，内容分批发送。", groupMembers).ConfigureAwait(false);
                    await thisRequestContentParts.AddQQChatMessageAsync(sender,
                        $@"{CQCode.At(BotId)} 总结一下大家都聊了什么，先发一条反馈，接着按话题总结，每话题分气泡发送（每条可选配表情包），再从群聊中选取部分内容作为【今日怪话】（不需要发表感想），再增加一个群聊颁奖环节，最后再一句话总结。
话题格式:
【话题1标题（替换为实际内容）】
1. xx
2. xx

今日怪话格式:
【今日怪话】
1. 群友名（替换为实际内容）：xx

群聊颁奖格式:
【群聊颁奖】
活跃奖: xx
小丑奖: xx", groupMembers).ConfigureAwait(false);
                    await thisRequestContentParts.SendAsync($"z{targetGroupId}", targetGroupId, targetGroupId, false, groupMembers.ToSenderDictionary(AIUserInfos), aiGroupConfig,
                            (id, msg) => GlobalBotClient.SendGroupMessageAsync(id, msg).ConfigureAwait(false), GlobalAIConfig.ModelSummary)
                        .ConfigureAwait(false);

                    //requestMessages.AddSystemHint($"[群聊内容到此为止]");
                    //await requestMessages.SendAsync($"z{targetGroupId}", targetGroupId, targetGroupId, false, groupMembers.ToSenderDictionary(GroupMemberInfos), aiGroupConfig,
                    //    (id, msg) => BotClient.SendGroupMessageAsync(id, msg).ConfigureAwait(false),
                    //    $"{sender.NickName}(QQ:{sender.UserId}) 想让你总结一下大家都聊了些什么，先对不同内容进行总结，最后再简短的一句话描述。")
                    //    .ConfigureAwait(false);
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