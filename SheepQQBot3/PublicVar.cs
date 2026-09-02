using CommonLibrary;
using Masuit.Tools;
using OpenAI.Chat;
using OpenWeatherMap.Standard;
using SheepQQBot3.Model;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Config;
using SheepQQBot3.SDK.Client;
using SheepQQBot3.SDK.Server;
using SheepQQBot3.View;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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
    /// Bot昵称
    /// </summary>
    public const string BOT_NICK_NAME = "哈莉";

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
    public static readonly string AISystemId = "22222";

    /// <summary>
    /// 管理员ID
    /// </summary>
    public static readonly string AISystemHintName = "系统提示";

    /// <summary>
    /// 系统ID
    /// </summary>
    public static readonly string SystemId = "10000";

    /// <summary>
    /// 测试QQID
    /// </summary>
    public static readonly string TestQQId = "205552607";

    /// <summary>
    /// 测试群号
    /// </summary>
    public static readonly string TestGroupId = "15873217";

    /// <summary>
    /// 超级管理ID
    /// </summary>
    public static readonly string SuperAdminId = "252961222";

    /// <summary>
    /// 管理员ID
    /// </summary>
    public static readonly HashSet<string> AdminIds = AppSettingExtensions.Get("adminId").Split(',').ToHashSet(each => each.ToString());

    /// <summary>
    /// BotID
    /// </summary>
    public static readonly string BotId = AppSettingExtensions.Get("selfId", string.Empty);

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
    public const int AI_MAX_RETRY_TIMES = 2;

    /// <summary>
    /// AI最短请求间隔, 群内at(30秒)
    /// </summary>
    public const int AI_REQUEST_INTERVAL_GROUP_PRIVATE = 45;

    /// <summary>
    /// AI最短请求间隔, 个人(20秒)
    /// </summary>
    public const int AI_REQUEST_INTERVAL_PRIVATE = 20;

    /// <summary>
    /// <see cref="MainWindow"/>
    /// </summary>
    public static MainWindow GlobalMainWindow { get; set; }

    /// <summary>
    /// <see cref="MainWindowViewModel"/>
    /// </summary>
    public static MainWindowViewModel Vm { get; set; }

    /// <summary>
    /// <see cref="SDK.Server.BotServer"/>
    /// </summary>
    public static BotServer GlobalBotServer => Vm.BotServer;

    /// <summary>
    /// <see cref="SDK.Client.BotClient"/>
    /// </summary>
    public static BotClient GlobalBotClient => Vm.BotClient;

    public static Process Bark;
    public static BarkWindow GlobalBarkWindow;

    /// <summary>
    /// 已记录的群消息(用于防止撤回)
    /// </summary>
    public static ConcurrentDictionary<string, GroupMessage> SavedGroupMessages { get; set; } = [];

    /// <summary>
    /// Bot配置
    /// </summary>
    public static BotConfig GlobalBotConfig { get; set; }

    /// <summary>
    /// 节假日信息
    /// </summary>
    public static Dictionary<string, bool> HolidayInfo { get; set; }

    /// <summary>
    /// AIClientChat
    /// </summary>
    public static ChatClient AIClientChat { get; set; }

    /// <summary>
    /// AIClientSummary
    /// </summary>
    public static ChatClient AIClientSummary { get; set; }

    /// <summary>
    /// AIClientImage
    /// </summary>
    public static ChatClient AIClientImage { get; set; }

    /// <summary>
    /// AIConfig
    /// </summary>
    public static AIConfig GlobalAIConfig { get; set; }

    /// <summary>
    /// AI数据
    /// </summary>
    public static AIData GlobalAIData { get; set; }

    /// <summary>
    /// AICharacter
    /// </summary>
    public static AICharacter GlobalAICharacter { get; set; }

    /// <summary>
    /// AI控制
    /// </summary>
    public static AIControl GlobalAIControl { get; set; }

    /// <summary>
    /// AI请求时间记录
    /// </summary>
    public static ConcurrentDictionary<string, DateTime> AILastRequestDates { get; set; } = [];

    /// <summary>
    /// AI用户信息(好感度描述等)
    /// </summary>
    public static ConcurrentDictionary<string, AIUserInfo> AIUserInfoDictionary { get; set; } = [];

    /// <summary>
    /// OpenWeatherMap服务
    /// </summary>
    public static Current OpenWeatherMapService { get; set; }

    /// <summary>
    /// AI记录的群历史消息
    /// </summary>
    public static readonly ConcurrentDictionary<string, List<ChatMessageContentPart>> AIHistoryParts = [];

    /// <summary>
    /// AI知识库-用户信息
    /// </summary>
    public static ConcurrentDictionary<string, AIChatSender> AIUserInfos;

    /// <summary>
    /// 初始化全局变量
    /// </summary>
    /// <param name="vm"><see cref="MainWindowViewModel"/></param>
    public static void InitPublicVar(MainWindowViewModel vm)
    {
        Vm = vm;
        var aiUserInfoPath = "AICache/userInfo.json";
        AIUserInfos = File.Exists(aiUserInfoPath)
            ? JsonExtensions.FromJsonFile<GroupMemberInfo>(aiUserInfoPath).UserInfos
            : [];
    }
}