using CommonLibrary;
using GenerativeAI.Types;
using Masuit.Tools;
using SheepQQBot3.DbModel;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.Model.ChatSummaryConfig;
using SheepQQBot3.Model.QQ;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yamei.Common;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Group;

public static partial class ProcessGroupMessage
{
    private static readonly Regex _regReplaceCQCode = RegexGenerator.ReplaceCQCode();
    private const string PATH_CACHE_WORDCLOUD = "WordCloud";
    private const string PATH_WORDCLOUD_CONFIG = "WordCloud/Config";

    /// <summary>
    /// 复读忽略设置
    /// </summary>
    private const int REPEAT_SKIP = 25;

    /// <summary>
    /// 复读忽略设置(统计时)
    /// </summary>
    private const int REPEAT_SKIP_SUMMARY = 5;

    /// <summary>
    /// 统计忽略词长
    /// </summary>
    private const int CHATSUMMARY_BYTELIMIT = 100;

    /// <summary>
    /// 群聊总结命令
    /// </summary>
    private const string COMMAND_CHATSUMMARY = "#ZJ#";

    /// <summary>
    /// 群聊总结命令CD (60分钟)
    /// </summary>
    private const int CHATSUMMARY_TOFASTTIMES = 3600;

    /// <summary>
    /// 群聊总结最后一次执行时间
    /// </summary>
    private static readonly ConcurrentDictionary<long, DateTime> _chatSummaryRequestLastTimes = [];

    private static readonly Queue<string> _repeatSkipQueue = [];

