using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.Model.Config
{
    [Serializable]
    public class SetConfig
    {
        [JsonIgnore]
        public static readonly List<BotFunction> DefaultBotFunctions = Enum.GetNames(typeof(BotFunctionType))
            .Select(each => new BotFunction((BotFunctionType)Enum.Parse(typeof(BotFunctionType), each), false))
            .ToList();

        /// <summary>
        /// 配置中的功能
        /// </summary>
        public List<BotFunction> BotFunctions { get; set; }

        /// <summary>
        /// 配置ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 配置显示的图标
        /// </summary>
        [JsonIgnore]
        public BitmapImage Icon =>
            TargetType switch
            {
                BotConfigTargetType.Common => null,
                BotConfigTargetType.Group => GetHttpIcon($"http://p.qlogo.cn/gh/{TargetId}/{TargetId}/40/"),
                BotConfigTargetType.Private => GetHttpIcon($"http://q.qlogo.cn/headimg_dl?dst_uin={TargetId}&spec=40"),
                _ => throw new NotImplementedException()
            };

        /// <summary>
        /// 对象类型
        /// </summary>
        public BotConfigTargetType TargetType { get; set; }

        /// <summary>
        /// 闹钟助手配置
        /// </summary>
        public Dictionary<Guid, AlarmAideConfig> AlarmAideConfigs { get; set; }

        /// <summary>
        /// 闹钟助手允许投稿成员ID配置
        /// </summary>
        public HashSet<long> AlarmAideSubmitMemberIds { get; set; }

        /// <summary>
        /// 基金播报配置
        /// </summary>
        public Dictionary<Guid, FundAlarmConfig> FundAlarmConfigs { get; set; }

        /// <summary>
        /// 基金阈值观测配置
        /// </summary>
        public Dictionary<Guid, FundLimitObserveConfig> FundLimitObserveConfigs { get; set; }

        /// <summary>
        /// 复读机杀手配置
        /// </summary>
        public Dictionary<Guid, RepeaterKillerConfig> RepeaterKillerConfigs { get; set; }

        #region 已执行内容的保存

        /// <summary>
        /// 保存已提醒闹钟列表
        /// </summary>
        public Dictionary<Guid, DateTime> AlarmAideAlarmedList { get; set; }

        /// <summary>
        /// 保存群自定义提醒内容
        /// </summary>
        public Dictionary<Guid, CustomGroupAlarm> CustomGroupAlarms { get; set; }

        private Dictionary<Guid, DateTime> _fundAlarmedList;
        /// <summary>
        /// 保存已执行基金播报任务
        /// </summary>
        public Dictionary<Guid, DateTime> FundAlarmedList
        {
            get => _fundAlarmedList ??= new Dictionary<Guid, DateTime>();
            set => _fundAlarmedList = value;
        }

        private Dictionary<Guid, DateTime> _fundLimitObservedList;
        /// <summary>
        /// 保存已执行基金观测任务
        /// </summary>
        public Dictionary<Guid, DateTime> FundLimitObservedList
        {
            get => _fundLimitObservedList ??= new Dictionary<Guid, DateTime>();
            set => _fundLimitObservedList = value;
        }

        #endregion 已执行内容的保存

        /// <summary>
        /// 显示文字
        /// </summary>
        [JsonIgnore]
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
        public long TargetId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
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
            CustomGroupAlarms = new Dictionary<Guid, CustomGroupAlarm>();
            RepeaterKillerConfigs = new Dictionary<Guid, RepeaterKillerConfig>();
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
        /// 通过URL取得图标
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <returns><see cref="BitmapImage"/></returns>
        private BitmapImage GetHttpIcon(string url)
        {
            var image = new BitmapImage();
            const int bytesToRead = 100;

            var request = WebRequest.Create(new Uri(url, UriKind.Absolute));
            request.Timeout = -1;
            request.Credentials = CredentialCache.DefaultNetworkCredentials;
            var response = request.GetResponse();
            var responseStream = response.GetResponseStream();
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