using Masuit.Tools;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    public class AICharacter
    {
        /// <summary>
        /// 系统说明(AI提示)
        /// </summary>
        [JsonPropertyName("systemInstruction")]
        public Dictionary<string, string> SystemInstruction { get; set; }

        /// <summary>
        /// 取得系统说明的单段文本
        /// </summary>
        [JsonIgnore]
        public string SystemInstructionText
        {
            get
            {
                var systemInstruction = new StringBuilder();
                SystemInstruction.ForEach(info =>
                {
                    systemInstruction.AppendLine($"# {info.Key}");
                    systemInstruction.AppendLine(info.Value);
                    systemInstruction.AppendLine();
                });

                return systemInstruction.ToString();
            }
        }
    }
}