    /// <summary>
    /// 群聊总结
    /// </summary>
    /// <param name="aiGroupConfig"><see cref="AIGroupConfig"/></param>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    /// <returns></returns>
    public static async Task<bool> ChatSummaryAsync(AIGroupConfig aiGroupConfig, GroupMessage groupMessage)
    {
        var groupId = groupMessage.GroupId;
        var senderId = groupMessage.Sender.UserId;
        var messageId = groupMessage.MessageId;
        var timeStamp = groupMessage.DateTime.ToTimeStamp();
        var message = groupMessage.Message;

        try
        {
            // MEMO : 命令格式检查
            if (!message.StartsWith(COMMAND_CHATSUMMARY, StringComparison.CurrentCultureIgnoreCase))
            {
                if (!NeedRecordMessage(message))
                    return true;

                // MEMO : 复读消息不添加
                if (_repeatSkipQueue.Contains(message))
                    return true;

                var addBotGroupMessage = new BotGroupMessage(groupId, senderId, messageId, timeStamp, message);
                if (addBotGroupMessage.MessageText.GetByteCount() > CHATSUMMARY_BYTELIMIT)
                    return true;

                _repeatSkipQueue.Enqueue(message);
                if (_repeatSkipQueue.Count > REPEAT_SKIP)
                    _repeatSkipQueue.Dequeue();

                // MEMO : 将群聊录入数据库
                lock (BotDb.SyncLock)
                {
                    var botGroupMessage = BotDb.BotGroupMessages.FindAsync(groupId, senderId, messageId, timeStamp).Result;
                    if (botGroupMessage == null)
                    {
                        botGroupMessage = new BotGroupMessage(groupId, senderId, messageId, timeStamp, message);
                        if (!botGroupMessage.MessageText.IsNullOrEmpty() || !botGroupMessage.MessageImage.IsNullOrEmpty())
                            BotDb.AddAsync(botGroupMessage);
                    }
                }

                return true;
            }
            else
            {
                if (message.Length <= 4)
                    return false;

                var dateNow = DateTime.Now;
                if (!BotExtensions.IsAdmin(senderId)
                    && (dateNow - _chatSummaryRequestLastTimes.GetOrAdd(groupId, DateTime.MinValue)).TotalSeconds < CHATSUMMARY_TOFASTTIMES)
                {
                    await BotClient.SendMessageEmojiAsync(messageId, Emoji.Coffee).ConfigureAwait(false);
                    return true;
                }

                _chatSummaryRequestLastTimes.AddOrUpdate(groupId, dateNow, dateNow);
                var summaryType = message.ToUpper().Substring(4, 1);
                var summaryWords = new Dictionary<string, int>();
                var wordCloudWidth = 1000;
                var wordNums = 100;

                var chatSummaryGroupConfigFilePath = Path.Combine(PATH_WORDCLOUD_CONFIG, $"{groupId}.json");
                var chatSummaryGroupConfig = JsonExtensions.FromJsonFile<ChatSummaryGroupConfig>(chatSummaryGroupConfigFilePath);
                var regNumber = new Regex("[0-9]+");

                switch (summaryType)
                {
                    case "A":
                        // MEMO : 某些时间不该发消息
                        if (AIExtensions.IsCantSendMessage(groupId, (id, msg) => _ = BotClient.SendGroupMessageAsync(id, msg)))
                            return true;

                        // MEMO : AI小时统计
                        var aiHourStr = message[5..];
                        var aiHour = 16;
                        if (!aiHourStr.IsNullOrEmpty())
                        {
                            if (!int.TryParse(aiHourStr, out aiHour))
                            {
                                await BotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
                                return false;
                            }

                            if (aiHour is <= 0 or >= 24)
                            {
                                await BotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_ParameterRangeError(senderId, messageId)).ConfigureAwait(false);
                                return false;
                            }
                        }

                        await BotClient.SendGroupMessageAsync(groupId, $"{CQCode.At(senderId)} 小助手正在收集聊天记录进行总结，请稍等片刻!").ConfigureAwait(false);
                        //await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                        await AISummary(groupId, dateNow.AddHours(-aiHour)).ConfigureAwait(false);

                        return true;
                    case "B":
                        if (!BotExtensions.IsAdmin(senderId))
                        {
                            await BotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CanOnlyAdminUseError(senderId, messageId)).ConfigureAwait(false);
                            return false;
                        }

                        var dataMessage = message[5..];
                        if (dataMessage.IsNullOrEmpty())
                        {
                            await BotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
                            return false;
                        }

                        if (await JiebaDb.StopWords.FindAsync(dataMessage).ConfigureAwait(false) != null)
                        {
                            await BotClient.SendGroupMessageAsync(groupId, $"关键字[{dataMessage}]已存在于StopWords").ConfigureAwait(false);
                            return false;
                        }

                        if (!chatSummaryGroupConfig.ExcludeWords.Add(dataMessage))
                        {
                            await BotClient.SendGroupMessageAsync(groupId, $"关键字[{dataMessage}]已存在于群配置").ConfigureAwait(false);
                            return false;
                        }

                        await File.WriteAllTextAsync(chatSummaryGroupConfigFilePath, chatSummaryGroupConfig.ToJsonIgnoreNull()).ConfigureAwait(false);
                        await BotClient.SendGroupMessageAsync(groupId, $"已添加统计屏蔽词[{dataMessage}]").ConfigureAwait(false);
                        return true;
                    case "H":
                        // MEMO : 小时统计
                        var hourStr = message[5..];
                        var hour = 12;
                        if (!hourStr.IsNullOrEmpty())
                        {
                            if (!int.TryParse(hourStr, out hour))
                            {
                                await BotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
                                return false;
                            }

                            if (hour is <= 0 or >= 24)
                            {
                                await BotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_ParameterRangeError(senderId, messageId)).ConfigureAwait(false);
                                return false;
                            }
                        }

                        await BotClient.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                        CalcWordCloud(groupId, dateNow.AddHours(-hour));
                        wordCloudWidth = 1200;
                        wordNums = 200;
                        break;
                    case "D":
                        // MEMO : 日统计
                        await BotClient.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                        CalcWordCloud(groupId, dateNow.AddDays(-1));
                        wordCloudWidth = 1500;
                        wordNums = 250;
                        break;
                    case "W":
                        // MEMO : 周统计
                        await BotClient.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                        CalcWordCloud(groupId, dateNow.AddDays(-7));
                        wordCloudWidth = 1800;
                        wordNums = 300;
                        break;
                    case "M":
                        // MEMO : 月统计
                        await BotClient.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                        CalcWordCloud(groupId, dateNow.AddMonths(-1));
                        wordCloudWidth = 2400;
                        wordNums = 400;
                        break;
                    case "Y":
                        // MEMO : 年统计
                        await BotClient.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                        CalcWordCloud(414774779, dateNow.AddYears(-1));
                        wordCloudWidth = 3000;
                        wordNums = 500;
                        break;
                    default:
                        await BotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
                        return false;
                }

                var wordCloudImage = $"{groupId}.png";
                var maskFilePath = CommonExtensions.GetPath(PATH_WORDCLOUD_CONFIG, wordCloudImage, GetPathType.Normal);
                var wordCloudWords = summaryWords.OrderByDescending(each => each.Value)
                    .Take(wordNums)
                    .ToDictionary(each => each.Key, each => each.Value);

                wordCloudWords.GenerateWordCloud(wordCloudWidth, wordCloudWidth,
                    CommonExtensions.GetPath(PATH_CACHE_WORDCLOUD, wordCloudImage, GetPathType.Normal), true, maskFilePath);
                await File.WriteAllTextAsync(Path.Combine(PATH_CACHE_WORDCLOUD, $"{groupId}.txt"), string.Join("\r\n", wordCloudWords.Keys)).ConfigureAwait(false);
                await BotClient.SendGroupMessageAsync(groupId,
                    CQCode.Image(CommonExtensions.GetPath(PATH_CACHE_WORDCLOUD, wordCloudImage, GetPathType.CQCodePath))).ConfigureAwait(false);
                if (IsDebug)
                    await BotClient.SendGroupMessageAsync(groupId, "聊天记录统计已发送!").ConfigureAwait(false);

