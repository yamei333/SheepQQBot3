using System.Collections.Generic;
using System.Threading.Tasks;
using SheepQQBot3.Model;
using Yamei.Common;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    public static partial class ProcessRevokeGroupMessage
    {
        private static readonly string[] _repeatSllhh = new[]
        {
            "你以为我不知道吗",
            "wzstlpmd🐱",
            "ynl!",
            "brdn!",
            "nhzs!",
            "没什么好藏的, 发出来给大伙乐乐",
            "哈莉, 撤回禁止!",
            "你撤回你🐱呢",
            "复读撤回也是一种卜鸽",
            "我的我的, 哈哈",
            "失礼了, 哈哈",
            "sll,hh",
            ";ao?",
        };

        /// <summary>
        /// 复读撤回消息
        /// </summary>
        /// <param name="groupMessage"><see cref="GroupMessage"/></param>
        /// <returns></returns>
        public static async Task<bool> RepeatRevokeMessage(GroupRevokeMessage groupRevokeMessage)
        {
            var operatorId = groupRevokeMessage.OperatorId;
            var groupId = groupRevokeMessage.GroupId;
            if (operatorId == PublicVar.AdminId)
            {
                // MEMO : ADMIN不复读撤回消息
                return true;
            }

            if (!Api.TryGetGroupMessage(groupRevokeMessage.MessageId, out var groupMessage))
                return false;

            var sender = groupMessage.Sender;
            var targetId = sender.UserId;
            var sendMessages = new List<GroupForwardMessage>
            {
                new(sender.CardName, targetId, groupMessage.Message),
                new(PublicVar.BOT_NAME, PublicVar.BotId, _repeatSllhh.Random())
            };
            await Api.SendGroupForwardMessage(groupId, sendMessages);
            return true;
        }
    }
}