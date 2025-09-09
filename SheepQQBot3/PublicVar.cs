using CommonLibrary;
using Masuit.Tools;
using Microsoft.EntityFrameworkCore;
using OpenWeatherMap.Standard;
using SheepQQBot3.DbModel;
using SheepQQBot3.DbModel.JiebaDb;
using SheepQQBot3.Model;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Config;
using SheepQQBot3.SDK.Server;
using SheepQQBot3.View;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    /// 系统ID
    /// </summary>
    public static readonly long SystemId = 10000;

    /// <summary>
    /// 测试QQID
    /// </summary>
    public static readonly long TestQQId = 205552607;

    /// <summary>
    /// 测试群号
    /// </summary>
    public static readonly long TestGroupId = 15873217;

    /// <summary>
    /// 超级管理ID
    /// </summary>
    public static readonly long SuperId = 252961222;

    /// <summary>
    /// 管理员ID
    /// </summary>
    public static readonly HashSet<long> AdminIds = AppSettingExtensions.Get("adminId").Split(',').ToHashSet(long.Parse);

    /// <summary>
    /// BotID
    /// </summary>
    public static readonly long BotId = long.Parse(AppSettingExtensions.Get("selfId", "0"));

    /// <summary>
    /// 色图斗士信息缓存
    /// </summary>
    public static readonly ConcurrentDictionary<long, SetuDoushiInfo> SetuDoushiInfoCache = [];

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
    /// 最大重试次数
    /// </summary>
    public const int AI_MAX_RETRY_TIMES = 1;

    /// <summary>
    /// AI最短请求间隔, 群内at(30秒)
    /// </summary>
    public const int AI_REQUEST_INTERVAL_GROUP_PRIVATE = 30;

    /// <summary>
    /// AI最短请求间隔, 个人(20秒)
    /// </summary>
    public const int AI_REQUEST_INTERVAL_PRIVATE = 20;

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

    public static NapCatWindow NapCatWindow;
    public static Process Bark;
    public static BarkWindow BarkWindow;

    /// <summary>
    /// 已记录的群消息(用于防止撤回)
    /// </summary>
    public static ConcurrentDictionary<int, GroupMessage> SavedGroupMessages { get; set; } = [];

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
    /// AIConfig
    /// </summary>
    public static AIConfig AIConfig { get; set; }

    /// <summary>
    /// AI数据
    /// </summary>
    public static AIData AIData { get; set; }

    /// <summary>
    /// AICharacter
    /// </summary>
    public static AICharacter AICharacter { get; set; }

    /// <summary>
    /// AI控制
    /// </summary>
    public static AIControl AIControl { get; set; }

    /// <summary>
    /// AI请求时间记录
    /// </summary>
    public static ConcurrentDictionary<string, DateTime> AILastRequestDates { get; set; } = [];

    /// <summary>
    /// AI用户信息(好感度描述等)
    /// </summary>
    public static ConcurrentDictionary<long, AIUserInfo> AIUserInfoDictionary { get; set; } = [];

    /// <summary>
    /// OpenWeatherMap服务
    /// </summary>
    public static Current OpenWeatherMapService { get; set; }

    /// <summary>
    /// 初始化全局变量
    /// </summary>
    /// <param name="vm"><see cref="MainWindowViewModel"/></param>
    public static void InitPublicVar(MainWindowViewModel vm)
    {
        Vm = vm;
    }
}