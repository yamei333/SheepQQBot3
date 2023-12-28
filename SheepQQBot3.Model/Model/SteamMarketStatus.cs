using System;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Model
{
    [Serializable]
    public class SteamMarketStatus
    {
        /// <summary>
        /// 验证用字段
        /// </summary>
        [JsonPropertyName("sheepqqbot3")]
        public string SheepQQBot3 { get; set; }
    }
}