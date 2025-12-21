using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    public class AIStopResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("parameters")]
        public AIChatResponse ChatResponse { get; set; }
    }
}