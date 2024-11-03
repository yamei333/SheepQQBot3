using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Extension;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Group;

public static partial class ProcessRevokeGroupMessage
{
    //private static readonly string[] _repeatSllhh = new[]
    //{
    //    "你以为我不知道吗",
    //    "wzstlpmd🐱",
    //    "ynl!",
    //    "brdn!",
    //    "nhzs!",
    //    "没什么好藏的, 发出来给大伙乐乐",
    //    "哈莉, 撤回禁止!",
    //    "你撤回你🐱呢",
    //    "复读撤回也是一种卜鸽",
    //    "我的我的, 哈哈",
    //    "失礼了, 哈哈",
    //    "sll,hh",
    //    ";ao?",
    //};

    /// <summary>
    /// 复读撤回消息
    /// </summary>
    /// <param name="groupRevokeMessage"><see cref="GroupRevokeMessage"/></param>
    /// <returns></returns>
    public static async Task<bool> RepeatRevokeMessageAsync(GroupRevokeMessage groupRevokeMessage)
    {
        if (!IsDebug && BotExtensions.IsAdmin(groupRevokeMessage.OperatorId))
        {
            // MEMO : ADMIN不复读撤回消息
            return true;
        }

        // MEMO : 没取到, 说明消息已经过太久了
        if (!SavedGroupMessages.TryRemove(groupRevokeMessage.MessageId, out var revokeMessage))
            return true;

        var sender = revokeMessage.Sender;
        var sendMessages = new List<GroupForwardMessage>
        {
            //new(sender.CardName, targetId, groupMessage.Message),
            new(BOT_NAME, BotId, $"{sender.NickName}({sender.UserId})"),
            new(BOT_NAME, BotId, CQCode.ReplaceCQImage(revokeMessage.Message)),
            // MEMO : 0.14.3.8 不发送嘲讽消息
            //new(BOT_NAME, BotId, _repeatSllhh.Random()),
        };
        await BotServer.SendGroupForwardMessageAsync(groupRevokeMessage.GroupId, sendMessages).ConfigureAwait(false);
        return true;
    }
}