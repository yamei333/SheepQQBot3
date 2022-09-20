using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using Yamei.Common;
using static System.Threading.Tasks.Task;

namespace SheepQQBot3.SDK.Client
{
    partial class CQAPI
    {
        /// <summary>
        /// 发送群消息
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="message">消息内容</param>
        /// <param name="setConfigs">已设定的消息内容, 用于消息重定义</param>
        public async Task SendGroupMessage(long groupId, string message, Dictionary<Guid, SetConfig> setConfigs = null)
        {
            var messageList = MessageUtil.ProcessCQMessage(message);
            var redirectData = messageList.FirstOrDefault(each => each.Type == "ym_redirect");
            if (redirectData != null)
            {
                if (Guid.TryParse(redirectData.Data.Data, out var redirectId))
                {
                    var alarmAideConfigs = setConfigs
                        .SelectMany(each => each.Value.AlarmAideConfigs.Values)
                        .ToDictionary(each => each.Id, each => each);
                    if (!alarmAideConfigs.TryGetValue(redirectId, out var alarmAideConfig))
                        return;

                    var alarmTexts = alarmAideConfig.AlarmTexts;
                    if (alarmTexts.Count > 0)
                        await SendGroupMessage(groupId, alarmTexts.Values.Random()).ConfigureAwait(false);

                    return;
                }
                else
                {
                    // MEMO : 数据不准确, 发送错误信息
                    messageList = MessageUtil.ProcessCQMessage("ym_redirect 数据不正确");
                }
            }

            var idleData = messageList.FirstOrDefault(each => each.Type == "ym_ifnotidle");
            if (idleData != null)
            {
                if (int.TryParse(idleData.Data.Data, out var overTime))
                {
                    if (CommonUtil.GetIsNotIdle(overTime))
                    {
                        // MEMO : 忙时, 触发
                        //messageList.Add(new Element("text", new ElementBaseData($"闲时:{CommonUtil.GetIdleTime()}")));
                        messageList.Remove(idleData);
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
            }

            await SendDataAsync("send_group_msg", new ParamData
            {
                group_id = groupId.ToString(),
                message = messageList
            });
        }

        /// <summary>
        /// 发送群消息
        /// </summary>
        /// <param name="userId">群号</param>
        /// <param name="message">消息内容</param>
        public async Task SendPrivateMessage(long userId, string message)
            => await SendDataAsync("send_private_msg", new ParamData
            {
                user_id = userId.ToString(),
                message = MessageUtil.ProcessCQMessage(message)
            });

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="messageType"><see cref="MessageType"/></param>
        /// <param name="targetId">群号</param>
        /// <param name="message">消息内容</param>
        public async void SendMessage(MessageType messageType, long targetId, string message)
        {
            switch (messageType)
            {
                case MessageType.Private:
                    await Run(() => SendPrivateMessage(targetId, message));
                    break;
                case MessageType.Group:
                    await Run(() => SendGroupMessage(targetId, message));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }
        }

        /// <summary>
        /// 撤回消息
        /// </summary>
        /// <param name="messageId">消息ID</param>
        public async void DeleteMessage(int messageId)
            => await SendDataAsync("delete_msg", new ParamData
            {
                message_id = messageId.ToString()
            });

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <param name="messageId">消息ID</param>
        public async void GetMessage(int messageId)
            => await SendDataAsync("get_msg", new ParamData
            {
                message_id = messageId.ToString()
            });

        /// <summary>
        /// 发送好友赞
        /// </summary>
        /// <param name="userId">对象QQ</param>
        /// <param name="times">点赞次数</param>
        public async void SendLike(long userId, int times)
            => await SendDataAsync("send_like", new ParamData
            {
                user_id = userId.ToString(),
                times = times.ToString()
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
                group_id = groupId.ToString(),
                user_id = userId.ToString(),
                reject_add_request = isReject.ToString()
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
                group_id = groupId.ToString(),
                user_id = userId.ToString(),
                duration = duration.ToString()
            });

        /// <summary>
        /// 群全体禁言
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="enable">是否禁言</param>
        public async void SetGroupAllBan(long groupId, bool enable)
            => await SendDataAsync("set_group_whole_ban", new ParamData
            {
                group_id = groupId.ToString(),
                enable = enable.ToString()
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
                group_id = groupId.ToString(),
                enable = userId.ToString(),
                card = card
            });

        /// <summary>
        /// 设置群名称
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="groupName">群名称</param>
        public async void SetGroupName(long groupId, string groupName)
            => await SendDataAsync("set_group_name", new ParamData
            {
                group_id = groupId.ToString(),
                group_name = groupName
            });
    }
}