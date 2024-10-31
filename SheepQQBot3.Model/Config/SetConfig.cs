using CommonLibrary;
using MessagePack;
using SheepQQBot3.Model.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace SheepQQBot3.Model.Config;

/// <summary>
/// 配置类
/// </summary>
[MessagePackObject]
public class SetConfig
{
    /// <summary>
    /// 默认BotFunction
    /// </summary>
    [JsonIgnore]
    public static readonly List<BotFunction> DefaultBotFunctions = Enum.GetNames(typeof(BotFunctionType))
        .Select(each => new BotFunction((BotFunctionType)Enum.Parse(typeof(BotFunctionType), each), false))
        .ToList();

    /// <summary>
    /// 配置中的功能
    /// </summary>
    [Key(nameof(BotFunctions))]
    public List<BotFunction> BotFunctions { get; set; }

    /// <summary>
    /// 配置ID
    /// </summary>
    [Key(nameof(Id))]
    public Guid Id { get; set; }

    /// <summary>
    /// 配置显示的图标
    /// 头像大图链接 https://q1.qlogo.cn/g?b=qq&nk={QQ号}252961222}&s=640
    /// </summary>
    [JsonIgnore]
    [IgnoreMember]
    public BitmapFrame Icon => TargetType switch
    {
        BotConfigTargetType.Common => QQExtensions.GetQQImage(int.Parse(AppSettingExtensions.Get("selfId", "0"))),
        BotConfigTargetType.Group => QQExtensions.GetQQGroupImage(TargetId),
        BotConfigTargetType.Private => QQExtensions.GetQQImage(TargetId),
        _ => QQExtensions.GetQQImage(10000),
    };

    /// <summary>
    /// 对象类型
    /// </summary>
    [Key(nameof(TargetType))]
    public BotConfigTargetType TargetType { get; set; }

    /// <summary>
    /// 闹钟助手配置
    /// </summary>
    [Key(nameof(AlarmAideConfigs))]
    public Dictionary<Guid, AlarmAideConfig> AlarmAideConfigs { get; set; }

    /// <summary>
    /// 闹钟助手允许投稿成员ID配置
    /// </summary>
    [Key(nameof(AlarmAideSubmitMemberIds))]
    public HashSet<long> AlarmAideSubmitMemberIds { get; set; }

    /// <summary>
    /// 黑名单ID配置
    /// </summary>
    [Key(nameof(BlackListIds))]
    public HashSet<long> BlackListIds { get; set; }

    /// <summary>
    /// 基金播报配置
    /// </summary>
    [Key(nameof(FundAlarmConfigs))]
    public Dictionary<Guid, FundAlarmConfig> FundAlarmConfigs { get; set; }

    /// <summary>
    /// 基金阈值观测配置
    /// </summary>
    [Key(nameof(FundLimitObserveConfigs))]
    public Dictionary<Guid, FundLimitObserveConfig> FundLimitObserveConfigs { get; set; }

    /// <summary>
    /// 复读机杀手配置
    /// </summary>
    [Key(nameof(RepeaterKillerConfigs))]
    public Dictionary<Guid, RepeaterKillerConfig> RepeaterKillerConfigs { get; set; }

    /// <summary>
    /// 直播提醒配置
    /// </summary>
    [Key(nameof(LiveAlarmConfigs))]
    public Dictionary<Guid, LiveAlarmConfig> LiveAlarmConfigs { get; set; }

    #region 已执行内容的保存

    [JsonIgnore]
    [IgnoreMember]
    private List<int> _processedMessageIds;

    /// <summary>
    /// 保存已处理的消息ID
    /// </summary>
    [Key(nameof(ProcessedMessageIds))]
    public List<int> ProcessedMessageIds
    {
        get => _processedMessageIds ??= new List<int>();
        set => _processedMessageIds = value;
    }

    [JsonIgnore]
    [IgnoreMember]
    private Dictionary<Guid, DateTime> _alarmAideAlarmedList;

    /// <summary>
    /// 保存已提醒闹钟列表
    /// </summary>
    [Key(nameof(AlarmAideAlarmedList))]
    public Dictionary<Guid, DateTime> AlarmAideAlarmedList
    {
        get => _alarmAideAlarmedList ??= new Dictionary<Guid, DateTime>();
        set => _alarmAideAlarmedList = value;
    }

    [JsonIgnore]
    [IgnoreMember]
    private ConcurrentDictionary<Guid, DateTime> _fundAlarmedList;

    /// <summary>
    /// 保存已执行基金播报任务
    /// </summary>
    [Key(nameof(FundAlarmedList))]
    public ConcurrentDictionary<Guid, DateTime> FundAlarmedList
    {
        get => _fundAlarmedList ??= new ConcurrentDictionary<Guid, DateTime>();
        set => _fundAlarmedList = value;
    }

    [JsonIgnore]
    [IgnoreMember]
    private ConcurrentDictionary<Guid, DateTime> _fundLimitObservedList;

