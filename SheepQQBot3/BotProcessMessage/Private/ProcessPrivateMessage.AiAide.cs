using GenerativeAI.Types;
using Masuit.Tools;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.AI;
using System;
using System.Collections.Generic;
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
        var targetId = privateMessage.Sender.UserId;
        var message = privateMessage.Message;
        var isAdmin = BotExtensions.IsAdmin(targetId);
        var isSuperAdmin = targetId == SuperAdminId;

        var dateNow = DateTime.Now;
        if (!isSuperAdmin && dateNow.ToTimeStamp() <= AIExtensions.GetAIUserData(targetId).BlockUntil)
        {
            if (IsDebug)
                await BotServer.SendPrivateMessageAsync(targetId, "你正在被屏蔽!").ConfigureAwait(false);

            return;
        }

        // MEMO : 不该发消息时, 发送不回应消息
        if (!isAdmin
            && AIExtensions.IsCantSendMessage(targetId, (id, msg) => _ = BotServer.SendGroupMessageAsync(id, msg)))
        {
            return;
        }

        // MEMO : debug时只准测试号测试
        if (IsDebug && targetId != TestQQId)
            return;

        // MEMO : 开头是#表示Bot命令, 忽略
        if (message.StartsWith("#"))
            return;

        var chatKey = $"p{targetId}";
        if ((dateNow - AILastRequestDates.GetOrAdd(chatKey, _ => DateTime.MinValue)).TotalSeconds < AI_REQUEST_INTERVAL_PRIVATE)
        {
            await BotServer.SendPrivateMessageAsync(targetId, "请求间隔过短!").ConfigureAwait(false);
            return;
        }

        AILastRequestDates.AddOrUpdate(chatKey, dateNow, dateNow);

        // MEMO : 构建发送消息并发送
        var thisRequestContents = new List<Content>();
        thisRequestContents.AddMessageContent(privateMessage.Sender, message, AIMessageSourceType.Private);
        await thisRequestContents.SendAsync(chatKey, targetId, 0, false,
            (id, msg) => _ = BotServer.SendPrivateMessageAsync(targetId, msg)).ConfigureAwait(false);
    }

    private static string GetEmojiCode(string emojiCode)
        => $"[CQ:image,file=file:///{PublicVar.AIConfig.FacePath}{emojiCode}.png]";
}