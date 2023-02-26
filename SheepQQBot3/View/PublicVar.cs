using System;
using System.Collections.Generic;
using System.Diagnostics;
using SheepQQBot3.SDK.Client;

namespace SheepQQBot3.View
{
    /// <summary>
    /// 存放公共变量
    /// </summary>
    public static class PublicVar
    {
        /// <summary>
        /// 随机数产生器
        /// </summary>
        public static Random Rand = new Random();

        /// <summary>
        /// 是否Debug模式
        /// </summary>
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
        /// 回车换行
        /// </summary>
        public const string ENTER = "\r\n";

        /// <summary>
        /// <see cref="MainWindow"/>
        /// </summary>
        public static MainWindow MWindow { get; set; }

        /// <summary>
        /// <see cref="MainWindowViewModel"/>
        /// </summary>
        public static MainWindowViewModel Vm { get; set; }

        /// <summary>
        /// <see cref="CQAPI"/>
        /// </summary>
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
        /// <param name="vm"><see cref="MainWindowViewModel"/></param>
        public static void InitPublicVar(MainWindowViewModel vm)
        {
            Vm = vm;
        }
    }
}