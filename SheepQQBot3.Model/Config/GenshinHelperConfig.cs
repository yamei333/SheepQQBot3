using System;
using System.Collections.Generic;
using MessagePack;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// 原神助手配置
    /// </summary>
    [MessagePackObject]
    public class GenshinHelperConfig : NotifyPropertyChangedBase
    {
        /// <summary>
        /// 树脂提醒列表
        /// </summary>
        [Key(nameof(GenshinResinAlarms))]
        public Dictionary<Guid, GenshinResinAlarm> GenshinResinAlarms { get; set; }

        /// <inheritdoc />
        public GenshinHelperConfig()
        {
            GenshinResinAlarms = new Dictionary<Guid, GenshinResinAlarm>();
        }
    }
}