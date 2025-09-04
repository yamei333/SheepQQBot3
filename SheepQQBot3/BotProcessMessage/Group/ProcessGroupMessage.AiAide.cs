using GenerativeAI.Types;
using Masuit.Tools;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.QQ;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yamei.Common;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Group;

public static partial class ProcessGroupMessage
{
    private static readonly string _commandAI = $"[CQ:at,qq={BotId}] ";
    private static readonly Regex _regDeleteCQCode = RegexGenerator.CQDeleteCQCode();
    private static readonly Regex _regEmoji = new(@"\p{Cs}", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly ConcurrentDictionary<long, List<Content>> _historyContents = [];
    private const string GROUP_CHAT_HINT = "上面是群友最近的聊天内容，来参与群聊吧(随机发送1~3句话)";

    /// <summary>
    /// AI助手
    /// </summary>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    /// <returns></returns>
    public static async Task<bool> AiAideAsync(GroupMessage groupMessage)
    {
        var groupId = groupMessage.GroupId;
        var targetId = groupMessage.Sender.UserId;
        var message = groupMessage.Message;

        // MEMO : 字节数超过一定数量, 忽略
        if (_regDeleteCQCode.Replace(message, string.Empty).GetByteCount() > 50)
            return false;

        // MEMO : emoji数量超过一定数量, 忽略
        if (_regEmoji.Matches(message).Count >= 4)
            return false;

        var dateNow = DateTime.Now;
        if (dateNow.ToTimeStamp() <= AIExtensions.GetAIUserData(targetId).BlockUntil)
            return false;

        // MEMO :Z 开头是#表示Bot命令, 结尾是"色图"表示色图命令, 忽略
        if (message.StartsWith("#") || message.EndsWith("色图"))
            return false;

        var chatKey = $"g{groupId}";
        var isPrivateChat = message.StartsWith(_commandAI, StringComparison.CurrentCultureIgnoreCase);
        if (isPrivateChat && (dateNow - AILastRequestDates.GetOrAdd(chatKey, _ => DateTime.MinValue)).TotalSeconds < AI_REQUEST_INTERVAL_GROUP_PRIVATE)
        {
            await BotServer.SendMessageEmojiAsync(groupMessage.MessageId, Emoji.Coffee).ConfigureAwait(false);
            return false;
        }

        var useGroupChat = PublicVar.AIConfig.UseGroupChat.GetOrAdd(groupId, false);
        if (useGroupChat && !isPrivateChat)
        {
            // MEMO : 记录消息(添加到历史记录中)
            var historyContents = _historyContents.GetOrAdd(groupId, []);
            historyContents.AddMessageContent(groupMessage.Sender, message, AIMessageSourceType.Group);
            var count = historyContents.Count;
            var sendGroupChat = false;
            if (IsDebug && (count >= 7 || (groupId == TestGroupId && count >= 3)))
            {
                sendGroupChat = true;
            }
            else
            {
                sendGroupChat = count switch
                {
                    >= 50 => Rand.Next(100) >= 50,
                    >= 35 => Rand.Next(100) >= 65,
                    >= 25 => Rand.Next(100) >= 80,
                    >= 15 => Rand.Next(100) >= 90,
                    _ => false,
                };
            }

            if (sendGroupChat)
            {
                // MEMO : 发送消息
                await SendGroupAsync(historyContents).ConfigureAwait(false);
            }

            return true;
        }

        var removeAtMessage = message[_commandAI.Length..];
        if (isPrivateChat)
        {
            // MEMO : 暂时只给管理用
            if (!BotExtensions.IsAdmin(targetId))
            {
                //await BotServer.SendGroupMessageAsync(groupId, $"{CQCode.At(targetId)} 暂时不对非管理开放at回复功能").ConfigureAwait(false);
                await BotServer.SendMessageEmojiAsync(groupMessage.MessageId, Emoji.E_Sleep).ConfigureAwait(false);
                return true;
            }

            AILastRequestDates.AddOrUpdate(chatKey, dateNow, dateNow);
            // MEMO : 构建发送消息并发送
            var thisRequestContents = new List<Content>();
            thisRequestContents.AddMessageContent(groupMessage.Sender, removeAtMessage, AIMessageSourceType.Group);
            await thisRequestContents.SendAsync(
                chatKey, targetId, groupId, true,
                (id, msg) => _ = BotServer.SendGroupMessageAsync(id, msg)).ConfigureAwait(false);
        }

        return true;

        Task SendGroupAsync(List<Content> groupChatHistoryContents)
        {
            // MEMO : 清空消息
            _historyContents.AddOrUpdate(groupId, _ => [], (_, __) => []);
            // MEMO : 发送消息
            return groupChatHistoryContents.SendAsync(
                chatKey, groupId, groupId, false,
                (id, msg) => _ = BotServer.SendGroupMessageAsync(id, msg),
                contents => contents.AddSystemHint(GROUP_CHAT_HINT));
        }
    }
}