using System;
using System.Collections.Generic;
using MessagePack;

namespace SheepQQBot3.Model.Config
{
    [MessagePackObject]
    public class BotConfig
    {
        [Key(nameof(SetConfigs))]
        public Dictionary<Guid, SetConfig> SetConfigs { get; set; }

        public BotConfig(Dictionary<Guid, SetConfig> configs)
        {
            SetConfigs = configs;
        }

        public BotConfig()
        {
            SetConfigs = new Dictionary<Guid, SetConfig>();
        }
    }
}