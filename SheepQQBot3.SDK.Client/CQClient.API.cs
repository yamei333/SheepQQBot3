using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using Yamei.Common;
using static System.Threading.Tasks.Task;

namespace SheepQQBot3.SDK.Client
{
    partial class CQAPI
    {
        private readonly Dictionary<Guid, string> _interaciveJsons = new();

        private readonly Regex _regGetEcho = RegexGenerator.CQAPI_GetEcho();

        /// <summary>
        /// 发送群消息
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="message">消息内容</param>
        /// <param name="setConfigs">已设定的消息内容, 用于消息重定义</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="callBack">回调</param>
        public async Task SendGroupMessageAsync(
            long groupId, string message,
            Dictionary<Guid, SetConfig> setConfigs = null,
            double timeout = 5,
            Action<ClientReceiveData> callBack = null)
        {
            var messageList = MessageUtil.ProcessCQMessage(message);

            ProcessYmMessage(ElementType.ym_redirect, ProcessYmRedirect);
            ProcessYmMessage(ElementType.ym_ifnotidle, ymElement =>
            {
                if (int.TryParse(ymElement.Data.Data, out var overTime))
                {
                    if (CommonUtil.GetIsNotIdle(overTime))
                    {
                        // MEMO : 忙时, 触发
                        //messageList.Add(new Element("text", new ElementBaseData($"闲时:{CommonUtil.GetIdleTime()}")));
                    }
                    else
                    {
                        // MEMO : 闲时, 不触发
                        return;
                    }
                }
                else
                {
                    // MEMO : 数据不准确, 发送错误信息
                    messageList = MessageUtil.ProcessCQMessage("ym_ifnotidle 数据不正确");
                }
            });
            ProcessYmMessage(ElementType.ym_bark, ProcessYmBark);

            if (messageList?.Any() != true)
                return;

            var echo = Guid.NewGuid();
            await SendDataAsync("send_group_msg", new ParamData
            {
                GroupId = groupId.ToString(),
                Message = messageList
            }, echo).ConfigureAwait(false);

            callBack?.Invoke(GetReply(echo, jsonInfo => JsonSerializer.Deserialize<ClientReceiveData>(jsonInfo), timeout));

            void ProcessYmMessage(ElementType ymElementType, Action<Element> action)
            {
                var ymElementData = messageList.FirstOrDefault(each => each.Type == ymElementType);
                if (ymElementData != null)
                {
                    messageList.Remove(ymElementData);
                    action(ymElementData);
                }
            }

            async void ProcessYmRedirect(Element ymElement)
            {
                if (Guid.TryParse(ymElement.Data.Data, out var redirectId))
                {
                    var alarmAideConfigs = setConfigs!.SelectMany(each => each.Value.AlarmAideConfigs.Values)
                        .ToDictionary(each => each.Id, each => each);
                    if (!alarmAideConfigs.TryGetValue(redirectId, out var alarmAideConfig)) return;

                    var alarmTexts = alarmAideConfig.AlarmTexts;
                    if (alarmTexts?.Count > 0)
                        await SendGroupMessageAsync(groupId, alarmTexts.Values.Random()).ConfigureAwait(false);
                }
                else
                {
                    // MEMO : 数据不准确, 发送错误信息
                    messageList = MessageUtil.ProcessCQMessage("ym_redirect 数据不正确");
                }
            }

            async void ProcessYmBark(Element ymElement)
            {
                var elementData = ymElement.Data;
                if (!string.IsNullOrEmpty(elementData.Data))
                {
                    await PushExtensions.PushBarkMessageAsync(elementData.Title, elementData.Content).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// 发送群消息
        /// </summary>
        /// <param name="userId">群号</param>
        /// <param name="message">消息内容</param>
        public Task SendPrivateMessageAsync(long userId, string message)
            => SendDataAsync("send_private_msg", new ParamData
            {
                UserId = userId.ToString(),
                Message = MessageUtil.ProcessCQMessage(message)
            });

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="type"><see cref="ElementType"/></param>
        /// <param name="targetId">群号</param>
        /// <param name="message">消息内容</param>
        public async void SendMessage(MessageTargetType type, long targetId, string message)
        {
            switch (type)
            {
                case MessageTargetType.Private:
                    await Run(() => SendPrivateMessageAsync(targetId, message)).ConfigureAwait(false);
                    break;
                case MessageTargetType.Group:
                    await Run(() => SendGroupMessageAsync(targetId, message)).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// 发送合并转发群消息
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="messages">消息内容</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="callBack">回调</param>
        public async Task SendGroupForwardMessageAsync(
            long groupId,
            IEnumerable<GroupForwardMessage> messages,
            double timeout = 5,
            Action<ClientReceiveData> callBack = null)
        {
            var echo = Guid.NewGuid();
            await SendDataAsync("send_group_forward_msg", new GroupForwardMessageParamData
            {
                GroupId = groupId.ToString(),
                Messages = messages
                    .Select(each => new GroupForwardMessageElement(each))
                    .ToList()
            }, echo).ConfigureAwait(false);
            callBack?.Invoke(GetReply(echo, jsonInfo => JsonSerializer.Deserialize<ClientReceiveData>(jsonInfo), timeout));
        }

        /// <summary>
        /// 撤回消息
        /// </summary>
        /// <param name="messageId">消息ID</param>
        public Task DeleteMessageAsync(int messageId)
            => SendDataAsync("delete_msg", new ParamData
            {
                MessageId = messageId.ToString()
            });

        /// <summary>
        /// 获取群消息
        /// </summary>
        /// <param name="messageId">消息ID</param>
        /// <param name="timeout">超时时间</param>
        public async Task<GroupMessage> GetGroupMessageAsync(int messageId, double timeout = 5)
        {
            var echo = Guid.NewGuid();
            await SendDataAsync("get_msg", new ParamData
            {
                MessageId = messageId.ToString()
            }, echo).ConfigureAwait(false);

            var groupMessage = GetReply(echo, jsonInfo =>
            {
                var clientReceiveData = JsonSerializer.Deserialize<ClientReceiveData>(jsonInfo);
                return new GroupMessage(clientReceiveData.Data);
            }, timeout);
            return groupMessage;
        }

        /// <summary>
        /// 获取群消息历史记录
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="timeout">超时时间</param>
        public async Task<HistoryMessage[]> GetHistoryGroupMessagesAsync(long groupId, double timeout = 5)
        {
            var echo = Guid.NewGuid();
            await SendDataAsync("get_group_msg_history", new ParamData
            {
                GroupId = groupId.ToString()
            }, echo).ConfigureAwait(false);

            return GetReply(echo, jsonInfo =>
            {
                var clientReceiveData = JsonSerializer.Deserialize<ClientReceiveData_HistoryMessages>(jsonInfo);
                return clientReceiveData.Data.Messages;
            }, timeout);
        }

        /// <summary>
        /// 发送好友赞
        /// </summary>
        /// <param name="userId">对象QQ</param>
        /// <param name="times">点赞次数</param>
        public async void SendLike(long userId, int times)
            => await SendDataAsync("send_like", new ParamData
            {
                UserId = userId.ToString(),
                Times = times.ToString()
            });

        /// <summary>
        /// 踢出群
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="userId">对象QQ号</param>
        /// <param name="isReject">是否不再接受申请</param>
        public async void SetGroupKick(long groupId, long userId, bool isReject = false)
            => await SendDataAsync("set_group_kick", new ParamData
            {
                GroupId = groupId.ToString(),
                UserId = userId.ToString(),
                Reject_Add_Request = isReject.ToString()
            });

        /// <summary>
        /// 群禁言
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="userId">对象QQ号</param>
        /// <param name="duration">禁言时长(单位秒), 0表示取消禁言</param>
        public async void SetGroupBan(long groupId, long userId, int duration)
            => await SendDataAsync("set_group_ban", new ParamData
            {
                GroupId = groupId.ToString(),
                UserId = userId.ToString(),
                Duration = duration.ToString()
            });

        /// <summary>
        /// 群全体禁言
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="enable">是否禁言</param>
        public async void SetGroupAllBan(long groupId, bool enable)
            => await SendDataAsync("set_group_whole_ban", new ParamData
            {
                GroupId = groupId.ToString(),
                Enable = enable.ToString()
            });

        /// <summary>
        /// 设置群名片
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="userId">对象QQ号</param>
        /// <param name="card">群名片</param>
        public async void SetGroupCard(long groupId, int userId, string card)
            => await SendDataAsync("set_group_card", new ParamData
            {
                GroupId = groupId.ToString(),
                Enable = userId.ToString(),
                Card = card
            });

        /// <summary>
        /// 设置群名称
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="groupName">群名称</param>
        public async void SetGroupName(long groupId, string groupName)
            => await SendDataAsync("set_group_name", new ParamData
            {
                GroupId = groupId.ToString(),
                GroupName = groupName
            });

        /// <summary>
        /// 获得群成员名单
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="timeout">超时时间</param>
        [CQAPI_Interactive]
        public async Task<Dictionary<long, GroupMember>> GetGroupMembersAsync(long groupId, double timeout = 5)
        {
            var echo = Guid.NewGuid();
            await SendDataAsync("get_group_member_list", new ParamData
            {
                GroupId = groupId.ToString(),
                NoCache = false
            }, echo).ConfigureAwait(false);

            return GetReply(echo, jsonText =>
            {
                var clientData = JsonSerializer.Deserialize<ClientReceiveData_GroupMember>(jsonText);
                return clientData.Data.ToDictionary(each => each.UserId, each => each);
            }, timeout);
        }

        private T GetReply<T>(Guid echo, Func<string, T> getFunc, double timeout)
            where T : class
        {
            SpinWait.SpinUntil(() => _interaciveJsons.ContainsKey(echo), TimeSpan.FromSeconds(timeout));
            if (!_interaciveJsons.TryGetValue(echo, out var jsonText))
                return null;

            _interaciveJsons.Remove(echo);
            return getFunc(jsonText);
        }
    }
}