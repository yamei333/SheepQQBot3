using CommonLibrary;
using Masuit.Tools;
using OpenRouter.NET.Models;
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
    private static readonly Regex _regEmoji = new(@"\p{Cs}");
    private static readonly Regex _regInjectHurry = new("哈.{0,5}莉");

    private const string GROUP_CHAT_HINT = "上面是群友最近的聊天内容，参与一下群聊(随机1~3句话)";
    //private const string GROUP_PRIVATE_CHAT_HINT = "正在向你搭话(回复随机1~2句话)";

    /// <summary>
    /// AI助手
    /// </summary>
    /// <param name="aiGroupConfig"><see cref="AIGroupConfig"/></param>
    /// <param name="blackListUserConfig"></param>
    /// <param name="groupMembers"></param>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    public static async Task AIAideAsync(
        AIGroupConfig aiGroupConfig,
        BlackListUserConfig blackListUserConfig,
        Dictionary<string, GroupMember> groupMembers,
        GroupMessage groupMessage)
    {
        var groupId = groupMessage.GroupId;
        var sender = groupMessage.Sender;
        var targetId = sender.UserId.ToString();
        var message = groupMessage.Message;

        // MEMO : 字节数超过一定数量(设定数字/3), 忽略
        if (!BotExtensions.IsAdmin(targetId) && _regDeleteCQCode.Replace(message, string.Empty).GetByteCount() > 90)
        {
            //YameiLogExtensions.WriteLog(LogType.Info, $"忽略群消息(字数太多): {message}");
            return;
        }

        // MEMO : emoji数量超过一定数量
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
            await GlobalBotClient.SendMessageEmojiAsync(groupMessage.MessageId, Emoji.Coffee).ConfigureAwait(false);
            return;
        }

        if (aiGroupConfig.JoinGroupChat && !isPrivateChat)
        {
            if (blackListUserConfig.BanedAICollect)
                return;

            // MEMO : 日程在深度睡眠时, 不接收消息
            if (AIStatusUtil.GetSchedule().Contains("deep sleep"))
                return;

            // MEMO : 记录消息(添加到历史记录中)
            var historyMessages = AIHistoryContentParts.GetOrAdd(groupId, []);
            await historyMessages.AddQQChatMessageAsync(sender, message, groupMembers).ConfigureAwait(false);

            //YameiLogExtensions.WriteLog(LogType.Info, $"群({groupId})消息记录数: {historyContents.Count}");
            if (CanSendGroupChat(aiGroupConfig, historyMessages.Count))
            {
                // MEMO : 某些时间不该发消息
                if (AIExtensions.IsCantSendMessage(string.Empty, (id, msg) => _ = GlobalBotClient.SendGroupMessageAsync(id, msg)))
                    return;

                // MEMO : 发送消息
                await SendGroupAsync(historyMessages).ConfigureAwait(false);
            }

            return;
        }

        //var removeAtMessage = message[_commandAI.Length..].TrimStart();
        if (isPrivateChat && aiGroupConfig.UseAtResponse)
        {
            if (blackListUserConfig.BanedAIResponse)
                return;

            // MEMO : 是否只给管理用
            if (aiGroupConfig.AtResponseAdminOnly && !BotExtensions.IsAdmin(targetId))
            {
                await GlobalBotClient.SendMessageEmojiAsync(groupMessage.MessageId, Emoji.Moyu).ConfigureAwait(false);
                return;
            }

            // MEMO : 某些时间不该发消息
            if (AIExtensions.IsCantSendMessage(groupId, (id, msg) => _ = GlobalBotClient.SendGroupMessageAsync(id, msg)))
                return;

            await GlobalBotClient.SendMessageEmojiAsync(groupMessage.MessageId, Emoji.E_Flash).ConfigureAwait(false);

            AILastRequestDates.AddOrUpdate(chatKey, dateNow, dateNow);
            // MEMO : 获得现有的缓存群消息
            var historyContentParts = AIHistoryContentParts.GetOrAdd(groupId, []);

            // MEMO : 判断使用模型(开头是/image)
            var useModelImage = false;
            message = Regex.Replace(message, @"(?<=\[CQ:at,qq=(?<qqId>\d+)\]\s*)/image\s*", match =>
            {
                useModelImage = true;
                return string.Empty;
            }, RegexOptions.IgnoreCase);
            // MEMO : 构建发送消息并发送
            await historyContentParts.AddQQChatMessageAsync(sender, message, groupMembers).ConfigureAwait(false);
            //historyContents.AddSystemHint($"[QQID:{targetId}] {GROUP_PRIVATE_CHAT_HINT}");
            //historyContents.AddSystemHint($"{sender.NickName}(QQID:{sender.UserId}){GROUP_PRIVATE_CHAT_HINT}");
            //var groupMembers = await GlobalBotClient.GetGroupMembersAsync(groupId).ConfigureAwait(false);
            await historyContentParts.SendAsync(
                chatKey, targetId, groupId, true, groupMembers.ToSenderDictionary(AIUserInfos), aiGroupConfig,
                (id, msg) => _ = GlobalBotClient.SendGroupMessageAsync(id, msg),
                useModelImage ? GlobalAIConfig.ModelImage : GlobalAIConfig.ModelChat).ConfigureAwait(false);
        }

        return;

        Task SendGroupAsync(List<ContentPart> groupChatHistoryContentParts)
        {
            // MEMO : 清空消息
            AIHistoryContentParts.AddOrUpdate(groupId, _ => [], (_, __) => []);
            // MEMO : 发送消息
            return groupChatHistoryContentParts.SendAsync(
                chatKey, groupId, groupId, false, groupMembers.ToSenderDictionary(AIUserInfos), aiGroupConfig,
                (id, msg) => _ = GlobalBotClient.SendGroupMessageAsync(
                    aiGroupConfig.JoinGroupChatSendToTestGroup ? TestGroupId : id, msg),
                GlobalAIConfig.ModelChat,
                GROUP_CHAT_HINT);
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