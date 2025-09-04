using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.BotProcessMessage;
using SheepQQBot3.BotProcessMessage.Group;
using SheepQQBot3.BotProcessMessage.Private;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.SDK.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SheepQQBot3.View;

partial class MainWindowViewModel
{
    private const int MaxLogCount = 1000;
    private const int MaxStoreProcessedMessageCount = 20;

    /// <summary>
    /// 处理历史记录消息用
    /// </summary>
    //private readonly object _messageLock = new();
    private DateTime _lastBlockedTime = DateTime.MinValue;

    private void InitServer()
    {
        BotServer = new BotServer(PublicVar.BotDb);
        var botServer = BotServer;
        AddRunLog(new RunLog_SystemInfo("SERVER 开始监听"));
        botServer.ClientConnected += (o, args) =>
        {
            AddRunLog(new RunLog_SystemInfo("SERVER 连接成功"));
            if (PublicVar.IsDebug)
            {
                botServer.SendGroupMessageAsync(15873217, "测试Bot启动完成!").ConfigureAwait(false);

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
        botServer.ClientDisconnected += (o, data) =>
        {
            AddRunLog(new RunLog_SystemWarning("SERVER 连接断开!!"));
        };
        botServer.OnSendMessageError += (o, clientReceiveData) =>
        {
            var dateNow = DateTime.Now;
            if (clientReceiveData.Wording == "send group message failed: blocked by server")
            {
                if ((dateNow - _lastBlockedTime).TotalMicroseconds > 2000)
                {
                    LogExtensions.AddRunLog(new RunLog_BlockedByServer("账号已被风控!"));
                    _lastBlockedTime = dateNow;
                }
                else
                {
                    // MEMO : 不处理重复发送的风控消息
                }
            }
            else
            {
                YameiLogExtensions.WriteLog(
                    LogType.Quest,
                    $"发送消息失败, 未知错误 {JsonSerializer.Serialize(clientReceiveData)}");
            }
        };

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
                GetSelectedGroupConfig(groupId, BotFunctionType.Group_RepeatRevokeMessage, RunAction);
                void RunAction(SetConfig config) => ProcessRevokeGroupMessage.RepeatRevokeMessageAsync(groupRevokeMessage);
            }
        };
        botServer.OnGroupMessage += (o, message) => OnGroupMessage(message);
        botServer.OnPrivateMessage += (o, message) => OnPrivateMessage(message);
        botServer.Start();
    }

    private void OnPrivateMessage(PrivateMessage privateMessage)
    {
        var userId = privateMessage.UserId;
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

        var taskList = new List<Task>();
        GetSelectedCommonConfig(BotFunctionType.Common_KeyConfig, config =>
        {
            StartTaskList(taskList, KeyConfig);
            void KeyConfig() => ProcessMessage.KeyConfigAsync(privateMessage);
        });
        GetSelectedCommonConfig(BotFunctionType.Common_CustomAlarm, config =>
        {
            StartTaskList(taskList, CustomGroupAlarm);
            void CustomGroupAlarm() => ProcessMessage.CustomPrivateAlarmAsync(privateMessage);
        });

        if (BotExtensions.IsAdmin(userId))
        {
            StartTaskList(taskList, AdminCommand);
            void AdminCommand() => ProcessPrivateMessage.AdminCommandAsync(privateMessage);

            StartTaskList(taskList, ChatSummaryConfig);
            void ChatSummaryConfig() => ProcessPrivateMessage.ChatSummaryConfigAsync(privateMessage);

            GetSelectedCommonConfig(BotFunctionType.Common_AiConfig, config =>
            {
                StartTaskList(taskList, AiAide);
                void AiAide() => ProcessPrivateMessage.AiAideAsync(privateMessage);
            });
        }

        Task.WaitAll(taskList.ToArray());
    }

