using CommonLibrary;
using Masuit.Tools;
using OpenAI.Chat;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.QQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yamei.Common;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Group;

public static partial class ProcessGroupMessage
{
    private static readonly string _commandAI = $"[CQ:at,qq={BotId}]";
    private static readonly Regex _regInjectHurry = new("哈.{0,5}莉");

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
        var dateNow = DateTime.Now;

        var chatKey = $"g{groupId}";
        var isPrivateChat = message.StartsWith(_commandAI, StringComparison.CurrentCultureIgnoreCase);
        if (isPrivateChat && (dateNow - AILastRequestDates.GetOrAdd(chatKey, _ => DateTime.MinValue)).TotalSeconds < AI_REQUEST_INTERVAL_GROUP_PRIVATE)
        {
            await GlobalBotClient.SendMessageEmojiAsync(groupMessage.MessageId, Emoji.Coffee).ConfigureAwait(false);
            return;
        }

        if (aiGroupConfig.JoinGroupChat && !isPrivateChat)
        {
            // MEMO : 日程在深度睡眠时, 不接收消息
            if (AIStatusUtil.GetSchedule().Contains("deep sleep"))
                return;

            if (blackListUserConfig.BanedAICollect)
                return;

            if (NeedNotRecordMessage(msg =>
            {
                // MEMO : 注入攻击
                if (!BotExtensions.IsAdmin(targetId) && _regInjectHurry.IsMatch(msg))
                {
                    YameiLogExtensions.WriteLog(LogType.Info, $"忽略群消息(注入): {msg}");
                    return true;
                }

                return false;
            }))
            {
                return;
            }

            // MEMO : 除CQ段以外, 字节数超过一定数量(1汉字=3字节), 认为是转发消息
            // MEMO : at消息则无此限制
            if (_regCQCode.Replace(message, string.Empty).GetByteCount() > CHAT_SUMMARY_LIMIT_BYTE)
                message = "[转发消息]";

            // MEMO : 记录消息(添加到历史记录中)
            var historyParts = AIHistoryParts.GetOrAdd(groupId, []);
            await historyParts.AddQQChatMessageAsync(sender, message, groupMembers, imageNumLimit: 3).ConfigureAwait(false);

            var messageCount = historyParts.Count(part => part.Kind == ChatMessageContentPartKind.Text);
            if (CanSendGroupChat(aiGroupConfig, messageCount))
            {
                YameiLogExtensions.WriteLog(LogType.Info, $"AI群消息发送(触发消息数: {messageCount})");
                // MEMO : 发送消息
                await SendGroupAsync(historyParts, groupId, false, GlobalAIConfig.ModelChat, AIRequestType.Chat).ConfigureAwait(false);
            }

            return;
        }

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
            var historyContentParts = AIHistoryParts.GetOrAdd(groupId, []);
            // MEMO : 判断使用模型(开头是/image)
            var useModelImage = _regCQCode.Replace(message, string.Empty).Trim().StartsWith("/image", StringComparison.CurrentCultureIgnoreCase);
            // MEMO : 构建发送消息并发送
            await historyContentParts.AddQQChatMessageAsync(sender, message, groupMembers, imageNumLimit: 1).ConfigureAwait(false);
            await SendGroupAsync(
                historyContentParts,
                targetId,
                true,
                useModelImage ? GlobalAIConfig.ModelImage : GlobalAIConfig.ModelChat,
                useModelImage ? AIRequestType.Image : AIRequestType.Chat)
                .ConfigureAwait(false);
        }

        return;

        Task SendGroupAsync(
            List<ChatMessageContentPart> groupChatHistoryParts,
            string requestTargetId,
            bool isAt,
            AIModel aiModel,
            AIRequestType requestType)
        {
            // MEMO : 清空消息
            AIHistoryParts.AddOrUpdate(groupId, _ => [], (_, __) => []);
            // MEMO : 发送消息
            return groupChatHistoryParts.SendAsync(
                chatKey, requestTargetId, groupId, isAt, groupMembers.ToSenderDictionary(AIUserInfos), aiGroupConfig,
                (id, msg) => _ = GlobalBotClient.SendGroupMessageAsync(
                    aiGroupConfig.JoinGroupChatSendToTestGroup ? TestGroupId : id, msg),
                aiModel,
                requestType,
                isAt ? null : AppSettingExtensions.Get("groupChatPrompt"));
        }

        bool NeedNotRecordMessage(Func<string, bool> otherCheckFunc = null)
        {
            return BotExtensions.NeedNotRecordMessage(message, msg =>
            {
                // MEMO : 被Bot拉黑
                if (dateNow.ToTimeStamp() <= AIExtensions.GetAIUserData(targetId).BlockUntil)
                    return true;

                if (otherCheckFunc?.Invoke(msg) == true)
                    return true;

                return false;
            });
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