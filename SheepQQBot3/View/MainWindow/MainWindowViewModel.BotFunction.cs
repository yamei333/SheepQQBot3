using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SheepQQBot3.View;

partial class MainWindowViewModel
{
    /// <summary>
    /// 用于取消任务的<see cref="CancellationTokenSource"/>
    /// </summary>
    public CancellationTokenSource CancelToken { get; set; }

    private void InitBotFunctions()
    {
        var cancelToken = new CancellationTokenSource();
        CancelToken = cancelToken;
        StartTask(TaskProcess.AlarmAides);
        StartTask(TaskProcess.CustomAlarm);
        StartTask(TaskProcess.FundHelper);
        StartTask(TaskProcess.LiveAlarm);
        StartTask(TaskProcess.GenshinResinAlarm);
    }

    /// <summary>
    /// 开始一个Bot的Task
    /// </summary>
    /// <param name="method"></param>
    private void StartTask(Action method)
        => Task.Factory.StartNew(method, CancelToken.Token);

    /// <summary>
    /// 开始一个Bot的Task
    /// </summary>
    /// <param name="method"></param>
    private static void StartTaskList(ICollection<Task> taskList, Action method)
    {
        var task = new Task(method);
        taskList.Add(task);
        task.Start();
    }
}