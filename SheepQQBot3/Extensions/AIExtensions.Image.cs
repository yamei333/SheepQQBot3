using System;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonLibrary;
using Masuit.Tools;
using OpenAI.Chat;
using SheepQQBot3.Enums;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Extension;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.Extensions;

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
        var result = await AIClientImage.CompleteChatAsync(
                requestMessages,
                new ChatCompletionOptions
                {
                    // Tools = {_chatTool},
                    // ToolChoice = ChatToolChoice.CreateFunctionChoice(FUNCTION_NAME_REPLY_USER),
#pragma warning disable OPENAI001
                    ReasoningEffortLevel = ChatReasoningEffortLevel.High,
#pragma warning restore OPENAI001
                    Temperature = GlobalAIConfig.Temperature,
                    TopP = GlobalAIConfig.TopP,
                    MaxOutputTokenCount = GlobalAIConfig.MaxToken,
                })
            .ConfigureAwait(false);
        var imageCompletion = result.Value;
        if (imageCompletion.FinishReason != ChatFinishReason.Stop)
        {
            YameiLogExtensions.WriteJsonDeserializeLog(
                new JsonException(ERROR_JSON_ERROR),
                nameof(AIChatResponse),
                "[GeminiError-非预期Reason]生成图片返回非Stop");
            YameiLogExtensions.WriteLog(LogType.Error, "[GeminiError-非预期Reason]生成图片返回非Stop");
#if DEBUG
            botSendMessage(isGroupRequest ? TestGroupId : sendTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat("生成图片返回非Stop")}");
#endif
            return;
        }

        var sendMessage = string.Empty;
        TrySaveGeneratedImage(result.GetRawResponse());
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

    /// <summary>
    /// 从 PipelineResponse 中直接提取 choices[0].message.images[0].image_url.url 并保存为图片
    /// </summary>
    public static bool TrySaveGeneratedImage(PipelineResponse rawResponse, string outputFileName = "today_topic.jpg")
    {
        try
        {
            // 在只读内存切片上流式解析
            using var doc = JsonDocument.Parse(rawResponse.Content.ToMemory());
            var root = doc.RootElement;

            // choices[0] -> message -> images[0] -> image_url -> url
            if (!root.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
                return false;

            var message = choices[0].GetProperty("message");
            if (!message.TryGetProperty("images", out var images) || images.GetArrayLength() == 0)
                return false;

            var imageItem = images[0];
            if (!imageItem.TryGetProperty("image_url", out var imageUrlObj))
                return false;

            if (!imageUrlObj.TryGetProperty("url", out var urlProp))
                return false;

            var dataUri = urlProp.GetString();
            if (string.IsNullOrEmpty(dataUri))
                return false;

            // 拿Base64
            var commaIndex = dataUri.IndexOf(',');
            var base64Data = commaIndex >= 0 ? dataUri.Substring(commaIndex + 1) : dataUri;

            // 写入
            var imageBytes = Convert.FromBase64String(base64Data);
            File.WriteAllBytes(outputFileName, imageBytes);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"提取图片失败: {ex.Message}");
            return false;
        }
    }
}