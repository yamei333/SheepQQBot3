using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.DbModel;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
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
    private const int REPEAT_SKIP = 15;

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

    private static string _lastMessage;
    //private class ChatSummaryMessage
    //{
    //    public ChatSummaryMessage(long timeStamp, string message)
    //    {
    //        TimeStamp = timeStamp;
    //        Message = message;
    //    }

    //    public long TimeStamp { get; set; }

    //    public string Message { get; set; }
    //}

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
            if (message.StartsWith("#") || message.EndsWith("色图"))
                return true;

            // MEMO : 直接衔接的复读消息不添加
            if (_lastMessage == message)
                return true;

            _lastMessage = message;
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
            var summaryType = message.ToUpper()[4..];
            var summaryWords = new Dictionary<string, int>();
            var wordCloudWidth = 1000;
            var wordNums = 100;

            var chatSummaryConfigFilePath = Path.Combine(PATH_WORDCLOUD_CONFIG, $"{groupId}.json");
            var charSummaryConfig = (ChatSummaryConfig)null;
            var regNumber = new Regex("[0-9]+");
            if (File.Exists(chatSummaryConfigFilePath))
            {
                var jsonText = await File.ReadAllTextAsync(chatSummaryConfigFilePath, Encoding.UTF8).ConfigureAwait(false);
                charSummaryConfig = JsonSerializer.Deserialize<ChatSummaryConfig>(jsonText, new JsonSerializerOptions
                {
                    IncludeFields = true,
                });
            }

            switch (summaryType)
            {
                case "D":
                    // MEMO : 日统计
                    await BotServer.SendGroupMessageAsync(groupId, "正在进行日聊天记录统计...").ConfigureAwait(false);
                    CalcWordCloud(414774779, dateNow.AddDays(-1));
                    wordCloudWidth = 1200;
                    wordNums = 150;
                    break;
                case "W":
                    // MEMO : 周统计
                    await BotServer.SendGroupMessageAsync(groupId, "正在进行周聊天记录统计...").ConfigureAwait(false);
                    CalcWordCloud(414774779, dateNow.AddDays(-7));
                    wordCloudWidth = 1500;
                    wordNums = 200;
                    break;
                case "M":
                    // MEMO : 月统计
                    await BotServer.SendGroupMessageAsync(groupId, "正在进行月聊天记录统计...(时间比较长)").ConfigureAwait(false);
                    CalcWordCloud(414774779, dateNow.AddMonths(-1));
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
                    await BotServer.SendGroupMessageAsync(groupId, GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
                    return false;
            }

            var wordCloudImage = $"{groupId}.png";
            var maskFilePath = CommonExtensions.GetPath(PATH_WORDCLOUD_CONFIG, wordCloudImage, GetPathType.Normal);
            summaryWords.OrderByDescending(each => each.Value)
                .Take(wordNums)
                .ToDictionary(each => each.Key, each => each.Value)
                .GenerateWordCloud(wordCloudWidth, wordCloudWidth,
                    CommonExtensions.GetPath(PATH_CACHE_WORDCLOUD, wordCloudImage, GetPathType.Normal), true, maskFilePath);
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
                    var processedMessage = new List<string>();
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

                            var messageCount = processedMessage.Count;
                            // MEMO : 15句以内复读则忽略
                            if (processedMessage.Skip(messageCount - REPEAT_SKIP).Any(hisMessage => hisMessage == historyMessage))
                                return;

                            processedMessage.Add(historyMessage);
                            var segmenterResult = historyMessage.ExtractTagsWithWeight_Idf();
                            var excludeWords = charSummaryConfig?.ExcludeWords;
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

        //var regNumber = new Regex(@"\d+");
        //if (regNumber.IsMatch(contentMessage))
        //{
        //    await SendRollResult(int.Parse(contentMessage)).ConfigureAwait(false);
        //}
        //else
        //{
        //    await Api.SendGroupMessageAsync(groupId, $"{CQCode.Reply(senderId, messageId)}命令格式有误!")
        //        .ConfigureAwait(false);
        //}

        //// 无匹配结果,或API超过使用次数限制
        //// 暂不处理
        //return true;

        //Task SendRollResult(int maxRollNumber)
        //{
        //    return Api.SendGroupMessageAsync(groupId,
        //        $"[{groupMessage.Sender.CardName}]的Roll点结果 {Rand.Next(maxRollNumber) + 1}");
        //}
    }
}