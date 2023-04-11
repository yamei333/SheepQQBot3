using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MessagePack;
using Yamei.Common;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// Bot配置类
    /// </summary>
    [MessagePackObject]
    public class BotConfig
    {
        [JsonIgnore]
        [IgnoreMember]
        private Dictionary<Guid, SetConfig> _setConfigs;

        /// <summary>
        /// 群,个人等配置
        /// </summary>
        [Key(nameof(SetConfigs))]
        public Dictionary<Guid, SetConfig> SetConfigs
        {
            get => _setConfigs ??= new Dictionary<Guid, SetConfig>();
            set => _setConfigs = value;
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

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public BotConfig()
        { }

        /// <summary>
        /// 初始化BotFuntion可用状态
        /// </summary>
        public void InitBotFunctionIsEnabled()
        {
            SetConfigs.Values.ForEach(each => each.InitBotFunctionIsEnabled());
        }
    }
}