using Masuit.Tools;
using Microsoft.EntityFrameworkCore;
using SheepQQBot3.DbModel;
using SheepQQBot3.DbModel.JiebaDb;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.SDK.Server;
using SheepQQBot3.View;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;

namespace SheepQQBot3;

/// <summary>
/// 存放公共变量
/// </summary>
public static class PublicVar
{
    /// <summary>
    /// Bot名称
    /// </summary>
    public const string BOT_NAME = "助手哈莉";

    /// <summary>
    /// 随机数产生器
    /// </summary>
    public static Random Rand = new();

    /// <summary>
    /// 是否Debug模式
    /// </summary>
    public static bool IsDebug = false;

    /// <summary>
    /// 管理员ID
    /// </summary>
    public static readonly long CommonId = 22222;

    /// <summary>
    /// 管理员ID
    /// </summary>
    public static readonly HashSet<long> AdminIds = ConfigurationManager.AppSettings["adminId"]!.Split(',').ToHashSet(long.Parse);

    /// <summary>
    /// BotID
    /// </summary>
    public static readonly long BotId = long.Parse(ConfigurationManager.AppSettings["selfId"]!);

    /// <summary>
    /// 色图斗士信息缓存
    /// </summary>
    public static readonly ConcurrentDictionary<long, SetuDoushiInfo> SetuDoushiInfoCache = new();

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
    /// <see cref="SDK.Server.BotServer"/>
    /// </summary>
    public static BotServer BotServer => Vm.BotServer;

    public static Process NapCat;
    public static NapCatWindow NapCatWindow;
    public static Process Bark;
    public static BarkWindow BarkWindow;

    public static ConcurrentDictionary<long, DGPDailyNote> GenshinDailyNote = new();
    public static bool DGPProcessOK = false;

    /// <summary>
    /// Jieba数据库
    /// </summary>
    public static JiebaDbContext JiebaDb = new(new DbContextOptions<JiebaDbContext>());

    /// <summary>
    /// Bot数据库
    /// </summary>
    public static BotDbContext BotDb = new(new DbContextOptions<BotDbContext>());

    /// <summary>
    /// Bot配置
    /// </summary>
    public static BotConfig BotConfig { get; set; }

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