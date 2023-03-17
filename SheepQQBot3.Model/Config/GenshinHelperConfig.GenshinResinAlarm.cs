using System;
using MessagePack;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// 原神树脂提醒
    /// </summary>
    [MessagePackObject]
    public class GenshinResinAlarm : NotifyPropertyChangedConfigBase
    {
        /// <summary>
        /// 配置名称
        /// </summary>
        [Key(nameof(ConfigName))]
        public string ConfigName { get; set; }

        /// <summary>
        /// 提醒对象QQ号
        /// </summary>
        [Key(nameof(TargetId))]
        public long TargetId { get; set; }

        /// <summary>
        /// Cookies
        /// </summary>
        [Key(nameof(Cookies))]
        public string Cookies { get; set; }

        /// <summary>
        /// BarkKey
        /// </summary>
        [Key(nameof(BarkKey))]
        public string BarkKey { get; set; }

        [Key(nameof(_resin))]
        private bool _resin;

        /// <summary>
        /// 树脂
        /// </summary>
        [IgnoreMember]
        public bool Resin
        {
            get => _resin;
            set
            {
                _resin = value;
                OnPropertyChanged(nameof(Resin));
            }
        }

        [Key(nameof(_dailyMission))]
        private bool _dailyMission;

        /// <summary>
        /// 每日任务
        /// </summary>
        [IgnoreMember]
        public bool DailyMission
        {
            get => _dailyMission;
            set
            {
                _dailyMission = value;
                OnPropertyChanged(nameof(DailyMission));
            }
        }

        [Key(nameof(_potCoin))]
        private bool _potCoin;

        /// <summary>
        /// 洞天宝钱
        /// </summary>
        [IgnoreMember]
        public bool PotCoin
        {
            get => _potCoin;
            set
            {
                _potCoin = value;
                OnPropertyChanged(nameof(PotCoin));
            }
        }

        [Key(nameof(_transformer))]
        private bool _transformer;

        /// <summary>
        /// 参量质变仪
        /// </summary>
        [IgnoreMember]
        public bool Transformer
        {
            get => _transformer;
            set
            {
                _transformer = value;
                OnPropertyChanged(nameof(Transformer));
            }
        }

        /// <inheritdoc />
        public GenshinResinAlarm(
            Guid id,
            string configName,
            string cookies,
            string barkKey,
            long targetId)
        {
            Id = id;
            ConfigName = configName;
            Cookies = cookies;
            BarkKey = barkKey;
            TargetId = targetId;
            Resin = true;
            DailyMission = true;
            PotCoin = true;
            Transformer = false;
        }
    }
}