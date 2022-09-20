using SheepQQBot3.Model;
using Yamei.Common;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    public static partial class ProcessRevokeGroupMessage
    {
        /// <summary>
        /// 复读撤回消息
        /// </summary>
        /// <param name="groupMessage"><see cref="GroupMessage"/></param>
        /// <returns></returns>
        public static bool RepeatRevokeMessage(GroupMessage groupMessage)
        {
            Api.SendGroupMessage(groupMessage.GroupId, GetRevokeMessage());

            return true;

            string GetRevokeMessage()
            {
                var sender = groupMessage.Sender;
                var message = groupMessage.Message;
                var nickName = sender.NickName;
                var userId = sender.User_Id;
                var messageHead = $"{nickName}({userId})";
                return new[]
                {
                    $"{messageHead} 撤回了消息\n{message}\n你以为我不知道吗",
                    $"{messageHead} 撤回了消息\n{message}\nwzstlpmdm",
                    $"{messageHead} 撤回了消息\n{message}\n没什么好藏的, 发出来给大伙乐乐",
                    $"{messageHead} 撤回了消息\n{message}\n哈莉, 撤回禁止!",
                    $"{messageHead} 撤回了消息\n{message}\n你撤回你猫呢",
                }.Random();
            }
        }
    }
}