                return true;

                void CalcWordCloud(long targetGroupId, DateTime fromDate, DateTime? toDate = null)
                {
                    lock (BotDb.SyncLock)
                    {
                        var fromDateTimeStamp = fromDate.ToTimeStamp();
                        var toDateTimeStamp = (toDate ?? dateNow).ToTimeStamp();
                        var repeatSkipQueue = new Queue<string>();
                        BotDb.BotGroupMessages
                            .Where(each => each.GroupId == targetGroupId
                                && each.TimeStamp >= fromDateTimeStamp
                                && each.TimeStamp < toDateTimeStamp)
                            .ForEach(each =>
                            {
                                var historyMessage = each.MessageText;
                                historyMessage = historyMessage.Trim().ToUpper();
                                if (historyMessage.IsNullOrEmpty())
                                    return;

                                // MEMO : 不喜欢的内容直接屏蔽
                                if (_regInjectHurry.IsMatch(historyMessage))
                                    return;

                                // MEMO : REPEAT_SKIP_SUMMARY 句以内复读则忽略
                                if (repeatSkipQueue.Contains(historyMessage))
                                    return;

                                repeatSkipQueue.Enqueue(historyMessage);
                                if (repeatSkipQueue.Count > REPEAT_SKIP_SUMMARY)
                                    repeatSkipQueue.Dequeue();

                                var segmenterResult = historyMessage.ExtractTagsWithWeight_Idf();
                                var excludeWords = chatSummaryGroupConfig?.ExcludeWords;
                                segmenterResult
                                    .Where(wordWeightPair =>
                                    {
                                        var word = wordWeightPair.Word;
                                        if (regNumber.Match(word).Value == word)
                                            return false;

                                        return excludeWords == null || !excludeWords.Contains(word);
                                    })
                                    .ForEach(wordWeightPair => summaryWords
                                        .AddOrUpdate(wordWeightPair.Word, (int)(wordWeightPair.Weight * 100), (_, oldValue) => oldValue + (int)(wordWeightPair.Weight * 100)));
                            });
                    }
                }

                async Task AISummary(long targetGroupId, DateTime fromDate, DateTime? toDate = null)
                {
                    var groupMembers = await BotClient.GetGroupMembersAsync(targetGroupId).ConfigureAwait(false);
                    if (groupMembers == null)
                    {
                        await BotClient.SendGroupMessageAsync(targetGroupId, "群成员信息获取失败!").ConfigureAwait(false);
                        return;
                    }

                    var requestContents = new List<Content>();
                    requestContents.AddSystemHint($"[以下是今天的群聊内容]");
                    lock (BotDb.SyncLock)
                    {
                        var fromDateTimeStamp = fromDate.ToTimeStamp();
                        var toDateTimeStamp = (toDate ?? dateNow).ToTimeStamp();
                        BotDb.BotGroupMessages
                            .Where(each => each.GroupId == targetGroupId
                                && each.TimeStamp >= fromDateTimeStamp
                                && each.TimeStamp < toDateTimeStamp)
                            .ForEach(each =>
                            {
                                var historyMessage = each.MessageText;
                                historyMessage = historyMessage.Trim();
                                if (historyMessage.IsNullOrEmpty())
                                    return;

                                // MEMO : 不喜欢的内容直接屏蔽
                                if (_regInjectHurry.IsMatch(historyMessage))
                                    return;

                                _ = requestContents.AddMessageContentAsync(
                                    each.TargetId,
                                    //groupMembers.GetOrAdd(each.TargetId, new GroupMember()).ToSender(),
                                    historyMessage);
                            });
                    }

                    var sender = groupMessage.Sender;

                    requestContents.AddSystemHint($"[群聊内容到此为止] {sender.NickName}(QQ:{sender.UserId})想让你总结一下大家都聊了什么，先对不同内容进行总结，最后再简短的一句话描述。");
                    await requestContents.SendAsync($"z{targetGroupId}", targetGroupId, targetGroupId, false, groupMembers.ToSenderDictionary(), aiGroupConfig,
                        (id, msg) => BotClient.SendGroupMessageAsync(id, msg).ConfigureAwait(false))
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

        var uMessage = message.ToUpper();
        if (uMessage.EndsWith("色图")
            || uMessage.EndsWith("色图L")
            || uMessage.EndsWith("色图N")
            || uMessage.EndsWith("色图J")
            || uMessage.EndsWith("色图Y")
            || uMessage.EndsWith("色图S"))
        {
            return false;
        }

        // MEMO : 去除所有CQ码之后无任何内容的消息
        if (_regReplaceCQCode.Replace(message, string.Empty).Trim().IsNullOrEmpty())
            return false;

        return true;
    }
}