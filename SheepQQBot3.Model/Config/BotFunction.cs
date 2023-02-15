using System.Text.Json.Serialization;
using MessagePack;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.Model.Config
{
    /// <summary>
    /// 功能配置
    /// </summary>
    [MessagePackObject]
    public class BotFunction
    {
        [JsonIgnore]
        [IgnoreMember]
        public string DisplayName => BotFunctionType.ToFunctionName();

        [Key(nameof(BotFunctionType))]
        public BotFunctionType BotFunctionType { get; }

        /// <summary>
        /// 是否使用中
        /// </summary>
        [Key(nameof(IsUsed))]
        public bool IsUsed { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public BotFunction(BotFunctionType botFunctionType, bool isUsed)
        {
            BotFunctionType = botFunctionType;
            IsUsed = isUsed;
        }
    }
}