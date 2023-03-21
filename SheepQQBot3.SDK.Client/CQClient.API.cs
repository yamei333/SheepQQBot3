using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>
        /// 发送群消息
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="message">消息内容</param>
        /// <param name="setConfigs">已设定的消息内容, 用于消息重定义</param>
        public async Task SendGroupMessage(long groupId, string message, Dictionary<Guid, SetConfig> setConfigs = null)
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

            if (messageList?.Any() == true)
            {
                await SendDataAsync("send_group_msg", new ParamData
                {
                    Group_Id = groupId.ToString(),
                    Message = messageList
                });
            }

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
                    var alarmAideConfigs = setConfigs.SelectMany(each => each.Value.AlarmAideConfigs.Values)
                        .ToDictionary(each => each.Id, each => each);
                    if (!alarmAideConfigs.TryGetValue(redirectId, out var alarmAideConfig)) return;

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

            async void ProcessYmBark(Element ymElement)
            {
                var elementData = ymElement.Data;
                if (!string.IsNullOrEmpty(elementData.Data))
                {
                    await PushExtensions.PushBarkMessageAsync(elementData.Title, elementData.Content);
                }
            }
        }

        /// <summary>
        /// 发送群消息
        /// </summary>
        /// <param name="userId">群号</param>
        /// <param name="message">消息内容</param>
        public async Task SendPrivateMessage(long userId, string message)
            => await SendDataAsync("send_private_msg", new ParamData
            {
                User_Id = userId.ToString(),
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
                    await Run(() => SendPrivateMessage(targetId, message));
                    break;
                case MessageTargetType.Group:
                    await Run(() => SendGroupMessage(targetId, message));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// 撤回消息
        /// </summary>
        /// <param name="messageId">消息ID</param>
        public async void DeleteMessage(int messageId)
            => await SendDataAsync("delete_msg", new ParamData
            {
                Message_Id = messageId.ToString()
            });

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <param name="messageId">消息ID</param>
        public async void GetMessage(int messageId)
            => await SendDataAsync("get_msg", new ParamData
            {
                Message_Id = messageId.ToString()
            });

        /// <summary>
        /// 发送好友赞
        /// </summary>
        /// <param name="userId">对象QQ</param>
        /// <param name="times">点赞次数</param>
        public async void SendLike(long userId, int times)
            => await SendDataAsync("send_like", new ParamData
            {
                User_Id = userId.ToString(),
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
                Group_Id = groupId.ToString(),
                User_Id = userId.ToString(),
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
                Group_Id = groupId.ToString(),
                User_Id = userId.ToString(),
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
                Group_Id = groupId.ToString(),
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
                Group_Id = groupId.ToString(),
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
                Group_Id = groupId.ToString(),
                Group_Name = groupName
            });
    }
}