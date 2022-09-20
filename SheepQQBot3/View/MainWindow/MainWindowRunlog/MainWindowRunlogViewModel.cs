using System;
using System.Collections.Generic;
using System.ComponentModel;
using SheepQQBot3.Model.Config;
using SheepQQBot3.SDK.Client;
using LogMessageType = SheepQQBot3.Model.Enums.LogMessageType;

namespace SheepQQBot3.View
{
    public partial class MainWindowRunlogViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        private List<RunLog> _runLogs;
        private RunLog _selectedRunLog;
        private static MainWindowViewModel _mainVm => PublicVar.Vm;

        /// <summary>
        /// 值变化时调用, 用于通知界面
        /// </summary>
        /// <param name="propertyName">属性名</param>
        public void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Dictionary<Guid, SetConfig> SetConfigs => _mainVm.SetConfigs;
        public CQAPI Api => _mainVm.CqApi;

        /// <summary>
        /// 初始化
        /// </summary>
        public MainWindowRunlogViewModel()
        {
            RunLogMessages = new List<RunLogMessage>();
            RunLogs = new List<RunLog>();
        }

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
                new RunLogMessage($"时间: {runLog.DateTimeStrFFF}"),
                new RunLogMessage(runLog.MessageColor, $"类型: {runLog.MessageTypeStr}")
            };
            switch (runLog.MessageType)
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
                case LogMessageType.GroupMessage:
                    result.Add(new RunLogMessage($"来源-群号: {runLog.GroupId}"));
                    result.Add(new RunLogMessage($"　　-QQ号: {runLog.SenderId}"));
                    break;
                case LogMessageType.GroupRevokeMessage:
                    result.Add(new RunLogMessage($"来源-群号: {runLog.GroupId}"));
                    result.Add(new RunLogMessage($"　　-操作者: {runLog.OperatorId}"));
                    result.Add(new RunLogMessage($"　　-QQ号: {runLog.SenderId}"));
                    break;
                case LogMessageType.GroupPoke:
                    result.Add(new RunLogMessage($"来源-群号: {runLog.GroupId}"));
                    result.Add(new RunLogMessage($"　　-操作者: {runLog.SenderId}"));
                    result.Add(new RunLogMessage($"　　-目标: {runLog.TargetId}"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            result.Add(new RunLogMessage($"内容: {runLog.Content}"));

            return result;
        }
    }
}