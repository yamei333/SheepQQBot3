using Masuit.Tools;
using OpenRouter.NET.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static Masuit.Tools.Systems.EnumExt;

namespace SheepQQBot3.Model.AI
{
    public class AIControl
    {
        public AIConfig AIConfig { get; set; }
        public AICharacter AICharacter { get; set; }

        public ConcurrentDictionary<string, IEnumerable<ContentPart>> HistoryCaches { get; set; }

        public AIControl(AIConfig aiConfig, AICharacter aiCharacter)
        {
            CheckEmojiCodeFile(aiConfig);
            //CheckFaceCode(aiCharacter);

            AIConfig = aiConfig;
            AICharacter = aiCharacter;
            HistoryCaches = [];
        }

        //public (string, ChatSession) GetChat(List<Content> history = null)
        //{
        //    var minApiKey = AIConfig.ApiKeys.MinBy(each => each.Value).Key;
        //    var chat = AIClient[minApiKey];

        //    AIConfig.ApiKeys[minApiKey] = DateTime.Now.ToTimeStamp();
        //    chat.SystemInstruction = AICharacter.SystemInstructionText;
        //    chat.Config = new GenerationConfig
        //    {
        //        ResponseSchema = Schema.FromObject(ExampleAIChatResponse),
        //        ThinkingConfig = new ThinkingConfig
        //        {
        //            IncludeThoughts = false,
        //            ThinkingBudget = AIConfig.ThinkToken,
        //        },
        //        Temperature = AIConfig.Temperature,
        //    };
        //    var schemaProperties = chat.Config.ResponseSchema.Properties!;
        //    schemaProperties["contents"].Nullable = false;
        //    schemaProperties["contents"].Items.Properties["chatMessageInfo"].Nullable = false;
        //    schemaProperties["contents"].Items.Properties["chatMessageInfo"].Properties["text"].Nullable = false;
        //    chat.SafetySettings =
        //    [
        //        new SafetySetting
        //        {
        //            Category = HarmCategory.HARM_CATEGORY_HARASSMENT,
        //            Threshold = HarmBlockThreshold.BLOCK_NONE,
        //        },
        //        new SafetySetting
        //        {
        //            Category = HarmCategory.HARM_CATEGORY_HATE_SPEECH,
        //            Threshold = HarmBlockThreshold.BLOCK_NONE,
        //        },
        //        new SafetySetting
        //        {
        //            Category = HarmCategory.HARM_CATEGORY_SEXUALLY_EXPLICIT,
        //            Threshold = HarmBlockThreshold.BLOCK_NONE,
        //        },
        //        new SafetySetting
        //        {
        //            Category = HarmCategory.HARM_CATEGORY_DANGEROUS_CONTENT,
        //            Threshold = HarmBlockThreshold.BLOCK_NONE,
        //        },
        //    ];

        //    chat.UseGoogleSearch = true;
        //    if (history != null)
        //        chat.History = history;

        //    return (minApiKey, AIClient[minApiKey]);
        //}

        /// <summary>
        /// 检查表情文件是否缺失
        /// </summary>
        private static void CheckEmojiCodeFile(AIConfig aiConfig)
        {
            var aiFacePath = aiConfig.FacePath;
            // MEMO : 从代码开始反查
            Enum.GetNames(typeof(AIEmojiType))
                .Where(each => each != "None")
                .ForEach(codeEmojiCode =>
            {
                var fileName = GetFileName(codeEmojiCode);
                if (!File.Exists(Path.Combine(aiFacePath, fileName)))
                    throw new FileNotFoundException(fileName);
            });

            // MEMO : 从文件开始反查
            Directory.GetFiles(aiFacePath, "*.gif")
                .Select(each => new FileInfo(each).Name.Split('.')[0])
                .ForEach(emojiFileName =>
                {
                    if (!Enum.TryParse<AIEmojiType>(emojiFileName, out _))
                        throw new KeyNotFoundException(emojiFileName);
                });

            return;

            static string GetFileName(string emjCode) => $"{emjCode}.gif";
        }

        /// <summary>
        /// 检查AI面部表情定义, 程序定义
        /// </summary>
        private static void CheckFaceCode(AICharacter aiCharacter)
        {
            var responseFormat = aiCharacter.SystemInstruction["Response Format"];
            var regex = new Regex(@"(?<=## available expression values\r\n\r\n)[\s\S]+?(?=\r\n\r\n)");
            var aiFaceCodes = regex.Match(responseFormat).Value.Split("\r\n").Select(each => each.Split(':')[0]).OrderBy(each => each).ToHashSet();
            var codeFaceCodes = Enum.GetNames(typeof(AIExpressionType)).OrderBy(each => each).ToHashSet();

            var aiFaceCodeText = string.Join("\r\n", Enum.GetNames(typeof(AIExpressionType)).Select(each => $"{each}: {((AIExpressionType)Enum.Parse(typeof(AIExpressionType), each)).GetDisplay()}").ToArray());
            var aiFaceCodeText2 = string.Join(',', Enum.GetNames(typeof(AIExpressionType)).Select(each => $"\"{each}\"").ToArray());

            // MEMO : AI代码查文件和代码定义
            aiFaceCodes.ForEach(aiFaceCode =>
            {
                if (!codeFaceCodes.Contains(aiFaceCode))
                    throw new KeyNotFoundException(aiFaceCode);
            });

            // MEMO : 从代码开始反查
            codeFaceCodes
                .Where(each => each != "None")
                .ForEach(codeFaceCode =>
            {
                if (!aiFaceCodes.Contains(codeFaceCode))
                    throw new KeyNotFoundException(codeFaceCode);
            });

            return;
        }
    }
}