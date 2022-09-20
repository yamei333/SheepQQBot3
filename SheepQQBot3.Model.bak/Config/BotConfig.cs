namespace SheepQQBot3.Models
{
    [Serializable]
    public class BotConfig
    {
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