using SheepQQBot3.BotTask;
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

    /// <summary>
    /// 初始化Bot任务
    /// </summary>
    public void InitBotFunctions()
    {
        CancelToken = new CancellationTokenSource();
        StartTask(TaskProcess.AlarmAides);
        StartTask(TaskProcess.CustomAlarm);
        StartTask(TaskProcess.FundHelper);
        StartTask(TaskProcess.LiveAlarm);
        if (PublicVar.AIConfig.IsUseAI)
            StartTask(TaskProcess.AIStatusRecover);

        // MEMO : 0.14.9.2 暂时禁用steam监控, 发送推送消息太频繁了
        //StartTask(TaskProcess.SteamMarketWatchAsync);
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