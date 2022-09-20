using SheepQQBot3.Model.Config;
using SheepQQBot3.View;

namespace SheepQQBot3.Extensions
{
    public static class LogExtensions
    {
        //public static RunLog_SystemInfo RunLog_SystemInfo(string content)
        //    => new RunLog_SystemInfo(content);

        /// <summary>
        /// 增加日志
        /// </summary>
        /// <param name="runLog"></param>
        public static void AddRunLog(RunLog runLog) => PublicVar.Vm.AddRunLog(runLog);
    }
}