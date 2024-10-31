using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.DbModel;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.QQ;
using SheepQQBot3.SDK.Server.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yamei.Common;
using static System.Threading.Tasks.Task;

// ReSharper disable AsyncApostle.AsyncAwaitMayBeElidedHighlighting

namespace SheepQQBot3.SDK.Server;

/// <summary>
/// BotServer 服务端SDK
/// </summary>
partial class BotServer
{
    private static BotDbContext _botDb;

    private readonly ConcurrentDictionary<Guid, string> _interaciveJsons = new();

    /// <summary>
    /// 发送群消息
    /// </summary>
    /// <param name="groupId">群号</param>
    /// <param name="message">消息内容</param>
    /// <param name="setConfigs">已设定的消息内容, 用于消息重定义</param>
    public async Task SendGroupMessageAsync(
        long groupId,
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

        var echo = Guid.NewGuid();
        await SendDataAsync("send_group_msg", new ParamData
        {
            GroupId = groupId.ToString(),
            Message = messageList,
        }, echo).ConfigureAwait(false);

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

        async void ProcessYmSetuRank(Element ymElement)
        {
            var groupMembers = await GetGroupMembersAsync(groupId).ConfigureAwait(false);
            if (groupMembers == null)
                return;

            var dateNow = DateTime.Now;
            var dateNowStart = dateNow.ToString("yyyy-MM-dd 00:00:00").ToDateTime();
            var dateNowStartTimestamp = dateNowStart.ToTimeStamp();
            var dateNowEndTimestamp = dateNowStart.AddDays(1).ToTimeStamp();
            var setuSendHistorys = _botDb.SetuSendHistorys;
            if (setuSendHistorys == null || !setuSendHistorys.Any())
                return;

            var countInfos = _botDb.SetuSendHistorys
                .Where(history => history.TimeStamp >= dateNowStartTimestamp && history.TimeStamp < dateNowEndTimestamp)
                .GroupBy(history => history.TargetId,
                    (key, group) => new { TargetId = key, Items = group.ToList() })
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
                });

            var sendMessage = "=====今日色图大哥=====";
            if (!countInfos.Any())
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

                //var kingSearch = countInfos
                //    .OrderByDescending(each => each.SearchTagCount)
                //    .First();
                //if (kingSearch.SearchTagCount > 0)
                //    sendMessage += $"\r\n[狙击斗士]{GetSetuSenderName(kingSearch.TargetId)} 搜索次数 {kingSearch.SearchTagCount}";
                //else
                //    sendMessage += $"\r\n[狙击斗士]无人上榜! 竟无人搜索?";

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

