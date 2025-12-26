using CommonLibrary;
using Masuit.Tools;
using OpenAI.Chat;
using SheepQQBot3.Enums;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.Extensions
{
    public static partial class AIExtensions
    {
        private static async Task ImageRequestAsync(
            this List<ChatMessageContentPart> thisRequestParts,
            List<ChatMessage> requestMessages,
            List<ChatMessage> loadedHistories,
            Action<string, string> botSendMessage,
            bool isGroupRequest,
            string requestTargetId,
            string sendTargetId,
            string chatKey,
            bool isAt)
        {
            var imageCompletion = (await AIClientImage.CompleteChatAsync(
                requestMessages,
                new ChatCompletionOptions
                {
                    ToolChoice = ChatToolChoice.CreateFunctionChoice("reply_user"),
#pragma warning disable OPENAI001
                    ReasoningEffortLevel = ChatReasoningEffortLevel.High,
#pragma warning restore OPENAI001
                    Temperature = GlobalAIConfig.Temperature,
                    TopP = GlobalAIConfig.TopP,
                    MaxOutputTokenCount = GlobalAIConfig.MaxToken,
                })
                .ConfigureAwait(false)).Value;

            if (imageCompletion.FinishReason != ChatFinishReason.Stop)
            {
                YameiLogExtensions.WriteJsonDeserializeLog(
                    new JsonException(ERROR_JSON_ERROR),
                    nameof(AIChatResponse),
                    $"[GeminiError-非预期Reason]生成图片返回非Stop");
                YameiLogExtensions.WriteLog(LogType.Error, "[GeminiError-非预期Reason]生成图片返回非Stop");
#if DEBUG
                botSendMessage(isGroupRequest ? TestGroupId : sendTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat("生成图片返回非Stop")}");
#endif
                return;
            }

            var sendMessage = string.Empty;
            var imageUrls = ExtractImageUrls(imageCompletion.Content[0].Text);
            await imageUrls.ForeachAsync(async imageUrl =>
            {
                var (getSuccessed, fileName) = await HttpExtensions.HttpDownloadAsync(imageUrl, "Cache").ConfigureAwait(false);
                if (getSuccessed)
                    sendMessage += CQCode.Image(CommonExtensions.GetPath("Cache", fileName, GetPathType.CQCodePath));
            }).ConfigureAwait(false);

            if (!sendMessage.IsNullOrEmpty())
            {
                // MEMO : 正常生成图片并发送
                botSendMessage(sendTargetId, $"{sendMessage}{(isAt ? $"{CQCode.At(requestTargetId)} " : string.Empty)}你要的图片来了!");

                // MEMO : 保存历史记录(图片内容应该不多, 可以忽略最大限制)
                // MEMO : 删除过期图片信息
                thisRequestParts.DeleteExpireImage();
                // MEMO : 添加本次请求内容
                loadedHistories.Add(ChatMessage.CreateUserMessage(thisRequestParts));
                // MEMO : 添加本次回复内容
                loadedHistories.Add(ChatMessage.CreateAssistantMessage(
                    CreateTextPart(BOT_NAME, $"{IMAGE_EXPIRED}你要的图片来了!")));
                // MEMO : 保存历史记录
                loadedHistories.SaveAIHistory(chatKey);
            }
            else
            {
                botSendMessage(sendTargetId, "返回消息中不存在图片地址!");
            }
        }

        /// <summary>
        /// 从 Markdown 格式的字符串中提取所有图片 URL
        /// </summary>
        /// <param name="input">输入的包含 ![image](url) 的字符串</param>
        /// <returns>匹配到的 URL 列表</returns>
        public static List<string> ExtractImageUrls(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return [];

            // 正则表达式逻辑：匹配 ![描述](URL) 结构
            var pattern = @"!\[.*?\]\((?<url>.*?)\)";
            var matches = Regex.Matches(input, pattern, RegexOptions.IgnoreCase);

            // 使用 LINQ 提取所有名为 "url" 的捕获组内容
            return matches
                .Select(m => m.Groups["url"].Value)
                .ToList();
        }
    }
}