using Masuit.Tools;
using SheepQQBot3.Model;
using System.Linq;
using System.Threading.Tasks;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Group;

public static partial class ProcessGroupMessage
{
    /// <summary>
    /// 最大群消息缓存数(复读撤回消息用)
    /// </summary>
    private const int MAX_CACHE_MESSAGE_COUNT = 10;

    /// <summary>
    /// 复读撤回消息
    /// 记录群发送消息的内容
    /// </summary>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    /// <returns></returns>
    public static async Task<bool> RepeatRevokeMessageAsync(GroupMessage groupMessage)
    {
        var groupId = groupMessage.GroupId;
        var targetId = groupMessage.Sender.UserId;
        var messageId = groupMessage.MessageId;

        SavedGroupMessages.TryAdd(messageId, groupMessage);
        if (SavedGroupMessages.Count >= MAX_CACHE_MESSAGE_COUNT)
        {
            var removedMessageId = SavedGroupMessages.Values.OrderBy(message => message.DateTime).First().MessageId;
            SavedGroupMessages.TryRemove(removedMessageId, out _);
            //SavedGroupMessages = SavedGroupMessages.CopyRemove(
            //    SavedGroupMessages.Values.Order(message => message.First().MessageId);
        }
        // 暂不处理
        return true;
    }
}