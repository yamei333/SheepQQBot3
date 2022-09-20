using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using SheepQQBot3.Model;

namespace SheepQQBot3.SDK.Client
{
    public static class CommonUtil
    {
        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO dummy);

        [DllImport("Kernel32.dll")]
        private static extern uint GetLastError();

        private struct LASTINPUTINFO
        {
            public uint CbSize;
            public uint DwTime;
        }

        /// <summary>
        /// 获取计算机是否闲置状态
        /// </summary>
        /// <param name="overTime"></param>
        /// <returns></returns>
        public static bool GetIsNotIdle(int overTime)
        {
            var processNames = Process.GetProcesses().Select(each => each.ProcessName).ToHashSet();
            var specialProcessNames = new HashSet<string> { "DouyuLive" };

            return GetIdleTime() <= overTime
                || specialProcessNames.Any(each => processNames.Contains(each));
        }

        public static uint GetIdleTime()
        {
            var lastUserAction = new LASTINPUTINFO();
            lastUserAction.CbSize = (uint)Marshal.SizeOf(lastUserAction);
            GetLastInputInfo(ref lastUserAction);
            return (uint)Environment.TickCount - lastUserAction.DwTime;
        }

        private static long GetLastInputTime()
        {
            var lastUserAction = new LASTINPUTINFO();
            lastUserAction.CbSize = (uint)Marshal.SizeOf(lastUserAction);
            if (!GetLastInputInfo(ref lastUserAction))
                throw new Exception(GetLastError().ToString());

            return lastUserAction.DwTime;
        }

        public static byte[] GetSendData(SendData sendData)
        {
            var jsonText = JsonSerializer.Serialize(sendData);
            return Encoding.UTF8.GetBytes(jsonText);
        }

        public static void Sleep(Func<bool> condition, int time)
        {
            SpinWait.SpinUntil(condition, time);
        }

        public static void Sleep(int time)
        {
            SpinWait.SpinUntil(() => false, time);
        }
    }
}