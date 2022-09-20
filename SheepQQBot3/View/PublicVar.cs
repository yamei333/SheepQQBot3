using System;
using System.Collections.Generic;
using System.Diagnostics;
using SheepQQBot3.SDK.Client;

namespace SheepQQBot3.View
{
    public static class PublicVar
    {
        public static Random Rand = new Random();

        public static bool IsDebug = false;

        /// <summary>
        /// 管理员ID
        /// </summary>
        public const long ADMIN_ID = 252961222;

        /// <summary>
        /// 半角逗号
        /// </summary>
        public const char COMMA = ',';

        /// <summary>
        /// 全角逗号
        /// </summary>
        public const char COMMA_FULL = '，';

        /// <summary>
        /// 横线
        /// </summary>
        public const char LINE_CHAR = '-';

        /// <summary>
        /// 空格
        /// </summary>
        public const string SPACE = " ";

        /// <summary>
        /// 全角的空格
        /// </summary>
        public const string SPACE_FULL = "　";

        /// <summary>
        /// 回车符
        /// </summary>
        public const string ENTER = "\r\n";

        /// <summary>
        /// 换行
        /// </summary>
        public const string RN = ENTER;

        public static MainWindow MWindow { get; set; }
        public static MainWindowViewModel Vm { get; set; }
        public static CQAPI Api => Vm.CqApi;
        public static Process Gocq;
        public static GocqWindow GocqEmbedWindow;

        /// <summary>
        /// 节假日信息
        /// </summary>
        public static Dictionary<string, bool> HolidayInfo { get; set; }

        /// <summary>
        /// 初始化全局变量
        /// </summary>
        /// <param name="vm"></param>
        public static void InitPublicVar(MainWindowViewModel vm)
        {
            Vm = vm;
        }
    }
}