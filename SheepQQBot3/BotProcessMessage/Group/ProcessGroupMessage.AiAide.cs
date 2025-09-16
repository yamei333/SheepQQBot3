using CommonLibrary;
using GenerativeAI.Types;
using Masuit.Tools;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.QQ;
using System;
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
    private static readonly Regex _regEmoji = new(@"\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly Regex _regInjectHurry = new("哈.{0,5}莉", RegexOptions.IgnoreCase | RegexOptions.Multiline);

    private const string GROUP_CHAT_HINT = "上面是群友最近的聊天内容，参与一下群聊(随机1~3句话)";
    private const string GROUP_PRIVATE_CHAT_HINT = "正在向你搭话(回复随机1~2句话)";

    /// <summary>
    /// AI助手
    /// </summary>
    /// <param name="aiGroupConfig"><see cref="AIGroupConfig"/></param>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    public static async Task AIAideAsync(AIGroupConfig aiGroupConfig, GroupMessage groupMessage)
    {
        var groupId = groupMessage.GroupId;
        var targetId = groupMessage.Sender.UserId;
        var message = groupMessage.Message;

        if (aiGroupConfig.BlackListIds.Contains(targetId))
            return;

        // MEMO : 字节数超过一定数量(设定数字/3), 忽略
        if (!BotExtensions.IsAdmin(targetId) && _regDeleteCQCode.Replace(message, string.Empty).GetByteCount() > 90)
        {
            //YameiLogExtensions.WriteLog(LogType.Info, $"忽略群消息(字数太多): {message}");
            return;
        }

        // MEMO : emoji数量超过一定数量
        if (_regEmoji.Matches(message).Count >= 6)
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

        if (aiGroupConfig.JoinGroupChat && !isPrivateChat)
        {
            // MEMO : 日程在深度睡眠时, 不接收消息
            if (AIStatusUtil.GetSchedule().Contains("deep sleep"))
                return;

            // MEMO : 记录消息(添加到历史记录中)
            var historyContents = AIHistoryContents.GetOrAdd(groupId, []);
            historyContents.AddMessageContent(targetId, message);

            //YameiLogExtensions.WriteLog(LogType.Info, $"群({groupId})消息记录数: {historyContents.Count}");
            if (CanSendGroupChat(aiGroupConfig, historyContents.Count))
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
        if (isPrivateChat && aiGroupConfig.UseAtResponse)
        {
            // MEMO : 是否只给管理用
            if (aiGroupConfig.AtResponseAdminOnly && !BotExtensions.IsAdmin(targetId))
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
            var historyContents = AIHistoryContents.GetOrAdd(groupId, []);
            //var sender = groupMessage.Sender;
            // MEMO : 构建发送消息并发送
            historyContents.AddMessageContent(targetId, removeAtMessage);
            historyContents.AddSystemHint($"[QQID:{targetId}] {GROUP_PRIVATE_CHAT_HINT}");
            //historyContents.AddSystemHint($"{sender.NickName}(QQID:{sender.UserId}){GROUP_PRIVATE_CHAT_HINT}");
            var groupMembers = await BotServer.GetGroupMembersAsync(groupId).ConfigureAwait(false);
            await historyContents.SendAsync(
                chatKey, targetId, groupId, true, groupMembers.ToSenderDictionary(), aiGroupConfig,
                (id, msg) => _ = BotServer.SendGroupMessageAsync(id, msg)).ConfigureAwait(false);
        }

        return;

        async Task SendGroupAsync(List<Content> groupChatHistoryContents)
        {
            // MEMO : 清空消息
            AIHistoryContents.AddOrUpdate(groupId, _ => [], (_, __) => []);
            var groupMembers = await BotServer.GetGroupMembersAsync(groupId).ConfigureAwait(false);
            // MEMO : 发送消息
            await groupChatHistoryContents.SendAsync(
                chatKey, groupId, groupId, false, groupMembers.ToSenderDictionary(), aiGroupConfig,
                (id, msg) => _ = BotServer.SendGroupMessageAsync(
                    aiGroupConfig.JoinGroupChatSendToTestGroup ? TestGroupId : id, msg),
                addSystemHint: contents => contents.AddSystemHint(GROUP_CHAT_HINT)).ConfigureAwait(false);
        }
    }

    private static bool CanSendGroupChat(AIGroupConfig aiGroupConfig, int count)
    {
        if (count >= aiGroupConfig.GroupChatResponseLimit100)
            return true;
        if (count >= aiGroupConfig.GroupChatResponseLimit50)
            return Rand.CheckPercent(50);
        if (count >= aiGroupConfig.GroupChatResponseLimit35)
            return Rand.CheckPercent(35);
        if (count >= aiGroupConfig.GroupChatResponseLimit20)
            return Rand.CheckPercent(20);
        if (count >= aiGroupConfig.GroupChatResponseLimit10)
            return Rand.CheckPercent(10);
        if (count >= aiGroupConfig.GroupChatResponseLimit5)
            return Rand.CheckPercent(5);

        return false;
    }
}