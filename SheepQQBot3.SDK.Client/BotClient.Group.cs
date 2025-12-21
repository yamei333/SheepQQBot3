using CommonLibrary;
using SheepQQBot3.Model;

namespace SheepQQBot3.SDK.Client
{
    partial class BotClient
    {
        /// <summary>
        /// 获取群消息
        /// </summary>
        /// <param name="messageId">消息ID</param>
        public Task<GroupMessage> GetGroupMessageAsync(string messageId)
        {
            return SendAsync("get_msg", new ParamData
            {
                MessageId = messageId,
            }, jsonInfo =>
            {
                var clientReceiveData = jsonInfo.FromJson<ClientReceiveData>();
                return new GroupMessage(clientReceiveData.Data);
            });
        }

        /// <summary>
        /// 获取群消息历史记录
        /// </summary>
        /// <param name="groupId">群号</param>
        public Task<HistoryMessage[]> GetHistoryGroupMessagesAsync(string groupId)
        {
            return SendAsync("get_group_msg_history", new ParamData
            {
                GroupId = groupId,
            }, jsonText =>
            {
                var clientReceiveData = jsonText.FromJson<ClientReceiveData_HistoryMessages>();
                return clientReceiveData.Data.Messages;
            });
        }

        /// <summary>
        /// 踢出群
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="userId">对象QQ号</param>
        /// <param name="isReject">是否不再接受申请</param>
        public Task<bool> SetGroupKickAsync(string groupId, string userId, bool isReject = false)
            => SendAsync("set_group_kick", new ParamData
            {
                GroupId = groupId,
                UserId = userId,
                Reject_Add_Request = isReject.ToString(),
            });

        /// <summary>
        /// 群禁言
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="userId">对象QQ号</param>
        /// <param name="duration">禁言时长(单位秒), 0表示取消禁言</param>
        public Task<bool> SetGroupBanAsync(string groupId, string userId, int duration)
            => SendAsync("set_group_ban", new ParamData
            {
                GroupId = groupId,
                UserId = userId,
                Duration = duration.ToString(),
            });

        /// <summary>
        /// 群全体禁言
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="enable">是否禁言</param>
        public Task<bool> SetGroupAllBanAsync(string groupId, bool enable)
            => SendAsync("set_group_whole_ban", new ParamData
            {
                GroupId = groupId,
                Enable = enable.ToString(),
            });

        /// <summary>
        /// 设置群名片
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="userId">对象QQ号</param>
        /// <param name="card">群名片</param>
        public Task<bool> SetGroupCardAsync(string groupId, string userId, string card)
            => SendAsync("set_group_card", new ParamData
            {
                GroupId = groupId,
                Enable = userId,
                Card = card,
            });

        /// <summary>
        /// 设置群名称
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="groupName">群名称</param>
        public Task<bool> SetGroupNameAsync(string groupId, string groupName)
            => SendAsync("set_group_name", new ParamData
            {
                GroupId = groupId,
                GroupName = groupName,
            });

        /// <summary>
        /// 获得群成员名单
        /// </summary>
        /// <param name="groupId">群号</param>
        public Task<Dictionary<string, GroupMember>> GetGroupMembersAsync(string groupId)
        {
            return SendAsync("get_group_member_list", new ParamData
            {
                GroupId = groupId,
                NoCache = false,
            }, jsonText =>
            {
                var clientData = jsonText.FromJson<ClientReceiveData_GroupMember>();
                return clientData.Data.ToDictionary(each => each.UserId.ToString(), each => each);
            });
        }
    }
}