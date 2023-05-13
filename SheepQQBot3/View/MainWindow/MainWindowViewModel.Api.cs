using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.SDK.Client;
using SheepQQBot3.SDK.Event;
using Yamei.Common;

namespace SheepQQBot3.View;

partial class MainWindowViewModel
{
    private const int MaxLogCount = 1000;
    private const int MaxStoreProcessedMessageCount = 20;

    private readonly object _messageLock = new();
    private DateTime _lastBlockedTime = DateTime.MinValue;

    private Task InitApiAsync()
    {
        CqApi = new CQAPI(PublicVar.BotDb);
        var cqApi = CqApi;
        AddRunLog(new RunLog_SystemInfo("API 开始监听"));
        cqApi.OnOpen += async (o, args) =>
        {
            AddRunLog(new RunLog_SystemInfo("API 连接成功"));
            if (PublicVar.IsDebug)
                await cqApi.SendGroupMessageAsync(15873217, "测试Bot启动完成!").ConfigureAwait(false);

            SetConfigs.Values.ForEach(RunAction);

            // MEMO : 处理历史消息记录
            async void RunAction(SetConfig config)
            {
                if (config.TargetType != BotConfigTargetType.Group)
                    return;

                var historyMessages = await cqApi.GetHistoryGroupMessagesAsync(config.TargetId).ConfigureAwait(false);
                if (historyMessages == null)
                    return;

                var processedMessageIds = config.ProcessedMessageIds;
                historyMessages.Where(historyMessage => historyMessage.Sender.UserId != PublicVar.BotId && historyMessage.SubType == SubType.Normal && !processedMessageIds.Contains(historyMessage.MessageId))
                    .ForEach(historyMessage =>
                    {
                        lock (_messageLock)
                        {
                            OnGroupMessage(new GroupMessage(historyMessage));
                        }
                    });
            }
        };
        cqApi.OnClose += (o, data) =>
        {
            AddRunLog(new RunLog_SystemWarning("API 连接断开!!"));
        };
        cqApi.OnGetGroupMessage += (o, groupMessage) =>
        {
            YameiLogExtensions.WriteLog(
                LogType.Quest,
                $"不应该发生的分支-{groupMessage.Message}");
        };
        cqApi.OnSendMessageError += (o, clientReceiveData) =>
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

        cqApi.Start();
        return Task.CompletedTask;
    }

    private void InitEvent()
    {
        CqEvent = new CQEvent();
        var cqEvent = CqEvent;
        AddRunLog(new RunLog_SystemInfo("EVENT 开始监听"));
        cqEvent.OnOpen += (o, args) =>
        {
            AddRunLog(new RunLog_SystemInfo("EVENT 连接成功"));
        };
        cqEvent.OnClose += (o, data) =>
        {
            AddRunLog(new RunLog_SystemWarning("EVENT 连接断开!!"));
        };
        cqEvent.OnGroupPoke += (o, groupPoke) =>
        {
            AddRunLog(new RunLog_GroupPoke(groupPoke));
        };
        cqEvent.OnGroupRevoke += (o, groupRevokeMessage) =>
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
        cqEvent.OnGroupMessage += (o, message) => OnGroupMessage(message);
        cqEvent.OnPrivateMessage += (o, message) => OnPrivateMessage(message);
        cqEvent.Start();
    }

    private void OnPrivateMessage(PrivateMessage privateMessage)
    {
        var userId = privateMessage.UserId;
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

        Task.WaitAll(taskList.ToArray());
    }

    private void OnGroupMessage(GroupMessage groupMessage)
    {
        var groupId = groupMessage.GroupId;
        var setConfig = SetConfigs.Values.FirstOrDefault(each => each.TargetId == groupId);
        if (setConfig == null)
            return;

        // MEMO : 保存已处理的MessageId
        setConfig.ProcessedMessageIds = setConfig.ProcessedMessageIds
            .CopyAddLimit(groupMessage.MessageId, MaxStoreProcessedMessageCount);
        ConfigExtensions.SaveConfig();
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

        GetSelectedGroupConfig(groupId, BotFunctionType.Group_RepeaterKiller, config =>
        {
            StartTaskList(taskList, () => ProcessGroupMessage.RepeaterKiller(groupMessage));
        });

        GetSelectedGroupConfig(groupId, BotFunctionType.Group_GenshinHelper, config =>
        {
            StartTaskList(taskList, GenshinHelper);

            void GenshinHelper() => ProcessGroupMessage.GenshinHelperAsync(
                config.GenshinHelperConfig?.GenshinResinAlarms.Values
                    .ToDictionary(each => each.TargetId, each => each),
                groupMessage);
        });

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

        Task.WaitAll(taskList.ToArray());
        //GetSelectedConfigs(BotFunctionType.Group_RepeatRevokeMessage, groupId)
        //    .ForEach(each => StartTask(() => ProcessGroupMessage.CustomGroupAlarm(each.CustomGroupAlarms, groupMessage)));

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