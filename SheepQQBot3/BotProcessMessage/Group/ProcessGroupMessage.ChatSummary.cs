using CommonLibrary;
using GenerativeAI.Types;
using Masuit.Tools;
using SheepQQBot3.DbModel;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.Model.ChatSummaryConfig;
using SheepQQBot3.Model.QQ;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yamei.Common;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Group;

public static partial class ProcessGroupMessage
{
    private const string PATH_CACHE_WORDCLOUD = "WordCloud";
    private const string PATH_WORDCLOUD_CONFIG = "WordCloud/Config";
    private static readonly Regex _regInjectHurryAndAt = new(@"\[CQ:at,qq=\d+\]|哈.{0,5}莉|雅.{0,3}美|爸.{0,3}爸", RegexOptions.IgnoreCase | RegexOptions.Multiline);

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
    private static ConcurrentDictionary<long, DateTime> _chatSummaryRequestLastTimes = [];

    private static readonly Queue<string> _repeatSkipQueue = [];

    /// <summary>
    /// 群聊总结
    /// </summary>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    /// <returns></returns>
    public static async Task<bool> ChatSummaryAsync(GroupMessage groupMessage)
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
                        if (!string.IsNullOrEmpty(botGroupMessage.MessageText) || !string.IsNullOrEmpty(botGroupMessage.MessageImage))
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
                    await BotServer.SendMessageEmojiAsync(messageId, Emoji.Coffee).ConfigureAwait(false);
                    return true;
                }

                _chatSummaryRequestLastTimes.AddOrUpdate(groupId, dateNow, dateNow);
                var summaryType = message.ToUpper().Substring(4, 1);
                var summaryWords = new Dictionary<string, int>();
                var wordCloudWidth = 1000;
                var wordNums = 100;

                var chatSummaryGroupConfigFilePath = Path.Combine(PATH_WORDCLOUD_CONFIG, $"{groupId}.json");
                var chatSummaryGroupConfig = (ChatSummaryGroupConfig)null;
                var regNumber = new Regex("[0-9]+");
                if (File.Exists(chatSummaryGroupConfigFilePath))
                {
                    var jsonText = await File.ReadAllTextAsync(chatSummaryGroupConfigFilePath, Encoding.UTF8).ConfigureAwait(false);
                    chatSummaryGroupConfig = JsonExtensions.JsonDeserialize<ChatSummaryGroupConfig>(jsonText);
                }

                switch (summaryType)
                {
                    case "A":
                        // MEMO : 自读时间不该总结...
                        if (AIStatusUtil.GetSchedule() == "masturbation time")
                        {
                            await BotServer.SendGroupMessageAsync(groupId, "我..现在正忙!..不太方便总结!").ConfigureAwait(false);
                            return true;
                        }

                        // MEMO : AI小时统计
                        var aiHourStr = message[5..];
                        var aiHour = 16;
                        if (!string.IsNullOrEmpty(aiHourStr))
                        {
                            if (!int.TryParse(aiHourStr, out aiHour))
                            {
                                await BotServer.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
                                return false;
                            }

                            if (aiHour is <= 0 or >= 24)
                            {
                                await BotServer.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_ParameterRangeError(senderId, messageId)).ConfigureAwait(false);
                                return false;
                            }
                        }

                        await BotServer.SendGroupMessageAsync(groupId, $"{CQCode.At(senderId)} 小助手正在收集聊天记录进行总结，请稍等片刻!").ConfigureAwait(false);
                        //await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                        await AISummary(groupId, dateNow.AddHours(-aiHour)).ConfigureAwait(false);

                        return true;
                    case "B":
                        if (!BotExtensions.IsAdmin(senderId))
                        {
                            await BotServer.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CanOnlyAdminUseError(senderId, messageId)).ConfigureAwait(false);
                            return false;
                        }

                        var dataMessage = message[5..];
                        if (string.IsNullOrEmpty(dataMessage))
                        {
                            await BotServer.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
                            return false;
                        }

                        if (JiebaDb.StopWords.Find(dataMessage) != null)
                        {
                            await BotServer.SendGroupMessageAsync(groupId, $"关键字[{dataMessage}]已存在于StopWords").ConfigureAwait(false);
                            return false;
                        }

                        if (!chatSummaryGroupConfig.ExcludeWords.Add(dataMessage))
                        {
                            await BotServer.SendGroupMessageAsync(groupId, $"关键字[{dataMessage}]已存在于群配置").ConfigureAwait(false);
                            return false;
                        }

                        await File.WriteAllTextAsync(chatSummaryGroupConfigFilePath, JsonSerializer.Serialize(chatSummaryGroupConfig, JsonExtensions.DefaultJsonOptions)).ConfigureAwait(false);
                        await BotServer.SendGroupMessageAsync(groupId, $"已添加统计屏蔽词[{dataMessage}]").ConfigureAwait(false);
                        return true;
                    case "H":
                        // MEMO : 小时统计
                        var hourStr = message[5..];
                        var hour = 12;
                        if (!string.IsNullOrEmpty(hourStr))
                        {
                            if (!int.TryParse(hourStr, out hour))
                            {
                                await BotServer.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
                                return false;
                            }

                            if (hour is <= 0 or >= 24)
                            {
                                await BotServer.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_ParameterRangeError(senderId, messageId)).ConfigureAwait(false);
                                return false;
                            }
                        }

                        await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                        CalcWordCloud(groupId, dateNow.AddHours(-hour));
                        wordCloudWidth = 1200;
                        wordNums = 200;
                        break;
                    case "D":
                        // MEMO : 日统计
                        await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                        CalcWordCloud(groupId, dateNow.AddDays(-1));
                        wordCloudWidth = 1500;
                        wordNums = 250;
                        break;
                    case "W":
                        // MEMO : 周统计
                        await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                        CalcWordCloud(groupId, dateNow.AddDays(-7));
                        wordCloudWidth = 1800;
                        wordNums = 300;
                        break;
                    case "M":
                        // MEMO : 月统计
                        await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                        CalcWordCloud(groupId, dateNow.AddMonths(-1));
                        wordCloudWidth = 2400;
                        wordNums = 400;
                        break;
                    case "Y":
                        // MEMO : 年统计
                        await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);
                        CalcWordCloud(414774779, dateNow.AddYears(-1));
                        wordCloudWidth = 3000;
                        wordNums = 500;
                        break;
                    default:
                        await BotServer.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
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
                await BotServer.SendGroupMessageAsync(groupId,
                    CQCode.Image(CommonExtensions.GetPath(PATH_CACHE_WORDCLOUD, wordCloudImage, GetPathType.CQCodePath))).ConfigureAwait(false);
                if (IsDebug)
                    await BotServer.SendGroupMessageAsync(groupId, "聊天记录统计已发送!").ConfigureAwait(false);

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
                                historyMessage = historyMessage.Trim();
                                if (string.IsNullOrEmpty(historyMessage))
                                    return;

                                // MEMO : 不喜欢的内容直接屏蔽
                                if (_regInjectHurryAndAt.IsMatch(historyMessage))
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
                    var groupMembers = await BotServer.GetGroupMembersAsync(targetGroupId).ConfigureAwait(false);

                    var requestContents = new List<Content>();
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
                                if (string.IsNullOrEmpty(historyMessage))
                                    return;

                                // MEMO : 不喜欢的内容直接屏蔽
                                if (_regInjectHurryAndAt.IsMatch(historyMessage))
                                    return;

                                requestContents.AddMessageContent(
                                    groupMembers.GetOrAdd(each.TargetId, new GroupMember()).ToSender(),
                                    historyMessage,
                                    AIMessageSourceType.Group);
                            });
                    }

                    var sender = groupMessage.Sender;
                    requestContents.AddSystemHint($"这些是今天的群聊内容，{sender.NickName}({sender.UserId})想让你总结一下大家都聊了些什么");
                    await requestContents.SendAsync($"z{targetGroupId}", targetGroupId, targetGroupId, false,
                        (id, msg) => BotServer.SendGroupMessageAsync(id, msg).ConfigureAwait(false))
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

    private static bool NeedRecordMessage(string message)
    {
        if (message.StartsWith("#"))
            return false;

        if (message.Contains("色图") && message.BytesCount() <= 20)
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

        return true;
    }
}