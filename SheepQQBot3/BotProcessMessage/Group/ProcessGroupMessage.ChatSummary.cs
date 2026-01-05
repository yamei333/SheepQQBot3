using CommonLibrary;
using Masuit.Tools;
using OpenAI.Chat;
using SheepQQBot3.DbModel;
using SheepQQBot3.Enums;
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
    /// <summary>
    /// 复读忽略设置
    /// </summary>
    private const int REPEAT_SKIP_LIMIT = 25;

    /// <summary>
    /// 群聊消息总结最低消息数
    /// </summary>
    private const int SUMMARY_MESSAGE_COUNT_LIMIT = 25;

    /// <summary>
    /// 统计忽略词长
    /// </summary>
    private const int CHAT_SUMMARY_LIMIT_BYTE = 150;

    /// <summary>
    /// 群聊总结命令
    /// </summary>
    private const string COMMAND_CHAT_SUMMARY = "#ZJ#";

    /// <summary>
    /// 群聊总结命令CD (30分钟)
    /// </summary>
    private const int CHAT_SUMMARY_TO_FAST_TIMES = 1800;

    /// <summary>
    /// 群聊总结最后一次执行时间
    /// </summary>
    private static readonly ConcurrentDictionary<string, DateTime> _chatSummaryRequestLastTimes = [];

    private static readonly Queue<string> _repeatSkipQueueGroupSummary = [];

    /// <summary>
    /// 群聊总结
    /// </summary>
    /// <param name="aiGroupConfig"><see cref="AIGroupConfig"/></param>
    /// <param name="blackListUserConfig"><see cref="BlackListUserConfig"/></param>
    /// <param name="groupMembers"></param>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    /// <returns></returns>
    public static async Task ChatSummaryAsync(
        AIGroupConfig aiGroupConfig,
        BlackListUserConfig blackListUserConfig,
        Dictionary<string, GroupMember> groupMembers,
        GroupMessage groupMessage)
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
                if (blackListUserConfig.BanedChatSummaryCollect)
                    return;

                var deleteCQMessage = message.Replace(_regCQCode, string.Empty);
                // MEMO : 复读消息不添加
                if (_repeatSkipQueueGroupSummary.Contains(deleteCQMessage))
                    return;

                if (BotExtensions.NeedNotRecordMessage(message))
                    return;

                _repeatSkipQueueGroupSummary.Enqueue(deleteCQMessage);
                if (_repeatSkipQueueGroupSummary.Count > REPEAT_SKIP_LIMIT)
                    _repeatSkipQueueGroupSummary.Dequeue();

                var addBotGroupMessage = new BotGroupMessage(groupId, senderId, messageId, timeStamp, message);
                var processedMessage = addBotGroupMessage.MessageText.Replace(_regCQCode, string.Empty);
                // 去掉CQCode之后内容过多的(认定是转发复读)
                if (processedMessage.GetByteCount() > CHAT_SUMMARY_LIMIT_BYTE)
                    addBotGroupMessage.MessageText = "[转发消息]";

                // MEMO : 将群聊录入数据库
                await using var botDb = DbExtensions.CreateBotDbContext();
                if (await botDb.BotGroupMessages.FindAsync(groupId, senderId, messageId, timeStamp).ConfigureAwait(false) == null)
                    await botDb.AddAsync(addBotGroupMessage).ConfigureAwait(false);

                return;
            }

            if (blackListUserConfig.BanedChatSummary)
                return;

            if (message.Length < 4)
                return;

            var dateNow = DateTime.Now;
            if (!BotExtensions.IsAdmin(senderId)
                && (dateNow - _chatSummaryRequestLastTimes.GetOrAdd(groupId, DateTime.MinValue)).TotalSeconds < CHAT_SUMMARY_TO_FAST_TIMES)
            {
                await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.Coffee).ConfigureAwait(false);
                return;
            }

            _chatSummaryRequestLastTimes.AddOrUpdate(groupId, dateNow, dateNow);
            if (message.Length == 4)
            {
                await AISummary(16, "一天").ConfigureAwait(false);
                return;
            }

            var summaryType = message.ToUpper().Substring(4, 1);
            switch (summaryType)
            {
                case "A":
                    await AISummary(16, "一天").ConfigureAwait(false);
                    return;
                default:
                    var match = Regex.Match(message[4..], @"\d+");
                    if (match.Success)
                    {
                        await AISummary(int.Parse(match.Value), $"{match}小时").ConfigureAwait(false);
                        return;
                    }

                    await GlobalBotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CommandTypeError(senderId, messageId)).ConfigureAwait(false);
                    return;
            }

            async Task AISummary(int aiHour, string description = "")
            {
                // MEMO : 某些时间不该发消息
                if (AIExtensions.IsCantSendMessage(groupId, (id, msg) => _ = GlobalBotClient.SendGroupMessageAsync(id, msg)))
                    return;

                // MEMO : AI小时统计
                if (aiHour <= 0)
                {
                    await GlobalBotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_ParameterRangeError(senderId, messageId)).ConfigureAwait(false);
                    return;
                }

                await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.E_Flash).ConfigureAwait(false);
                await AISummaryCore(groupId, description, dateNow.AddHours(-aiHour)).ConfigureAwait(false);
                return;
            }

            // MEMO : AI群聊总结
            async Task AISummaryCore(string targetGroupId, string description, DateTime fromDate, DateTime? toDate = null)
            {
                if (IsDebug)
                    targetGroupId = "414774779";

                var thisRequestParts = new List<ChatMessageContentPart>();
                thisRequestParts.AddSystemHint($"[以下是最近{description}的群聊内容]");
                var fromDateTimeStamp = fromDate.ToTimeStamp();
                var toDateTimeStamp = (toDate ?? dateNow).ToTimeStamp();
                await using var botDb = DbExtensions.CreateBotDbContext();
                botDb.BotGroupMessages
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

                        thisRequestParts.AddQQChatMessage(groupMembers[each.TargetId].ToAIChatSender(AIUserInfos), historyMessage, groupMembers);
                    });

#if !DEBUG
                if (thisRequestParts.Count <= SUMMARY_MESSAGE_COUNT_LIMIT + 1)
                {
                    await GlobalBotClient.SendGroupMessageAsync(targetGroupId, $"群聊消息过少(少于{SUMMARY_MESSAGE_COUNT_LIMIT}条)! 不需要总结!").ConfigureAwait(false);
                    return;
                }
#endif

                thisRequestParts.AddSystemHint($"[群聊内容到此为止]");

                var sender = groupMessage.Sender;
                await thisRequestParts.AddQQChatMessageAsync(sender,
                    $@"{CQCode.At(BotId)} {AppSettingExtensions.Get("chatSummaryPrompt")}", groupMembers, true).ConfigureAwait(false);
                await thisRequestParts.SendAsync($"z{targetGroupId}", targetGroupId, targetGroupId, false, groupMembers.ToSenderDictionary(AIUserInfos), aiGroupConfig,
                        (id, msg) => GlobalBotClient.SendGroupMessageAsync(id, msg).ConfigureAwait(false),
                        GlobalAIConfig.ModelSummary, AIRequestType.ChatSummary)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteLog(e);
        }
    }
}