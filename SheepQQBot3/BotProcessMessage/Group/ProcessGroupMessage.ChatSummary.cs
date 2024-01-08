using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.DbModel;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using Yamei.Common;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Group;

public static partial class ProcessGroupMessage
{
    /// <summary>
    /// 群聊总结命令
    /// </summary>
    private const string COMMAND_CHATSUMMARY = "#ZJ#";

    /// <summary>
    /// 群聊总结
    /// </summary>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    /// <returns></returns>
    public static async Task<bool> ChatSummaryAsync(GroupMessage groupMessage)
    {
        var groupId = groupMessage.GroupId;
        var targetId = groupMessage.Sender.UserId;
        var messageId = groupMessage.MessageId;
        var timeStamp = groupMessage.DateTime.ToTimeStamp();
        var message = groupMessage.Message;

        // MEMO : 命令格式检查
        if (!message.StartsWith(COMMAND_CHATSUMMARY, StringComparison.CurrentCultureIgnoreCase))
        {
            if (message.StartsWith("#") || message.EndsWith("色图"))
                return true;

            // MEMO : 将群聊录入数据库
            lock (BotDb.SyncLock)
            {
                var botGroupMessage = BotDb.BotGroupMessages.FindAsync(groupId, targetId, messageId, timeStamp).Result;
                if (botGroupMessage == null)
                {
                    botGroupMessage = new BotGroupMessage(groupId, targetId, messageId, timeStamp, message);
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
            var summaryType = message.ToUpper()[4..];
            var summaryWords = new Dictionary<string, int>();
            switch (summaryType)
            {
                case "D":
                    // MEMO : 日统计
                    lock (BotDb.SyncLock)
                    {
                        var fromDate = dateNow.GetTodayStart();
                        var toDate = fromDate.AddDays(1);
                        var fromDateTimeStamp = fromDate.ToTimeStamp();
                        var toDateTimeStamp = toDate.ToTimeStamp();
                        BotDb.BotGroupMessages
                            .Where(each => each.GroupId == groupId
                                && each.TimeStamp >= fromDateTimeStamp
                                && each.TimeStamp < toDateTimeStamp)
                            .Select(each => each.MessageText)
                            .ForEach(each =>
                            {
                                if (string.IsNullOrEmpty(each))
                                    return;

                                var segmenterResult = each.ExtractTagsWithWeight_Idf();
                                segmenterResult.ForEach(word =>
                                    summaryWords.AddOrUpdate(word.Word, (int)(word.Weight * 100), (_, oldValue) => oldValue + (int)(word.Weight * 100)));
                            });
                    }

                    const string wordCloudImage = "wordCloud.png";
                    summaryWords.OrderByDescending(each => each.Value)
                        .Take(150)
                        .ToDictionary(each => each.Key, each => each.Value)
                        .GenerateWordCloud($"{CACHE_DIRECTORY_NAME}/{wordCloudImage}", true, "mask.png");
                    await Api.SendGroupMessageAsync(groupId,
                        CQCode.Image(CommonExtensions.GetPath(CACHE_DIRECTORY_NAME, wordCloudImage))).ConfigureAwait(false);
                    break;
            }

            return true;
        }

        //var regNumber = new Regex(@"\d+");
        //if (regNumber.IsMatch(contentMessage))
        //{
        //    await SendRollResult(int.Parse(contentMessage)).ConfigureAwait(false);
        //}
        //else
        //{
        //    await Api.SendGroupMessageAsync(groupId, $"{CQCode.Reply(targetId, messageId)}命令格式有误!")
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