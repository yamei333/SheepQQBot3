using Masuit.Tools.Systems;
using SheepQQBot3.Model.Config;
using System;
using System.Collections.Generic;
using LogMessageType = SheepQQBot3.Model.Enums.LogMessageType;

namespace SheepQQBot3.View;

public partial class MainWindowRunlogViewModel : MainWindowViewModelBase
{
    /// <summary>
    /// 初始化
    /// </summary>
    public MainWindowRunlogViewModel()
    {
        RunLogMessages = new List<RunLogMessage>();
        RunLogs = new List<RunLog>();
    }

    private List<RunLog> _runLogs;
    public List<RunLog> RunLogs
    {
        get => _runLogs;
        set
        {
            if (_runLogs == value)
                return;

            _runLogs = value;
            OnPropertyChanged(nameof(RunLogs));
        }
    }

    public IEnumerable<RunLogMessage> RunLogMessages { get; set; }

    private RunLog _selectedRunLog;
    public RunLog SelectedRunLog
    {
        get => _selectedRunLog;
        set
        {
            if (_selectedRunLog == value)
                return;

            _selectedRunLog = value;
            OnPropertyChanged(nameof(SelectedRunLog));
            if (_selectedRunLog != null)
            {
                RunLogMessages = ProcessRunLog2LogMessage(_selectedRunLog);
                OnPropertyChanged(nameof(RunLogMessages));
            }
        }
    }

    private static IEnumerable<RunLogMessage> ProcessRunLog2LogMessage(RunLog runLog)
    {
        var result = new List<RunLogMessage>
        {
            new($"时间: {runLog.DateTimeStrFFF}"),
            new($"类型: {runLog.LogMessageType.GetDisplay()}"),
        };
        switch (runLog.LogMessageType)
        {
            case LogMessageType.System_Info:
            case LogMessageType.System_Error:
            case LogMessageType.System_Warning:
                result.Add(new RunLogMessage($"来源: {runLog.SenderId}"));
                break;
            case LogMessageType.MetaData:
                result.Add(new RunLogMessage($"来源: {runLog.SenderId}"));
                break;
            case LogMessageType.AlarmAide:
            case LogMessageType.FundHelper:
                result.Add(string.IsNullOrEmpty(runLog.GroupId)
                    ? new RunLogMessage($"QQ号: {runLog.SenderId}")
                    : new RunLogMessage($"群号: {runLog.GroupId}"));
                break;
            case LogMessageType.LiveAlarm:
                result.Add(new RunLogMessage($"群号: {runLog.SenderId}"));
                result.Add(new RunLogMessage($"直播间号: {runLog.GroupId}"));
                break;
            case LogMessageType.GroupMessage:
                result.Add(new RunLogMessage($"来源-群号: {runLog.GroupId}"));
                result.Add(new RunLogMessage($"　　-消息ID: {runLog.MessageId}"));
                result.Add(new RunLogMessage($"　　-QQ号: {runLog.SenderId}{(runLog.IsBlackList ? "(黑名单)" : string.Empty)}"));
                break;
            case LogMessageType.GroupRevokeMessage:
                result.Add(new RunLogMessage($"来源-群号: {runLog.GroupId}"));
                result.Add(new RunLogMessage($"　　-消息ID: {runLog.MessageId}"));
                result.Add(new RunLogMessage($"　　-操作者: {runLog.OperatorId}"));
                result.Add(new RunLogMessage($"　　-QQ号: {runLog.SenderId}{(runLog.IsBlackList ? "(黑名单)" : string.Empty)}"));
                break;
            case LogMessageType.GroupPoke:
                result.Add(new RunLogMessage($"来源-群号: {runLog.GroupId}"));
                result.Add(new RunLogMessage($"　　-操作者: {runLog.SenderId}"));
                result.Add(new RunLogMessage($"　　-目标: {runLog.TargetId}"));
                break;
            case LogMessageType.BlockedByServer:
                result.Add(new RunLogMessage("来源: QQBotService"));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        result.Add(new RunLogMessage($"内容: {runLog.Content}"));
        return result;
    }
}