    private void OnGroupMessage(GroupMessage groupMessage)
    {
        var groupId = groupMessage.GroupId;
        //var messageId = groupMessage.MessageId;
        //var message = groupMessage.Message;

        var setConfig = SetConfigs.Values.FirstOrDefault(each => each.TargetId == groupId);
        if (setConfig == null)
            return;

        // MEMO : 保存已处理的MessageId
        setConfig.ProcessedMessageIds = setConfig.ProcessedMessageIds
            .CopyAddLimit(groupMessage.MessageId, MaxStoreProcessedMessageCount);
        ConfigExtensions.SaveConfig();

        //if (groupMessage.GroupId == PublicVar.TestGroupId)
        //{
        //    var regGetImage = new Regex(@"(?<=\[CQ:image.+url=).+(?=[,\]])");
        //    regGetImage.Matches(groupMessage.Message).ForEach(match =>
        //    {
        //        var nsfwResult = NSFWExtensions.CheckWebImage(match.Value);
        //        CqApi.SendGroupMessageAsync(groupMessage.GroupId,
        //            $"NSFW Result:\r\n" +
        //            $"IsNsfw: {nsfwResult.IsNsfw}\r\n" +
        //            $"Pornography: {nsfwResult.PornographyPercent}\r\n" +
        //            $"Sexy: {nsfwResult.SexyPercent}\r\n" +
        //            $"Hentai: {nsfwResult.HentaiPercent}");
        //    });
        //}

        var isBlackList = false;
        if (!GetSelectedCommonConfig(
            BotFunctionType.Common_BlackList,
            config =>
            {
                isBlackList = config.BlackListIds.Contains(groupMessage.UserId);
                AddRunLog(isBlackList
                    ? new RunLog_GroupMessageBlackList(groupMessage)
                    : new RunLog_GroupMessage(groupMessage));
            }))
        {
            AddRunLog(new RunLog_GroupMessage(groupMessage));
        }

        // MEMO : 黑名单用户不作处理
        if (isBlackList)
            return;

        var taskList = new List<Task>();
        GetSelectedGroupConfig(groupId, BotFunctionType.Common_CustomAlarm, config =>
        {
            StartTaskList(taskList, CustomGroupAlarm);
            void CustomGroupAlarm() => ProcessMessage.CustomGroupAlarmAsync(groupMessage);
        });

        GetSelectedGroupConfig(groupId, BotFunctionType.Common_AlarmAideSubmit, config =>
        {
            StartTaskList(taskList, AlarmAideSubmit);
            void AlarmAideSubmit() => ProcessGroupMessage.AlarmAideSubmit(config.AlarmAideConfigs, config.AlarmAideSubmitMemberIds, groupMessage);
        });

        GetSelectedGroupConfig(groupId, BotFunctionType.Group_FundHelper, config =>
        {
            StartTaskList(taskList, FundHelper);
            void FundHelper() => ProcessGroupMessage.FundHelper(groupMessage);
        });

        GetSelectedGroupConfig(groupId, BotFunctionType.Group_RandomSetu, config =>
        {
            StartTaskList(taskList, RandomSetu);
            async void RandomSetu() => await ProcessGroupMessage.RandomSetuAsync(PublicVar.BotConfig, groupMessage).ConfigureAwait(false);
        });

        //GetSelectedGroupConfig(groupId, BotFunctionType.Group_RepeaterKiller, config =>
        //{
        //    StartTaskList(taskList, () => ProcessGroupMessage.RepeaterKiller(groupMessage));
        //});

        GetSelectedGroupConfig(groupId, BotFunctionType.Group_SearchImageSource, config =>
        {
            StartTaskList(taskList, SearchImageSource);
            async void SearchImageSource() => await ProcessGroupMessage.SearchImageSource(groupMessage).ConfigureAwait(false);
        });

        GetSelectedGroupConfig(groupId, BotFunctionType.Group_Roll, config =>
        {
            StartTaskList(taskList, Roll);
            async void Roll() => await ProcessGroupMessage.RollAsync(groupMessage).ConfigureAwait(false);
        });

        GetSelectedGroupConfig(groupId, BotFunctionType.Group_ChatSummary, config =>
        {
            StartTaskList(taskList, ChatSummary);
            async void ChatSummary() => await ProcessGroupMessage.ChatSummaryAsync(groupMessage).ConfigureAwait(false);
        });

        GetSelectedGroupConfig(groupId, BotFunctionType.Group_RepeatRevokeMessage, config =>
        {
            StartTaskList(taskList, RepeatRevokeMessage);
            async void RepeatRevokeMessage() => await ProcessGroupMessage.RepeatRevokeMessageAsync(groupMessage).ConfigureAwait(false);
        });

        GetSelectedGroupConfig(groupId, BotFunctionType.Group_AiAide, config =>
        {
            //StartTaskList(taskList, AiAide);
            //async void AiAide() => await ProcessGroupMessage.AiAideAsync(groupMessage).ConfigureAwait(false);
            ProcessGroupMessage.AiAideAsync(groupMessage).ConfigureAwait(false);
        });

        Task.WaitAll(taskList.ToArray());
        //var setConfig = SetConfigs.FirstOrDefault(each => each.Value.TargetId == groupMessage.GroupId).Value;
        //if (setConfig == null)
        //    return;

        //if (groupMessage.UserId == 252961222)
        //    _cqApi.SendMessage(LogMessageType.Group, groupMessage.GroupId, "你刚发了条消息");
    }

    private bool GetSelectedPrivateConfig(
        long userId,
        BotFunctionType botFunctionType,
        Action<SetConfig> runAction = null)
    {
        var setConfig = SetConfigs.Values
            .Where(each => each.TargetType == BotConfigTargetType.Private)
            .FirstOrDefault(each => each.TargetId == userId
                && each.BotFunctions.FirstOrDefault(botFunc => botFunc.BotFunctionType == botFunctionType)?.IsUsed == true);
        if (setConfig == null)
            return false;

        runAction?.Invoke(setConfig);
        return true;
    }

    private bool GetSelectedCommonConfig(
        BotFunctionType botFunctionType,
        Action<SetConfig> runAction = null)
    {
        var setConfig = SetConfigs.Values
            .Where(each => each.TargetType == BotConfigTargetType.Common)
            .FirstOrDefault(each => each.TargetId == PublicVar.CommonId
                && each.BotFunctions.FirstOrDefault(botFunc => botFunc.BotFunctionType == botFunctionType)?.IsUsed == true);
        if (setConfig == null)
            return false;

        runAction?.Invoke(setConfig);
        return true;
    }

    private bool GetSelectedGroupConfig(
        long groupId,
        BotFunctionType botFunctionType,
        Action<SetConfig> runAction = null)
    {
        var setConfig = SetConfigs.Values
            .Where(each => each.TargetType == BotConfigTargetType.Group)
            .FirstOrDefault(each => each.TargetId == groupId
                && each.BotFunctions.FirstOrDefault(botFunc => botFunc.BotFunctionType == botFunctionType)?.IsUsed == true);
        if (setConfig == null)
            return false;

        runAction?.Invoke(setConfig);
        return true;
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