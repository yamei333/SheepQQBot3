using CommonLibrary;
using Masuit.Tools;
using Microsoft.EntityFrameworkCore;
using SheepQQBot3.DbModel;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.Model;
using SheepQQBot3.Model.QQ;
using SheepQQBot3.SDK.Client.Utils;
using Yamei.Common;

namespace SheepQQBot3.SDK.Client
{
    partial class BotClient
    {
        /// <summary>
        /// 发送群消息
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="message">消息内容</param>
        /// <param name="setConfigs">已设定的消息内容, 用于消息重定义</param>
        public async Task SendGroupMessageAsync(
            string groupId,
            string message,
            Dictionary<Guid, SetConfig> setConfigs = null)
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
            ProcessYmMessage(ElementType.ym_setu_rank, ProcessYmSetuRank);

            if (messageList?.Any() != true)
                return;

            await SendAsync("send_group_msg", new ParamData
            {
                GroupId = groupId,
                Message = messageList,
            }).ConfigureAwait(false);
            return;

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
                if (!elementData.Data.IsNullOrEmpty())
                {
                    await PushExtensions.PushBarkMessageAsync(elementData.Title, elementData.Content).ConfigureAwait(false);
                }
            }

            async void ProcessYmSetuRank(Element ymElement)
            {
                var groupMembers = await GetGroupMembersAsync(groupId).ConfigureAwait(false);
                if (groupMembers == null)
                    return;

                var dateNow = DateTime.Now;
                var dateNowStart = dateNow.ToString("yyyy-MM-dd 00:00:00").ToDateTime();
                var dateNowStartTimestamp = dateNowStart.ToTimeStamp();
                var dateNowEndTimestamp = dateNowStart.AddDays(1).ToTimeStamp();

                await using var botDb = new BotDbContext(new DbContextOptions<BotDbContext>());
                var setuSendHistories = botDb.SetuSendHistorys;
                if (setuSendHistories == null || !setuSendHistories.Any())
                    return;

                var countInfos = botDb.SetuSendHistorys
                    .Where(history => history.TimeStamp >= dateNowStartTimestamp && history.TimeStamp < dateNowEndTimestamp)
                    .GroupBy(history => history.TargetId,
                        (key, group) => new { TargetId = key.ToString(), Items = group.ToList() })
                    .AsEnumerable()
                    .Select(gp =>
                    {
                        var items = gp.Items;
                        var itemCount = items.Count;
                        var isRequestSuccessedCount = items.Count(each => each.IsRequestSuccessed.ToBool());
                        var isRequestSuccessedPercent = isRequestSuccessedCount * 1.0 / itemCount;
                        var isGetSuccessedCount = items.Count(each => each.IsGetSuccessed.ToBool());
                        var isGetSuccessedPercent = isGetSuccessedCount * 1.0 / itemCount;

                        return new
                        {
                            gp.TargetId,
                            Count = itemCount,
                            RequestSuccessedCount = isRequestSuccessedCount,
                            RequestSuccessedPercent = isRequestSuccessedPercent,
                            GetSuccessedCount = isGetSuccessedCount,
                            GetSuccessedPercent = isGetSuccessedPercent,
                            SearchTagCount = items.Count(each => each.IsSearchTag),
                            FreeCount = items.Count(each => each.IsFree.ToBool()),
                            R18BonusCount = items.Count(each => each.IsR18Bonus.ToBool()),
                        };
                    })
                    .ToArray();

                var sendMessage = "=====今日色图大哥=====";
                if (countInfos.Length == 0)
                {
                    sendMessage += "\r\n今日竟无一人色图! 这个世界怎么了!";
                }
                else
                {
                    var kingSuccessed = Enumerable.MaxBy(
                        countInfos.Where(each => each.GetSuccessedCount >= 3),
                        each => each.GetSuccessedPercent);
                    if (kingSuccessed != null)
                        sendMessage += $"\r\n[色图王者]{GetSetuSenderName(kingSuccessed.TargetId)} 成功{kingSuccessed.GetSuccessedCount}次, 成功率 {kingSuccessed.GetSuccessedPercent:#0%}";
                    else
                        sendMessage += $"\r\n[色图王者]无人上榜! 堂堂大群竟无王者";

                    var kingCount = countInfos
                        .Where(each => each.Count >= 5)
                        .OrderBy(each => each.GetSuccessedPercent, true)
                        .FirstOrDefault();
                    if (kingCount != null)
                        sendMessage += $"\r\n[狂怒斗士]{GetSetuSenderName(kingCount.TargetId)} 请求{kingCount.Count}次, 成功率 {kingCount.GetSuccessedPercent:#0%}";
                    else
                        sendMessage += $"\r\n[狂怒斗士]无人上榜! 无人狂怒, 甚好";

                    var kingLucky = countInfos
                        .OrderByDescending(each => each.FreeCount)
                        .First();
                    if (kingLucky.FreeCount > 0)
                        sendMessage += $"\r\n[白嫖斗士]{GetSetuSenderName(kingLucky.TargetId)} 白嫖{kingLucky.FreeCount}次";
                    else
                        sendMessage += $"\r\n[白嫖斗士]无人上榜! 都这么脸黑吗!";

                    var kingGold = countInfos
                        .OrderByDescending(each => each.R18BonusCount)
                        .First();
                    if (kingGold.R18BonusCount > 0)
                        sendMessage += $"\r\n[金色斗士]{GetSetuSenderName(kingGold.TargetId)} 金色传说{kingGold.R18BonusCount}次";
                    else
                        sendMessage += $"\r\n[金色斗士]无人上榜! 并没有天选之人";
                }

                await SendGroupMessageAsync(groupId, sendMessage).ConfigureAwait(false);
                return;

                string GetSetuSenderName(string userId)
                {
                    if (groupMembers.TryGetValue(userId, out var groupMember))
                    {
                        return groupMember.Card.IsNullOrEmpty()
                            ? $"{groupMember.NickName}({userId})"
                            : $"{groupMember.Card}({userId})";
                    }
                    else
                    {
                        return $"无名色图斗士({userId})";
                    }
                }
            }
        }

        /// <summary>
        /// 发送私聊消息
        /// </summary>
        /// <param name="userId">群号</param>
        /// <param name="message">消息内容</param>
        public Task SendPrivateMessageAsync(string userId, string message)
            => SendPrivateMessageAsync(userId, null, message);

        /// <summary>
        /// 发送私聊消息
        /// </summary>
        /// <param name="userId">群号</param>
        /// <param name="groupId">临时消息的群号</param>
        /// <param name="message">消息内容</param>
        public Task SendPrivateMessageAsync(string userId, string groupId, string message)
            => SendAsync("send_private_msg", new ParamData
            {
                UserId = userId,
                GroupId = groupId,
                Message = MessageUtil.ProcessCQMessage(message),
            });

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="type"><see cref="ElementType"/></param>
        /// <param name="targetId">群号</param>
        /// <param name="message">消息内容</param>
        public async Task SendMessageAsync(MessageTargetType type, string targetId, string message)
        {
            switch (type)
            {
                case MessageTargetType.Private:
                    await Task.Run(() => SendPrivateMessageAsync(targetId, null, message)).ConfigureAwait(false);
                    break;
                case MessageTargetType.Group:
                    await Task.Run(() => SendGroupMessageAsync(targetId, message)).ConfigureAwait(false);
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
        /// <param name="title">标题</param>
        /// <param name="content">内容</param>
        /// <param name="summary">底部注释</param>
        /// <param name="prompt">左侧外显</param>
        /// <param name="callBack">回调</param>
        public Task<bool> SendGroupForwardMessageAsync(
            string groupId,
            IEnumerable<GroupForwardMessage> messages,
            string title = null,
            string[] content = null,
            string summary = null,
            string prompt = null,
            Action<ClientReceiveData> callBack = null)
        {
            return SendAsync("send_group_forward_msg", new GroupForwardMessageParamData
            {
                GroupId = groupId,
                Messages = messages
                    .Select(each => new GroupForwardMessageElement(each))
                    .ToList(),
                Contents = content?.Select(each => new GroupForwardMessageNew(each)).ToList(),
                Summary = summary,
                Prompt = prompt,
                Title = title,
            }, jsonText =>
            {
                callBack?.Invoke(jsonText.FromJson<ClientReceiveData>());
                return true;
            });
        }

        /// <summary>
        /// 撤回消息
        /// </summary>
        /// <param name="messageId">消息ID</param>
        public Task DeleteMessageAsync(string messageId)
            => SendAsync("delete_msg", new ParamData
            {
                MessageId = messageId,
            });

        /// <summary>
        /// 发送好友赞
        /// </summary>
        /// <param name="userId">对象QQ</param>
        /// <param name="times">点赞次数</param>
        public Task<bool> SendLikeAsync(string userId, int times)
            => SendAsync("send_like", new ParamData
            {
                UserId = userId,
                Times = times.ToString(),
            });

        /// <summary>
        /// 发送表情回应
        /// </summary>
        /// <param name="messageId">消息ID</param>
        /// <param name="emoji"><see cref="Emoji"/>></param>
        public Task<bool> SendMessageEmojiAsync(string messageId, Emoji emoji)
        {
            return SendAsync("set_msg_emoji_like", new ParamData
            {
                MessageId = messageId,
                EmojiId = ((int)emoji).ToString(),
            });
        }

        /// <summary>
        /// 取得图片信息
        /// </summary>
        /// <param name="fileId">文件名</param>
        public Task<ImageReceiveData> GetImageAsync(string fileId)
        {
            return SendAsync("get_image", new ParamData
            {
                FileId = fileId,
            }, jsonText => jsonText.FromJson<ImageReceiveData>());
        }
    }
}