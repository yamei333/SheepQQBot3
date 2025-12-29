using SheepQQBot3.BotProcessMessage;
using SheepQQBot3.BotProcessMessage.Group;
using SheepQQBot3.BotProcessMessage.Private;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.SDK.Client;
using SheepQQBot3.SDK.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.View;

partial class MainWindowViewModel
{
    private const int MaxLogCount = 1000;
    //private const int MaxStoreProcessedMessageCount = 20;

    /// <summary>
    /// 处理历史记录消息用
    /// </summary>
    //private readonly object _messageLock = new();
    //private DateTime _lastBlockedTime = DateTime.MinValue;

    // 存储每个用户的锁，Key 是用户 QQ 号
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _userLocks = [];

    private static readonly ConcurrentDictionary<string, Lazy<Task<Dictionary<string, GroupMember>>>> _globalGroupMembers = [];

    private void InitServer()
    {
        BotServer = new BotServer();
        BotClient = new BotClient();

        var botServer = BotServer;
        AddRunLog(new RunLog_SystemInfo("SERVER 开始监听"));
        botServer.ClientConnected += (o, args) =>
        {
            AddRunLog(new RunLog_SystemInfo("SERVER 连接成功"));
            if (IsDebug)
            {
                BotClient.SendGroupMessageAsync("15873217", "测试Bot启动完成!").ConfigureAwait(false);

                //var message = new List<GroupForwardMessage>
                //{
                //    new GroupForwardMessage("pm", 173629299, "我没有任何牌面!"),
                //    new GroupForwardMessage("zstlpmdm", 173629299, "我太ruaji了!"),
                //};
                //botServer.SendGroupForwardMessageAsync(15873217, message).ConfigureAwait(false);
            }

            #region 处理历史记录

            //SetConfigs.Values.ForEach(RunAction);

            //// MEMO : 处理历史消息记录
            //async void RunAction(SetConfig config)
            //{
            //    if (config.TargetType != BotConfigTargetType.Group)
            //        return;

            //    var historyMessages = await cqApi.GetHistoryGroupMessagesAsync(config.TargetId).ConfigureAwait(false);
            //    if (historyMessages == null)
            //        return;

            //    var processedMessageIds = config.ProcessedMessageIds;
            //    historyMessages.Where(historyMessage => historyMessage.Sender.UserId != PublicVar.BotId && historyMessage.SubType == SubType.Normal && !processedMessageIds.Contains(historyMessage.MessageId))
            //        .ForEach(historyMessage =>
            //        {
            //            lock (_messageLock)
            //            {
            //                OnGroupMessage(new GroupMessage(historyMessage));
            //            }
            //        });
            //}

            #endregion 处理历史记录
        };
        botServer.ClientDisconnected += (_, _) =>
        {
            AddRunLog(new RunLog_SystemWarning("SERVER 连接断开!!"));
        };
        //botServer.OnSendMessageError += (o, clientReceiveData) =>
        //{
        //    var dateNow = DateTime.Now;
        //    if (clientReceiveData.Wording == "send group message failed: blocked by server")
        //    {
        //        if ((dateNow - _lastBlockedTime).TotalMicroseconds > 2000)
        //        {
        //            LogExtensions.AddRunLog(new RunLog_BlockedByServer("账号已被风控!"));
        //            _lastBlockedTime = dateNow;
        //        }
        //        else
        //        {
        //            // MEMO : 不处理重复发送的风控消息
        //        }
        //    }
        //    else
        //    {
        //        YameiLogExtensions.WriteLog(
        //            LogType.Quest,
        //            $"发送消息失败, 未知错误 {JsonSerializer.Serialize(clientReceiveData)}");
        //    }
        //};

        botServer.OnGroupPoke += (o, groupPoke) =>
        {
            AddRunLog(new RunLog_GroupPoke(groupPoke));
        };
        botServer.OnGroupRevoke += (o, groupRevokeMessage) =>
        {
            AddRunLog(new RunLog_GroupRevokeMessage(groupRevokeMessage));

            var groupId = groupRevokeMessage.GroupId;
            var targetId = groupRevokeMessage.UserId;
            if (groupRevokeMessage.OperatorId == targetId)
            {
                var groupConfig = SetConfigs.Values.FirstOrDefault(x => x.TargetType == BotConfigTargetType.Group && x.TargetId == groupId);
                if (groupConfig == null)
                    return;

                if (IsEnabled(BotFunctionType.Group_RepeatRevokeMessage))
                    _ = ProcessRevokeGroupMessage.RepeatRevokeMessageAsync(groupRevokeMessage);

                bool IsEnabled(BotFunctionType type) => groupConfig.BotFunctions.Any(f => f.BotFunctionType == type && f.IsUsed);
            }
        };
        botServer.OnGroupMessage += (o, message) => _ = OnGroupMessageAsync(message);
        botServer.OnPrivateMessage += (o, message) => _ = OnPrivateMessageAsync(message);
        botServer.Start();
    }

