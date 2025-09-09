using CommonLibrary;
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
    private static readonly string _commandAI = $"[CQ:at,qq={BotId}]";
    private static readonly Regex _regDeleteCQCode = RegexGenerator.CQDeleteCQCode();
    private static readonly Regex _regEmoji = new(@"\p{Cs}", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly Regex _regInjectHurry = new("哈.{0,5}莉|雅.{0,3}美|爸.{0,3}爸", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly ConcurrentDictionary<long, List<Content>> _historyContents = [];

    private const string GROUP_CHAT_HINT = "上面是群友最近的聊天内容，来参与群聊吧(随机1~3句话)";
    private const string GROUP_PRIVATE_CHAT_HINT = "正在向你搭话，回复一下吧(随机1~3句话)";

    /// <summary>
    /// AI助手
    /// </summary>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    /// <returns></returns>
    public static async Task AiAideAsync(GroupMessage groupMessage)
    {
        var groupId = groupMessage.GroupId;
        var targetId = groupMessage.Sender.UserId;
        var message = groupMessage.Message;

        // MEMO : 字节数超过一定数量(设定数字/3), 忽略
        if (!BotExtensions.IsAdmin(targetId) && _regDeleteCQCode.Replace(message, string.Empty).GetByteCount() > 90)
        {
            //YameiLogExtensions.WriteLog(LogType.Info, $"忽略群消息(字数太多): {message}");
            return;
        }

        // MEMO : emoji数量超过一定数量, 忽略
        if (_regEmoji.Matches(message).Count >= 4)
        {
            YameiLogExtensions.WriteLog(LogType.Info, $"忽略群消息(emoji太多): {message}");
            return;
        }

        // MEMO : 注入攻击, 忽略
        if (!BotExtensions.IsAdmin(targetId) && _regInjectHurry.IsMatch(message))
        {
            YameiLogExtensions.WriteLog(LogType.Info, $"忽略群消息(注入): {message}");
            return;
        }

        var dateNow = DateTime.Now;
        if (dateNow.ToTimeStamp() <= AIExtensions.GetAIUserData(targetId).BlockUntil)
        {
            //YameiLogExtensions.WriteLog(LogType.Info, $"忽略群消息(拉黑): {message}");
            return;
        }

        // MEMO : 开头是#表示Bot命令, 结尾是"色图"表示色图命令, 单"r"是roll点命令, 忽略
        if (message.StartsWith("#") || message.EndsWith("色图") || message.ToLower() == "r")
        {
            //YameiLogExtensions.WriteLog(LogType.Info, $"忽略群消息(bot命令): {message}");
            return;
        }

        var chatKey = $"g{groupId}";
        var isPrivateChat = message.StartsWith(_commandAI, StringComparison.CurrentCultureIgnoreCase);
        if (isPrivateChat && (dateNow - AILastRequestDates.GetOrAdd(chatKey, _ => DateTime.MinValue)).TotalSeconds < AI_REQUEST_INTERVAL_GROUP_PRIVATE)
        {
            await BotServer.SendMessageEmojiAsync(groupMessage.MessageId, Emoji.Coffee).ConfigureAwait(false);
            return;
        }

        var useGroupChat = PublicVar.AIConfig.UseGroupChat.GetOrAdd(groupId, false);
        if (useGroupChat && !isPrivateChat)
        {
            // MEMO : 日程在深度睡眠时, 不接收消息
            if (AIStatusUtil.GetSchedule().Contains("deep sleep"))
                return;

            // MEMO : 记录消息(添加到历史记录中)
            var historyContents = _historyContents.GetOrAdd(groupId, []);
            historyContents.AddMessageContent(groupMessage.Sender, message, AIMessageSourceType.Group);
            var count = historyContents.Count;
            var sendGroupChat = false;
            //YameiLogExtensions.WriteLog(LogType.Info, $"记录群消息: 当前记录数量{count}, 内容:{message}.");
            if (IsDebug && (count >= 7 || (groupId == TestGroupId && count >= 3)))
            {
                sendGroupChat = true;
            }
            else
            {
                sendGroupChat = count switch
                {
                    >= 50 => Rand.Next(100) >= 50,
                    >= 35 => Rand.Next(100) >= 75,
                    >= 25 => Rand.Next(100) >= 80,
                    >= 15 => Rand.Next(100) >= 90,
                    _ => false,
                };
            }

            if (sendGroupChat)
            {
                // MEMO : 某些时间不该发消息
                if (AIExtensions.IsCantSendMessage(0, (id, msg) => _ = BotServer.SendGroupMessageAsync(id, msg)))
                    return;

                // MEMO : 发送消息
                await SendGroupAsync(historyContents).ConfigureAwait(false);
            }

            return;
        }

        var removeAtMessage = message[_commandAI.Length..].TrimStart();
        if (isPrivateChat)
        {
            // MEMO : 暂时只给管理用
            if (!BotExtensions.IsAdmin(targetId))
            {
                //await BotServer.SendGroupMessageAsync(groupId, $"{CQCode.At(targetId)} 暂时不对非管理开放at回复功能").ConfigureAwait(false);
                await BotServer.SendMessageEmojiAsync(groupMessage.MessageId, Emoji.Moyu).ConfigureAwait(false);
                return;
            }

            // MEMO : 某些时间不该发消息
            if (AIExtensions.IsCantSendMessage(groupId, (id, msg) => _ = BotServer.SendGroupMessageAsync(id, msg)))
                return;

            await BotServer.SendMessageEmojiAsync(groupMessage.MessageId, Emoji.E_Flash).ConfigureAwait(false);

            AILastRequestDates.AddOrUpdate(chatKey, dateNow, dateNow);
            // MEMO : 获得现有的缓存群消息
            var historyContents = _historyContents.GetOrAdd(groupId, []);
            var sender = groupMessage.Sender;
            // MEMO : 构建发送消息并发送
            historyContents.AddMessageContent(sender, removeAtMessage, AIMessageSourceType.Group);
            historyContents.AddSystemHint($"{sender.NickName}(QQId:{sender.UserId}){GROUP_PRIVATE_CHAT_HINT}");
            await historyContents.SendAsync(
                chatKey, targetId, groupId, true,
                (id, msg) => _ = BotServer.SendGroupMessageAsync(id, msg)).ConfigureAwait(false);
        }

        return;

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