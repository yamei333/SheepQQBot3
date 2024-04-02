using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.Model.ChatSummaryConfig
{
    public class ChatSummaryGroupConfig
    {
        /// <summary>
        /// ExcludeWords
        /// </summary>
        [JsonPropertyName("ExcludeWords")]
        public HashSet<string> ExcludeWords { get; set; }
    }
}