    /// <summary>
    /// 保存已执行基金观测任务
    /// </summary>
    [Key(nameof(FundLimitObservedList))]
    public ConcurrentDictionary<Guid, DateTime> FundLimitObservedList
    {
        get => _fundLimitObservedList ??= new ConcurrentDictionary<Guid, DateTime>();
        set => _fundLimitObservedList = value;
    }

    [JsonIgnore]
    [IgnoreMember]
    private Dictionary<Guid, DateTime> _liveAlarmedList;

    /// <summary>
    /// 保存已执行直播提醒任务 (不缓存)
    /// </summary>
    [JsonIgnore]
    [IgnoreMember]
    public Dictionary<Guid, DateTime> LiveAlarmedList
    {
        get => _liveAlarmedList ??= new Dictionary<Guid, DateTime>();
        set => _liveAlarmedList = value;
    }

    #endregion 已执行内容的保存

    /// <summary>
    /// 显示文字
    /// </summary>
    [JsonIgnore]
    [IgnoreMember]
    public string DisplayId =>
        TargetType switch
        {
            BotConfigTargetType.Common => $"系统-{TargetId}",
            BotConfigTargetType.Group => $"群-{TargetId}",
            BotConfigTargetType.Private => $"{TargetId}",
            _ => throw new NotImplementedException(),
        };

    /// <summary>
    /// 对象ID(群号/个人QQ号)
    /// </summary>
    [Key(nameof(TargetId))]
    public long TargetId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Key(nameof(TargetName))]
    public string TargetName { get; set; }

    public SetConfig(Guid id, BotConfigTargetType targetType, long targetId, string targetName)
    {
        Id = id;
        TargetType = targetType;
        TargetId = targetId;
        TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
        BotFunctions = CommonExtensions.Clone(DefaultBotFunctions);
        AlarmAideConfigs = new Dictionary<Guid, AlarmAideConfig>();
        AlarmAideSubmitMemberIds = new HashSet<long>();
        AlarmAideAlarmedList = new Dictionary<Guid, DateTime>();
        FundAlarmConfigs = new Dictionary<Guid, FundAlarmConfig>();
        FundLimitObserveConfigs = new Dictionary<Guid, FundLimitObserveConfig>();
        FundAlarmedList = new ConcurrentDictionary<Guid, DateTime>();
        FundLimitObservedList = new ConcurrentDictionary<Guid, DateTime>();
        LiveAlarmedList = new Dictionary<Guid, DateTime>();
        RepeaterKillerConfigs = new Dictionary<Guid, RepeaterKillerConfig>();
        LiveAlarmConfigs = new Dictionary<Guid, LiveAlarmConfig>();
        InitBotFunctionIsEnabled();
#if (!debug)
#else
            //AlarmAideConfigs = new List<AlarmAideConfig>()
            //{
            //    new AlarmAideConfig()
            //    {
            //        AlarmName = "zap",
            //        LoopDays = "12345",
            //        Condition = "111",
            //        Id =  Guid.NewGuid(),
            //        IsActive = true,
            //        AlarmTexts = new List<string> {"zap1","zap2"},
            //    },
            //    new AlarmAideConfig()
            //    {
            //        AlarmName = "zap",
            //        LoopDays = "12345",
            //        Condition = "111",
            //        Id =  Guid.NewGuid(),
            //        IsActive = false,
            //        AlarmTexts = new List<string> {"zap1","zap2"},
            //    }
            //};
#endif
    }

    /// <summary>
    /// 初始化BotFuntion可用状态
    /// </summary>
    internal void InitBotFunctionIsEnabled()
    {
        var allowFunctions = TargetType.GetAllowFunctions();
        BotFunctions.ForEach(each => each.IsEnabled = allowFunctions.Contains(each.BotFunctionType));
    }

    ///// <summary>
    ///// 通过URL取得图标
    ///// </summary>
    ///// <param name="url">URL地址</param>
    ///// <returns><see cref="BitmapImage"/></returns>
    //private static BitmapImage GetHttpIcon(string url)
    //{
    //    var image = new BitmapImage();
    //    const int bytesToRead = 100;

    //    var response = HttpExtensions.SendHttpResponse(url);
    //    var responseStream = response.Content.ReadAsStream();

    //    var reader = new BinaryReader(responseStream);
    //    var memoryStream = new MemoryStream();
    //    var bytebuffer = new byte[bytesToRead];
    //    var bytesRead = reader.Read(bytebuffer, 0, bytesToRead);
    //    while (bytesRead > 0)
    //    {
    //        memoryStream.Write(bytebuffer, 0, bytesRead);
    //        bytesRead = reader.Read(bytebuffer, 0, bytesToRead);
    //    }

    //    image.BeginInit();
    //    memoryStream.Seek(0, SeekOrigin.Begin);
    //    image.StreamSource = memoryStream;
    //    image.EndInit();
    //    return image;
    //}
}