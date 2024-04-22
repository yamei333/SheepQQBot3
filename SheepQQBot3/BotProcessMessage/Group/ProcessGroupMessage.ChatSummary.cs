using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.DbModel;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Model.ChatSummaryConfig;
using System;
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

    /// <summary>
    /// 复读忽略设置
    /// </summary>
    private const int REPEAT_SKIP = 25;

    /// <summary>
    /// 统计忽略词长
    /// </summary>
    private const int CHATSUMMARY_BYTELIMIT = 100;

    /// <summary>
    /// 群聊总结命令
    /// </summary>
    private const string COMMAND_CHATSUMMARY = "#ZJ#";

    /// <summary>
    /// 群聊总结命令CD (15分钟)
    /// </summary>
    private const int CHATSUMMARY_TOFASTTIMES = 900;

    /// <summary>
    /// 群聊总结最后一次执行时间
    /// </summary>
    private static DateTime _chatSummaryRequestLastTime = DateTime.MinValue;

    private static Queue<string> _repeatSkipQueue = new();

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
            if (!BotExtensions.IsAdmin(senderId) && (dateNow - _chatSummaryRequestLastTime).TotalSeconds < CHATSUMMARY_TOFASTTIMES)
            {
                await BotServer.SendGroupMessageAsync(groupId, "请求统计CD中, 过一会再试吧!..").ConfigureAwait(false);
                return true;
            }

            _chatSummaryRequestLastTime = dateNow;
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
                chatSummaryGroupConfig = JsonSerializer.Deserialize<ChatSummaryGroupConfig>(jsonText, new JsonSerializerOptions
                {
                    IncludeFields = true,
                });
            }

            switch (summaryType)
            {
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

                    File.WriteAllText(chatSummaryGroupConfigFilePath, JsonSerializer.Serialize(chatSummaryGroupConfig, CommonExtensions.DefaultJsonOptions));
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

                    await BotServer.SendGroupMessageAsync(groupId, $"正在进行{hour}小时聊天记录统计...").ConfigureAwait(false);
                    CalcWordCloud(groupId, dateNow.AddHours(-hour));
                    wordCloudWidth = 800;
                    wordNums = 100;
                    break;
                case "D":
                    // MEMO : 日统计
                    await BotServer.SendGroupMessageAsync(groupId, "正在进行日聊天记录统计...").ConfigureAwait(false);
                    CalcWordCloud(groupId, dateNow.AddDays(-1));
                    wordCloudWidth = 1200;
                    wordNums = 150;
                    break;
                case "W":
                    // MEMO : 周统计
                    await BotServer.SendGroupMessageAsync(groupId, "正在进行周聊天记录统计...").ConfigureAwait(false);
                    CalcWordCloud(groupId, dateNow.AddDays(-7));
                    wordCloudWidth = 1500;
                    wordNums = 200;
                    break;
                case "M":
                    // MEMO : 月统计
                    await BotServer.SendGroupMessageAsync(groupId, "正在进行月聊天记录统计...(时间比较长)").ConfigureAwait(false);
                    CalcWordCloud(groupId, dateNow.AddMonths(-1));
                    wordCloudWidth = 2000;
                    wordNums = 250;
                    break;
                case "Y":
                    // MEMO : 年统计
                    await BotServer.SendGroupMessageAsync(groupId, "正在进行年聊天记录统计...(时间比较长)").ConfigureAwait(false);
                    CalcWordCloud(414774779, dateNow.AddYears(-1));
                    wordCloudWidth = 3000;
                    wordNums = 300;
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

            wordCloudWords
                .GenerateWordCloud(wordCloudWidth, wordCloudWidth,
                    CommonExtensions.GetPath(PATH_CACHE_WORDCLOUD, wordCloudImage, GetPathType.Normal), true, maskFilePath);
            File.WriteAllText(Path.Combine(PATH_CACHE_WORDCLOUD, $"{groupId}.txt"), string.Join("\r\n", wordCloudWords.Keys));

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

                            // MEMO : REPEAT_SKIP 句以内复读则忽略
                            if (repeatSkipQueue.Contains(historyMessage))
                                return;

                            repeatSkipQueue.Enqueue(historyMessage);
                            if (repeatSkipQueue.Count > REPEAT_SKIP)
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
        }
    }

    private static bool NeedRecordMessage(string message)
    {
        if (message.StartsWith("#"))
            return false;

        if (message.Contains("色图") && message.BytesCount() <= 10)
            return false;

        var uMessage = message.ToUpper();
        if (uMessage.EndsWith("色图L")
            || uMessage.EndsWith("色图N")
            || uMessage.EndsWith("色图J")
            || uMessage.EndsWith("色图Y"))
        {
            return false;
        }

        return true;
    }
}