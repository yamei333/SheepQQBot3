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
    public static async Task<bool> AIAideAsync(PrivateMessage privateMessage)
    {
        var targetId = privateMessage.Sender.UserId;
        var message = privateMessage.Message;

        // MEMO : 日程在深度睡眠时, 不回应
        if (!BotExtensions.IsAdmin(targetId) && AIStatusUtil.GetSchedule() == "deep sleep time")
        {
            await BotServer.SendPrivateMessageAsync(targetId, "Zzz...").ConfigureAwait(false);
            return true;
        }

        var dateNow = DateTime.Now;
        if (dateNow.ToTimeStamp() <= AIExtensions.GetAIUserData(targetId).BlockUntil)
        {
            if (IsDebug)
                await BotServer.SendPrivateMessageAsync(targetId, "你正在被屏蔽!").ConfigureAwait(false);

            return false;
        }

        if (IsDebug && targetId != TestQQId)
            return false;

        // MEMO : 开头是#表示Bot命令, 忽略
        if (message.StartsWith("#"))
            return false;

        var chatKey = $"p{targetId}";
        if ((dateNow - AILastRequestDates.GetOrAdd(chatKey, _ => DateTime.MinValue)).TotalSeconds < AI_REQUEST_INTERVAL_PRIVATE)
        {
            await BotServer.SendPrivateMessageAsync(targetId, "请求间隔过短!").ConfigureAwait(false);
            return false;
        }

        AILastRequestDates.AddOrUpdate(chatKey, dateNow, dateNow);

        // MEMO : 构建发送消息并发送
        var thisRequestContents = new List<Content>();
        thisRequestContents.AddMessageContent(privateMessage.Sender, message, AIMessageSourceType.Private);
        await thisRequestContents.SendAsync(chatKey, targetId, 0, false,
            (id, msg) => _ = BotServer.SendPrivateMessageAsync(targetId, msg)).ConfigureAwait(false);

        return true;
    }

    private static string GetEmojiCode(string emojiCode)
        => $"[CQ:image,file=file:///{PublicVar.AIConfig.FacePath}{emojiCode}.png]";
}