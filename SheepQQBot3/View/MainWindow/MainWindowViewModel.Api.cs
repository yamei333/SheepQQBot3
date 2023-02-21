using System;
using System.Collections.Generic;
using System.Linq;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.SDK.Client;
using SheepQQBot3.SDK.Event;
using Yamei.Common;

namespace SheepQQBot3.View
{
    partial class MainWindowViewModel
    {
        private const int MaxLogCount = 200;

        private static Dictionary<int, Action<GroupMessage>> GetMessageCallBacks = new Dictionary<int, Action<GroupMessage>>();

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
                if (GetMessageCallBacks.TryGetValue(messageId, out var processAction))
                {
                    processAction(groupMessage);
                    GetMessageCallBacks.Remove(messageId);
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
                if (GetSelectedConfigs(BotFunctionType.Group_RepeatRevokeMessage, groupId).Any()
                    && groupRevokeMessage.OperatorId == groupRevokeMessage.UserId)
                {
                    GetMessageCallBacks.Add(messageId, groupMessage => ProcessRevokeGroupMessage.RepeatRevokeMessage(groupMessage));
                    CqApi.GetMessage(groupRevokeMessage.MessageId);
                }
            };
            cqEvent.OnGroupMessage += (o, groupMessage) =>
            {
                var groupId = groupMessage.GroupId;
                if (SetConfigs.Values.All(each => each.TargetId != groupId))
                    return;

                AddRunLog(new RunLog_GroupMessage(groupMessage));

                GetSelectedConfigs(BotFunctionType.Group_CustomGroupAlarm, groupId)
                    .ForEach(each => StartTask(() => ProcessGroupMessage.CustomGroupAlarm(each.CustomGroupAlarms, groupMessage)));
                GetSelectedConfigs(BotFunctionType.Common_AlarmAideSubmit, groupId)
                    .ForEach(each =>
                    {
                        StartTask(AlarmAideSubmit);
                        async void AlarmAideSubmit() => await ProcessGroupMessage.AlarmAideSubmit(each.AlarmAideConfigs, each.AlarmAideSubmitMemberIds, groupMessage);
                    });
                GetSelectedConfigs(BotFunctionType.Group_FundHelper, groupId)
                    .ForEach(each => StartTask(() => ProcessGroupMessage.FundHelper(groupMessage)));
                GetSelectedConfigs(BotFunctionType.Group_RandomSetu, groupId)
                    .ForEach(each =>
                    {
                        StartTask(RandomSetu);
                        async void RandomSetu() => await ProcessGroupMessage.RandomSetu(groupMessage).ConfigureAwait(false);
                    });
                GetSelectedConfigs(BotFunctionType.Group_RepeaterKiller, groupId)
                    .ForEach(each => StartTask(() => ProcessGroupMessage.RepeaterKiller(groupMessage)));

                //GetSelectedConfigs(BotFunctionType.Group_RepeatRevokeMessage, groupId)
                //    .ForEach(each => StartTask(() => ProcessGroupMessage.CustomGroupAlarm(each.CustomGroupAlarms, groupMessage)));

                //var setConfig = SetConfigs.FirstOrDefault(each => each.Value.TargetId == groupMessage.GroupId).Value;
                //if (setConfig == null)
                //    return;

                //if (groupMessage.UserId == 252961222)
                //    _cqApi.SendMessage(MessageType.Group, groupMessage.GroupId, "你刚发了条消息");
            };
            cqEvent.Start();

            IEnumerable<SetConfig> GetSelectedConfigs(BotFunctionType botFunctionType, long groupId)
                => SetConfigs.Values.Where(each =>
                {
                    var botFunction = each.BotFunctions.FirstOrDefault(botFunc => botFunc.BotFunctionType == botFunctionType);
                    return botFunction?.IsUsed == true && each.TargetId == groupId;
                });
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