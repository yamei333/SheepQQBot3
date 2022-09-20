using Newtonsoft.Json;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// 功能配置
    /// </summary>
    [Serializable]
    public class BotFunction
    {
        [JsonIgnore]
        public string DisplayName => BotFunctionType.ToFunctionName();

        public BotFunctionType BotFunctionType { get; }

        /// <summary>
        /// 是否使用中
        /// </summary>
        public bool? IsUsed { get; set; }

        public BotFunction(BotFunctionType botFunctionType, bool isUsed)
        {
            BotFunctionType = botFunctionType;
            IsUsed = isUsed;
        }
    }
}