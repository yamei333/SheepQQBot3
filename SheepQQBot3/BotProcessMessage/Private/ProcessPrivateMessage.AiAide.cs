using Masuit.Tools;
using OpenRouter.NET.Models;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.AI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yamei.Common;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Private;

public static partial class ProcessPrivateMessage
{
    /// <summary>
    /// AI助手
    /// </summary>
    /// <param name="privateMessage"><see cref="PrivateMessage"/></param>
    /// <returns></returns>
    public static async Task AIAideAsync(PrivateMessage privateMessage)
    {
        var sender = privateMessage.Sender;
        var targetId = sender.UserId.ToString();
        var message = privateMessage.Message;
        var isAdmin = BotExtensions.IsAdmin(targetId);
        var isSuperAdmin = targetId == SuperAdminId;

        var dateNow = DateTime.Now;
        if (!isSuperAdmin && dateNow.ToTimeStamp() <= AIExtensions.GetAIUserData(targetId).BlockUntil)
        {
            if (IsDebug)
                await GlobalBotClient.SendPrivateMessageAsync(targetId, "你正在被屏蔽!").ConfigureAwait(false);

            return;
        }

        // MEMO : 不该发消息时, 发送不回应消息
        if (!isAdmin && AIExtensions.IsCantSendMessage(targetId, (id, msg) => _ = GlobalBotClient.SendGroupMessageAsync(id, msg)))
            return;

        // MEMO : debug时只准测试号测试
        if (IsDebug && targetId != TestQQId)
            return;

        // MEMO : 开头是#表示Bot命令, 忽略
        if (message.StartsWith("#"))
            return;

        var chatKey = $"p{targetId}";
        if ((dateNow - AILastRequestDates.GetOrAdd(chatKey, _ => DateTime.MinValue)).TotalSeconds < AI_REQUEST_INTERVAL_PRIVATE)
        {
            await GlobalBotClient.SendPrivateMessageAsync(targetId, "请求间隔过短!").ConfigureAwait(false);
            return;
        }

        AILastRequestDates.AddOrUpdate(chatKey, dateNow, dateNow);

        // MEMO : 判断使用模型(开头是/image)
        var useModelImage = message.StartsWith("/image", StringComparison.CurrentCultureIgnoreCase);
        message = Regex.Replace(message, @"^/image\s*", "", RegexOptions.IgnoreCase);

        // MEMO : 构建发送消息并发送
        var thisRequestContentParts = new List<ContentPart>();
        await thisRequestContentParts.AddQQChatMessageAsync(sender, message, null).ConfigureAwait(false);

        var aiChatSenders = new ConcurrentDictionary<string, AIChatSender>();
        aiChatSenders.GetOrAdd(targetId, privateMessage.Sender.ToAIChatSender(AIUserInfos));

        await thisRequestContentParts.SendAsync(chatKey, targetId, string.Empty, false, aiChatSenders, null,
            (id, msg) => _ = GlobalBotClient.SendPrivateMessageAsync(targetId, msg),
            useModelImage ? GlobalAIConfig.ModelImage : GlobalAIConfig.ModelChat).ConfigureAwait(false);
    }
}