            string GetSetuSenderName(long userId)
            {
                if (groupMembers.TryGetValue(userId, out var groupMember))
                {
                    return string.IsNullOrEmpty(groupMember.Card)
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
    public Task SendPrivateMessageAsync(long userId, string message)
        => SendPrivateMessageAsync(userId, null, message);

    /// <summary>
    /// 发送私聊消息
    /// </summary>
    /// <param name="userId">群号</param>
    /// <param name="groupId">临时消息的群号</param>
    /// <param name="message">消息内容</param>
    public Task SendPrivateMessageAsync(long userId, long? groupId, string message)
        => SendDataAsync("send_private_msg", new ParamData
        {
            UserId = userId.ToString(),
            GroupId = groupId.HasValue ? groupId.ToString() : null,
            Message = MessageUtil.ProcessCQMessage(message),
        });

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="type"><see cref="ElementType"/></param>
    /// <param name="targetId">群号</param>
    /// <param name="message">消息内容</param>
    public async Task SendMessageAsync(MessageTargetType type, long targetId, string message)
    {
        switch (type)
        {
            case MessageTargetType.Private:
                await Run(() => SendPrivateMessageAsync(targetId, null, message)).ConfigureAwait(false);
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
                .ToList(),
        }, echo).ConfigureAwait(false);
        callBack?.Invoke(GetReply(echo, JsonExtensions.Deserialize<ClientReceiveData>, timeout));
    }

    /// <summary>
    /// 撤回消息
    /// </summary>
    /// <param name="messageId">消息ID</param>
    public Task DeleteMessageAsync(int messageId)
        => SendDataAsync("delete_msg", new ParamData
        {
            MessageId = messageId.ToString(),
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
            MessageId = messageId.ToString(),
        }, echo).ConfigureAwait(false);

        var groupMessage = GetReply(echo, jsonInfo =>
        {
            var clientReceiveData = JsonExtensions.Deserialize<ClientReceiveData>(jsonInfo);
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
            GroupId = groupId.ToString(),
        }, echo).ConfigureAwait(false);

        return GetReply(echo, jsonInfo =>
        {
            var clientReceiveData = JsonExtensions.Deserialize<ClientReceiveData_HistoryMessages>(jsonInfo);
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
            Times = times.ToString(),
        }).ConfigureAwait(false);

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
            Reject_Add_Request = isReject.ToString(),
        }).ConfigureAwait(false);

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
            Duration = duration.ToString(),
        }).ConfigureAwait(false);

    /// <summary>
    /// 群全体禁言
    /// </summary>
    /// <param name="groupId">群号</param>
    /// <param name="enable">是否禁言</param>
    public async void SetGroupAllBan(long groupId, bool enable)
        => await SendDataAsync("set_group_whole_ban", new ParamData
        {
            GroupId = groupId.ToString(),
            Enable = enable.ToString(),
        }).ConfigureAwait(false);

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
            Card = card,
        }).ConfigureAwait(false);

    /// <summary>
    /// 设置群名称
    /// </summary>
    /// <param name="groupId">群号</param>
    /// <param name="groupName">群名称</param>
    public async void SetGroupName(long groupId, string groupName)
        => await SendDataAsync("set_group_name", new ParamData
        {
            GroupId = groupId.ToString(),
            GroupName = groupName,
        }).ConfigureAwait(false);

    /// <summary>
    /// 获得群成员名单
    /// </summary>
    /// <param name="groupId">群号</param>
    /// <param name="timeout">超时时间</param>
    public async Task<Dictionary<long, GroupMember>> GetGroupMembersAsync(long groupId, double timeout = 5)
    {
        var echo = Guid.NewGuid();
        await SendDataAsync("get_group_member_list", new ParamData
        {
            GroupId = groupId.ToString(),
            NoCache = false,
        }, echo).ConfigureAwait(false);

        return GetReply(echo, jsonText =>
        {
            var clientData = JsonExtensions.Deserialize<ClientReceiveData_GroupMember>(jsonText);
            return clientData.Data.ToDictionary(each => each.UserId, each => each);
        }, timeout);
    }

    /// <summary>
    /// 发送表情回应
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="emoji"><see cref="Emoji"/>></param>
    public async void SendMessageEmojiAsync(long messageId, Emoji emoji)
    {
        var echo = Guid.NewGuid();
        await SendDataAsync("set_msg_emoji_like", new ParamData
        {
            MessageId = messageId.ToString(),
            EmojiId = ((int)emoji).ToString(),
        }, echo).ConfigureAwait(false);
    }

    /// <summary>
    /// 获取Cookies
    /// </summary>
    public async Task<string> GetCookiesAsync(string domain, double timeout = 5)
    {
        var echo = Guid.NewGuid();
        await SendDataAsync("get_cookies", new ParamData
        {
            Domain = domain,
        }, echo).ConfigureAwait(false);

        return GetReply(echo, jsonText =>
        {
            return jsonText;
        }, timeout);
    }

    private T GetReply<T>(Guid echo, Func<string, T> getFunc, double timeout)
        where T : class
    {
        SpinWait.SpinUntil(() => _interaciveJsons.ContainsKey(echo), TimeSpan.FromSeconds(timeout));
        if (!_interaciveJsons.TryGetValue(echo, out var jsonText))
            return null;

        _interaciveJsons.Remove(echo, out _);
        return getFunc(jsonText);
    }
}