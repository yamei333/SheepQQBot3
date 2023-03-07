using System;
using System.Collections.Generic;
using System.Linq;
using CommonLibrary;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.SDK.Client;
using SheepQQBot3.SDK.Event;

namespace SheepQQBot3.View
{
    partial class MainWindowViewModel
    {
        private const int MaxLogCount = 200;

        private static readonly Dictionary<int, Action<GroupMessage>> GetMessageCallBacks = new();

        private DateTime _lastBlockedTime = DateTime.MinValue;

        private void InitApi()
        {
            CqApi = new CQAPI();
            var cqApi = CqApi;
            AddRunLog(new RunLog_SystemInfo("API 开始监听"));
            cqApi.OnOpen += (o, args) =>
            {
                AddRunLog(new RunLog_SystemInfo("API 连接成功"));
            };
            cqApi.OnClose += (o, data) =>
            {
                AddRunLog(new RunLog_SystemWarning("API 连接断开!!"));
            };
            cqApi.OnGetGroupMessage += (o, groupMessage) =>
            {
                var messageId = groupMessage.MessageId;
                if (!GetMessageCallBacks.TryGetValue(messageId, out var processAction))
                    return;

                processAction(groupMessage);
                GetMessageCallBacks.Remove(messageId);
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
                    YameiLogExtensions.WriteLog(LogType.Quest, $"发送消息失败, 未知错误 {clientReceiveData}");
                }
            };

            cqApi.Start();
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
                var messageId = groupRevokeMessage.MessageId;
                var targetId = groupRevokeMessage.UserId;
                if (groupRevokeMessage.OperatorId == targetId)
                {
                    GetSelectedConfig(groupId, BotFunctionType.Group_RepeatRevokeMessage, config =>
                    {
                        if (targetId == PublicVar.ADMIN_ID)
                        {
                            // MEMO : ADMIN不复读撤回消息
                            return;
                        }

                        GetMessageCallBacks.Add(messageId, RepeatRevokeMessage);
                        CqApi.GetMessage(groupRevokeMessage.MessageId);

                        async void RepeatRevokeMessage(GroupMessage groupMessage)
                            => await ProcessRevokeGroupMessage.RepeatRevokeMessage(groupMessage);
                    });
                }
            };
            cqEvent.OnGroupMessage += (o, groupMessage) =>
            {
                var groupId = groupMessage.GroupId;
                if (SetConfigs.Values.All(each => each.TargetId != groupId))
                    return;

                var isBlackList = false;
                if (!GetSelectedConfig(
                    groupId,
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

                GetSelectedConfig(groupId, BotFunctionType.Group_CustomGroupAlarm, config =>
                {
                    StartTask(() => ProcessGroupMessage.CustomGroupAlarm(config.CustomGroupAlarms, groupMessage));
                });

                GetSelectedConfig(groupId, BotFunctionType.Common_AlarmAideSubmit, config =>
                {
                    StartTask(AlarmAideSubmit);
                    async void AlarmAideSubmit() => await ProcessGroupMessage.AlarmAideSubmit(config.AlarmAideConfigs, config.AlarmAideSubmitMemberIds, groupMessage);
                });

                GetSelectedConfig(groupId, BotFunctionType.Group_FundHelper, config =>
                {
                    StartTask(() => ProcessGroupMessage.FundHelper(groupMessage));
                });

                GetSelectedConfig(groupId, BotFunctionType.Group_RandomSetu, config =>
                {
                    StartTask(RandomSetu);
                    async void RandomSetu() => await ProcessGroupMessage.RandomSetu(config, groupMessage).ConfigureAwait(false);
                });

                GetSelectedConfig(groupId, BotFunctionType.Group_RepeaterKiller, config =>
                {
                    StartTask(() => ProcessGroupMessage.RepeaterKiller(groupMessage));
                });

                GetSelectedConfig(groupId, BotFunctionType.Group_GenshinHelper, config =>
                {
                    StartTask(GenshinHelper);
                    async void GenshinHelper() => await ProcessGroupMessage.GenshinHelper(
                        config.GenshinHelperConfig.GenshinResinAlarms.Values
                            .ToDictionary(each => each.TargetId, each => each),
                        groupMessage);
                });

                //GetSelectedConfigs(BotFunctionType.Group_RepeatRevokeMessage, groupId)
                //    .ForEach(each => StartTask(() => ProcessGroupMessage.CustomGroupAlarm(each.CustomGroupAlarms, groupMessage)));

                //var setConfig = SetConfigs.FirstOrDefault(each => each.Value.TargetId == groupMessage.GroupId).Value;
                //if (setConfig == null)
                //    return;

                //if (groupMessage.UserId == 252961222)
                //    _cqApi.SendMessage(LogMessageType.Group, groupMessage.GroupId, "你刚发了条消息");
            };
            cqEvent.Start();

            bool GetSelectedConfig(
                long groupId,
                BotFunctionType botFunctionType,
                Action<SetConfig> runAction = null)
            {
                var setConfig = SetConfigs.Values.FirstOrDefault(each =>
                {
                    var botFunction =
                        each.BotFunctions.FirstOrDefault(botFunc => botFunc.BotFunctionType == botFunctionType);
                    return botFunction?.IsUsed == true && each.TargetId == groupId;
                });
                if (setConfig == null)
                    return false;

                runAction?.Invoke(setConfig);
                return true;
            }
        }

        /// <summary>
        /// 增加日志
        /// </summary>
        /// <param name="runLog"></param>
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
}