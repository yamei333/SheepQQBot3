using GenerativeAI;
using GenerativeAI.Types;
using Masuit.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Yamei.Common;
using static Masuit.Tools.Systems.EnumExt;

namespace SheepQQBot3.Model.AI
{
    public class AIControl
    {
        public AIConfig AIConfig { get; set; }
        public AICharacter AICharacter { get; set; }

        public ConcurrentDictionary<string, ChatSession> AIChatSessions { get; set; }

        public ConcurrentDictionary<string, IEnumerable<Part>> HistoryCaches { get; set; }

        public static AIChatResponse ExampleAIChatResponse = new()
        {
            Date = "2025-8-1 10:11:12",
            Contents = [
                new AIChatResponseContent
                {
                    Think = "对方向我友好问好，我也应该友好回应",
                    Body = "挥挥手",
                    Mind = "我跟他不认识，只要平常心就好",
                    Face = "Happy",
                    ChatMessageInfo = new AIChatMessage
                    {
                        Emoji = "eihei",
                        Text = "你好呀",
                        Delay = 1000,
                    },
                },
                new AIChatResponseContent
                {
                    Think = "作为小助手应该询问他需要什么帮助",
                    Mind = "先问问他他需要什么帮助",
                    Face = "Happy",
                    ChatMessageInfo = new AIChatMessage
                    {
                        Emoji = "nihao",
                        Text = "请问你需要什么帮助",
                        Delay = 500,
                    },
                },
            ],
            FavorabilityChangeInfos =
            [
                new AIFavorabilityChangeInfo
                {
                    TargetId = 252961222,
                    Value = 1,
                },
            ],
            StatusChangeInfo = new AIStatusChangeInfo
            {
                MoodIndexChange = 1,
            },
        };

        public AIControl(AIConfig aiConfig, AICharacter aiCharacter)
        {
            CheckEmojiCodeFile(aiConfig, aiCharacter);
            CheckFaceCode(aiCharacter);

            AIConfig = aiConfig;
            AICharacter = aiCharacter;
            var model = aiConfig.Model;
            AIChatSessions = [];
            aiConfig.ApiKeys.ForEach(each =>
            {
                var key = each.Key;
                var ai = new GoogleAi(key);
                var geminiModel = ai.CreateGeminiModel(model);
                AIChatSessions.GetOrAdd(key, geminiModel.StartChat());
            });

            HistoryCaches = [];
        }

        public (string, ChatSession) GetChat(List<Content> history = null)
        {
            var minApiKey = AIConfig.ApiKeys.MinBy(each => each.Value).Key;
            var chat = AIChatSessions[minApiKey];

            AIConfig.ApiKeys[minApiKey] = DateTime.Now.ToTimeStamp();
            chat.SystemInstruction = AICharacter.SystemInstructionText;
            chat.Config = new GenerationConfig
            {
                ResponseSchema = Schema.FromObject(ExampleAIChatResponse),
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = false,
                    ThinkingBudget = AIConfig.ThinkToken,
                },
                Temperature = AIConfig.Temperature,
            };
            var schemaProperties = chat.Config.ResponseSchema.Properties!;
            schemaProperties["date"].Nullable = false;
            schemaProperties["contents"].Nullable = false;
            schemaProperties["contents"].Items.Properties["chatMessageInfo"].Nullable = false;
            schemaProperties["contents"].Items.Properties["chatMessageInfo"].Properties["text"].Nullable = false;
            chat.SafetySettings =
            [
                new SafetySetting
                {
                    Category = HarmCategory.HARM_CATEGORY_HARASSMENT,
                    Threshold = HarmBlockThreshold.BLOCK_NONE,
                },
                new SafetySetting
                {
                    Category = HarmCategory.HARM_CATEGORY_HATE_SPEECH,
                    Threshold = HarmBlockThreshold.BLOCK_NONE,
                },
                new SafetySetting
                {
                    Category = HarmCategory.HARM_CATEGORY_SEXUALLY_EXPLICIT,
                    Threshold = HarmBlockThreshold.BLOCK_NONE,
                },
                new SafetySetting
                {
                    Category = HarmCategory.HARM_CATEGORY_DANGEROUS_CONTENT,
                    Threshold = HarmBlockThreshold.BLOCK_NONE,
                },
            ];

            chat.UseGoogleSearch = true;
            if (history != null)
                chat.History = history;

            return (minApiKey, AIChatSessions[minApiKey]);
        }

        /// <summary>
        /// 检查表情文件, 程序定义, AI介绍表情代码, 3者是否匹配
        /// </summary>
        private void CheckEmojiCodeFile(AIConfig aiConfig, AICharacter aiCharacter)
        {
            var responseFormat = aiCharacter.SystemInstruction["Response Format"];
            const string START_TEXT = "## available emoji values\r\n\r\n";
            var aiEmojiCodeEnumText = responseFormat[(responseFormat.IndexOf(START_TEXT, StringComparison.CurrentCulture) + START_TEXT.Length)..];
            var aiEmojiCodeEnums = aiEmojiCodeEnumText.Split("\r\n").Select(each => each.Split(':')[0]).OrderBy(each => each).ToHashSet();
            var codeEmojiCodeEnums = Enum.GetNames(typeof(AIEmojiType)).OrderBy(each => each).ToHashSet();
            var aiFacePath = aiConfig.FacePath;

            // MEMO : AI代码查文件和代码定义
            aiEmojiCodeEnums
                .Where(each => each != "None")
                .ForEach(aiEmojiCode =>
            {
                var fileName = GetFileName(aiEmojiCode);
                if (!File.Exists(Path.Combine(aiFacePath, fileName)))
                    throw new FileNotFoundException(fileName);

                if (!codeEmojiCodeEnums.Contains(aiEmojiCode))
                    throw new KeyNotFoundException(aiEmojiCode);
            });

            // MEMO : 从代码开始反查
            codeEmojiCodeEnums
                .Where(each => each != "None")
                .ForEach(codeEmojiCode =>
            {
                var fileName = GetFileName(codeEmojiCode);
                if (!File.Exists(Path.Combine(aiFacePath, fileName)))
                    throw new FileNotFoundException(fileName);

                if (!aiEmojiCodeEnums.Contains(codeEmojiCode))
                    throw new KeyNotFoundException(codeEmojiCode);
            });

            // MEMO : 从文件开始反查
            Directory.GetFiles(aiFacePath, "*.png")
                .Select(each => new FileInfo(each).Name.Split('.')[0])
                .ForEach(emojiFileName =>
                {
                    if (!aiEmojiCodeEnums.Contains(emojiFileName))
                        throw new KeyNotFoundException(emojiFileName);

                    if (!codeEmojiCodeEnums.Contains(emojiFileName))
                        throw new KeyNotFoundException(emojiFileName);
                });

            return;

            string GetFileName(string emjCode) => $"{emjCode}.png";
        }

        /// <summary>
        /// 检查AI面部表情定义, 程序定义
        /// </summary>
        private void CheckFaceCode(AICharacter aiCharacter)
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