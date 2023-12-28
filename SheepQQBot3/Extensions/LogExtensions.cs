using SheepQQBot3.Model.Config;

namespace SheepQQBot3.Extensions;

public static class LogExtensions
{
    //public static RunLog_SystemInfo RunLog_SystemInfo(string content)
    //    => new RunLog_SystemInfo(content);

    /// <summary>
    /// 增加日志
    /// </summary>
    /// <param name="runLog">日志</param>
    public static void AddRunLog(RunLog runLog) => PublicVar.Vm.AddRunLog(runLog);

    /// <summary>
    /// 增加任务启动日志
    /// </summary>
    /// <param name="taskName">任务名</param>
    public static void AddTaskRunLog(string taskName) => PublicVar.Vm.AddRunLog(new RunLog_SystemInfo($"{taskName} 模块已运行"));
}