using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;
using MessagePack;
using SheepQQBot3.Model.Enums;
using SheepQQBot3.Model.Extension;

namespace SheepQQBot3.Model.Config
{
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
        /// </summary>
        [JsonIgnore]
        [IgnoreMember]
        public BitmapImage Icon =>
            TargetType switch
            {
                BotConfigTargetType.Common => null,
                BotConfigTargetType.Group => GetHttpIcon($"https://p.qlogo.cn/gh/{TargetId}/{TargetId}/40/"),
                BotConfigTargetType.Private => GetHttpIcon($"https://q.qlogo.cn/headimg_dl?dst_uin={TargetId}&spec=40"),
                _ => throw new NotImplementedException()
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

        /// <summary>
        /// 原神助手配置
        /// </summary>
        [Key(nameof(GenshinHelperConfig))]
        public GenshinHelperConfig GenshinHelperConfig { get; set; }

        #region 已执行内容的保存

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

        /// <summary>
        /// 保存群自定义提醒内容
        /// </summary>
        [Key(nameof(CustomGroupAlarms))]
        public Dictionary<Guid, CustomGroupAlarm> CustomGroupAlarms { get; set; }

        [JsonIgnore]
        [IgnoreMember]
        private Dictionary<Guid, DateTime> _fundAlarmedList;

        /// <summary>
        /// 保存已执行基金播报任务
        /// </summary>
        [Key(nameof(FundAlarmedList))]
        public Dictionary<Guid, DateTime> FundAlarmedList
        {
            get => _fundAlarmedList ??= new Dictionary<Guid, DateTime>();
            set => _fundAlarmedList = value;
        }

        [JsonIgnore]
        [IgnoreMember]
        private Dictionary<Guid, DateTime> _fundLimitObservedList;

        /// <summary>
        /// 保存已执行基金观测任务
        /// </summary>
        [Key(nameof(FundLimitObservedList))]
        public Dictionary<Guid, DateTime> FundLimitObservedList
        {
            get => _fundLimitObservedList ??= new Dictionary<Guid, DateTime>();
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

        [JsonIgnore]
        [IgnoreMember]
        private Dictionary<(Guid Id, GenshinDailyNoteAlarmType AlarmType), DateTime> _genshinResinAlarmedList;

        /// <summary>
        /// 保存已执行原神每日提醒任务 (不缓存)
        /// </summary>
        [JsonIgnore]
        [IgnoreMember]
        public Dictionary<(Guid Id, GenshinDailyNoteAlarmType AlarmType), DateTime> GenshinResinAlarmedList
        {
            get => _genshinResinAlarmedList ??= new Dictionary<(Guid Id, GenshinDailyNoteAlarmType AlarmType), DateTime>();
            set => _genshinResinAlarmedList = value;
        }

        [JsonIgnore]
        [IgnoreMember]
        private Dictionary<long, DateTime> _setuSendRecords;

        /// <summary>
        /// 色图最后发送时间记录
        /// </summary>
        [Key(nameof(SetuSendLastRecords))]
        public Dictionary<long, DateTime> SetuSendLastRecords
        {
            get => _setuSendRecords ??= new Dictionary<long, DateTime>();
            set => _setuSendRecords = value;
        }

        [JsonIgnore]
        [IgnoreMember]
        private Dictionary<long, DateTime> _canSetuSendCDs;

        /// <summary>
        /// 保存色图的CD
        /// </summary>
        [Key(nameof(CanSetuSendCDs))]
        public Dictionary<long, DateTime> CanSetuSendCDs
        {
            get => _canSetuSendCDs ??= new Dictionary<long, DateTime>();
            set => _canSetuSendCDs = value;
        }

        [JsonIgnore]
        [IgnoreMember]
        private Dictionary<long, int> _setuSenderLv;

        /// <summary>
        /// 保存色图斗士Lv
        /// </summary>
        [Key(nameof(SetuSenderLv))]
        public Dictionary<long, int> SetuSenderLv
        {
            get => _setuSenderLv ??= new Dictionary<long, int>();
            set => _setuSenderLv = value;
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
                _ => throw new NotImplementedException()
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
            FundAlarmedList = new Dictionary<Guid, DateTime>();
            FundLimitObservedList = new Dictionary<Guid, DateTime>();
            LiveAlarmedList = new Dictionary<Guid, DateTime>();
            CustomGroupAlarms = new Dictionary<Guid, CustomGroupAlarm>();
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

        /// <summary>
        /// 通过URL取得图标
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <returns><see cref="BitmapImage"/></returns>
        private static BitmapImage GetHttpIcon(string url)
        {
            var image = new BitmapImage();
            const int bytesToRead = 100;

            var response = HttpExtensions.SendHttpResponse(url);
            var responseStream = response.Content.ReadAsStream();

            var reader = new BinaryReader(responseStream);
            var memoryStream = new MemoryStream();
            var bytebuffer = new byte[bytesToRead];
            var bytesRead = reader.Read(bytebuffer, 0, bytesToRead);
            while (bytesRead > 0)
            {
                memoryStream.Write(bytebuffer, 0, bytesRead);
                bytesRead = reader.Read(bytebuffer, 0, bytesToRead);
            }

            image.BeginInit();
            memoryStream.Seek(0, SeekOrigin.Begin);
            image.StreamSource = memoryStream;
            image.EndInit();
            return image;
        }
    }
}