    private async Task OnPrivateMessageAsync(PrivateMessage privateMessage)
    {
        var senderId = privateMessage.UserId;
        //if (userId == 252961222)
        //{
        //    var regGetImage = new Regex(@"(?<=\[CQ:image.+url=).+(?=[,\]])");
        //    regGetImage.Matches(privateMessage.Message).ForEach(match =>
        //    {
        //        var nsfwResult = NSFWExtensions.CheckWebImage(match.Value);
        //        CqApi.SendPrivateMessageAsync(userId,
        //            $"NSFW Result:\r\n" +
        //            $"IsNsfw: {nsfwResult.IsNsfw}\r\n" +
        //            $"Pornography: {nsfwResult.PornographyPercent}\r\n" +
        //            $"Sexy: {nsfwResult.SexyPercent}\r\n" +
        //            $"Hentai: {nsfwResult.HentaiPercent}\r\n" +
        //            $"PredictedLabel: {nsfwResult.PredictedLabel}");
        //    });
        //}

        var systemConfig = SetConfigs.Values.FirstOrDefault(x => x.TargetType == BotConfigTargetType.Common && x.TargetId == AISystemId);
        if (systemConfig == null)
            return;

        var blackListUserConfig = systemConfig?.BlackListUserConfigs.GetValueOrDefault(senderId, new BlackListUserConfig(senderId)) ?? new BlackListUserConfig(senderId);
        // 获取或创建一个属于该用户的锁
        var userLock = _userLocks.GetOrAdd(senderId, _ => new SemaphoreSlim(1, 1));
        await userLock.WaitAsync().ConfigureAwait(false);

        var taskList = new List<Task>();
        if (IsEnabled(BotFunctionType.Common_KeyConfig))
            taskList.Add(ProcessMessage.KeyConfigAsync(privateMessage));

        if (IsEnabled(BotFunctionType.Common_CustomAlarm))
            taskList.Add(ProcessMessage.CustomPrivateAlarmAsync(privateMessage));

        if (IsEnabled(BotFunctionType.Common_AIConfig))
            taskList.Add(ProcessPrivateMessage.AIAideAsync(blackListUserConfig, privateMessage));

        if (BotExtensions.IsAdmin(senderId))
            taskList.Add(ProcessPrivateMessage.AdminCommandAsync(privateMessage));

        try
        {
            await Task.WhenAll(taskList).ConfigureAwait(false);
        }
        finally
        {
            userLock.Release();
        }

        return;

        bool IsEnabled(BotFunctionType type) => systemConfig.BotFunctions.Any(f => f.BotFunctionType == type && f.IsUsed);
    }

    private async Task OnGroupMessageAsync(GroupMessage groupMessage)
    {
        var groupId = groupMessage.GroupId;
        var groupConfig = SetConfigs.Values.FirstOrDefault(x => x.TargetType == BotConfigTargetType.Group && x.TargetId == groupId);
        if (groupConfig == null)
            return;

        var senderId = groupMessage.UserId;
        // MEMO : 取得GroupMembers
        Lazy<Task<Dictionary<string, GroupMember>>> lazyWrapper;
        if (IsDebug)
        {
            lazyWrapper = _globalGroupMembers.GetOrAdd("414774779",
                id => new Lazy<Task<Dictionary<string, GroupMember>>>(() => GlobalBotClient.GetGroupMembersAsync(id)));
        }
        else
        {
            lazyWrapper = _globalGroupMembers.GetOrAdd(groupId,
                id => new Lazy<Task<Dictionary<string, GroupMember>>>(() => GlobalBotClient.GetGroupMembersAsync(id)));
        }

        var groupMembers = await lazyWrapper.Value.ConfigureAwait(false);
        if (groupMembers == null)
        {
            await GlobalBotClient.SendGroupMessageAsync(groupId, "群成员信息获取失败!").ConfigureAwait(false);
            return;
        }

        // MEMO : 新进群友忽略 / 机器人消息忽略
        if (!groupMembers.TryGetValue(senderId, out var groupMember) || groupMember.IsRobot)
            return;

        var systemConfig = SetConfigs.Values.FirstOrDefault(x => x.TargetType == BotConfigTargetType.Common && x.TargetId == AISystemId);
        var blackListUserConfig = systemConfig?.BlackListUserConfigs.GetValueOrDefault(senderId, new BlackListUserConfig(senderId)) ?? new BlackListUserConfig(senderId);

        AddRunLog(blackListUserConfig.BanedChatSummaryCollect
            ? new RunLog_GroupMessageBlackList(groupMessage)
            : new RunLog_GroupMessage(groupMessage));

        // 获取或创建一个属于该用户的锁
        var userLock = _userLocks.GetOrAdd(senderId, _ => new SemaphoreSlim(1, 1));
        await userLock.WaitAsync().ConfigureAwait(false);

        var taskList = new List<Task>();
        var actions = new (BotFunctionType Type, Func<Task> Action)[]
        {
            (BotFunctionType.Common_CustomAlarm, () => ProcessMessage.CustomGroupAlarmAsync(groupMessage)),
            (BotFunctionType.Common_AlarmAideSubmit, () => ProcessGroupMessage.AlarmAideSubmitAsync(groupConfig.AlarmAideConfigs, groupConfig.AlarmAideSubmitMemberIds, groupMessage)),
            //(BotFunctionType.Group_FundHelper, () => ProcessGroupMessage.FundHelperAsync(groupMessage)),
            (BotFunctionType.Group_RandomSetu, () => ProcessGroupMessage.RandomSetuAsync(blackListUserConfig, groupMessage)),
            (BotFunctionType.Group_SearchImageSource, () => ProcessGroupMessage.SearchImageSourceAsync(groupMessage)),
            (BotFunctionType.Group_Roll, () => ProcessGroupMessage.RollAsync(groupMessage)),
            (BotFunctionType.Group_ChatSummary, () => ProcessGroupMessage.ChatSummaryAsync(groupConfig.AIGroupConfig, blackListUserConfig, groupMembers, groupMessage)),
            (BotFunctionType.Group_RepeatRevokeMessage, () => ProcessGroupMessage.RepeatRevokeMessageAsync(groupMessage)),
            (BotFunctionType.Group_AIAide, () => ProcessGroupMessage.AIAideAsync(groupConfig.AIGroupConfig, blackListUserConfig, groupMembers, groupMessage)),
        };

        foreach (var (type, action) in actions)
        {
            if (IsEnabled(type))
                taskList.Add(action());
        }

        try
        {
            await Task.WhenAll(taskList).ConfigureAwait(false);
        }
        finally
        {
            userLock.Release();
        }

        return;

        bool IsEnabled(BotFunctionType type) => groupConfig.BotFunctions.Any(f => f.BotFunctionType == type && f.IsUsed);
    }

    /// <summary>
    /// 增加日志
    /// </summary>
    /// <param name="runLog"><see cref="RunLog"/></param>
    public void AddRunLog(RunLog runLog)
    {
        var runLogs = new List<RunLog> { runLog };
        var mainWindowRunlogViewModel = MainWindowRunlogViewModel;
        runLogs.AddRange(mainWindowRunlogViewModel.RunLogs);
        // MEMO : 超过上限时移除最后一条记录
        if (runLogs.Count > MaxLogCount)
            runLogs.RemoveAt(MaxLogCount);

        mainWindowRunlogViewModel.RunLogs = new List<RunLog>(runLogs);
        mainWindowRunlogViewModel.SelectedRunLog = runLog;
    }
}