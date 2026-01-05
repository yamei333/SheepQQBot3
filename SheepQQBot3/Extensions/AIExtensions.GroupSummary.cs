using Masuit.Tools;
using OpenAI.Chat;
using SheepQQBot3.Model;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.Extensions
{
    public static partial class AIExtensions
    {
        private static readonly ChatTool _groupSummaryTool = ChatTool.CreateFunctionTool(
            "reply_user",
            "Call this function to send a response to the user.",
            BinaryData.FromString(JsonSchemaGenerator.Generate(typeof(AIGroupChatSummaryResponse))));

        private static async Task GroupSummaryRequestAsync(
            this List<ChatMessageContentPart> thisRequestParts,
            List<ChatMessage> requestMessages,
            AIGroupConfig aiGroupConfig,
            string requestTargetId,
            string sendTargetId)
        {
            var chatCompletion = (await AIClientSummary.CompleteChatAsync(
                requestMessages,
                new ChatCompletionOptions
                {
                    Tools = { _groupSummaryTool },
                    ToolChoice = ChatToolChoice.CreateFunctionChoice("reply_user"),
#pragma warning disable OPENAI001
                    ReasoningEffortLevel = ChatReasoningEffortLevel.High,
#pragma warning restore OPENAI001
                    Temperature = GlobalAIConfig.Temperature,
                    TopP = GlobalAIConfig.TopP,
                    MaxOutputTokenCount = GlobalAIConfig.MaxToken,
                })
                .ConfigureAwait(false)).Value;

            var aiChatResponse = GetAIChatResponse(chatCompletion);
            var chatMessages = aiChatResponse.Contents;
            var sendMessages = new List<GroupForwardMessage>
            {
                new(BOT_NAME, BotId, $"总结消息数: {thisRequestParts.Count - 2}"),
            };
            chatMessages.ForEach(content =>
            {
                var sendMessage = CreateSendMessage(aiGroupConfig, content, false, requestTargetId, true);
                if (sendMessage.IsNullOrEmpty())
                    return;

                sendMessages.Add(new GroupForwardMessage(BOT_NAME, BotId, sendMessage));
            });

            await GlobalBotClient.SendGroupForwardMessageAsync(IsDebug ? TestGroupId : sendTargetId, sendMessages,
                    $"{DateTime.Now.ToYYYYMD()} 群聊总结", [$"{BOT_NICK_NAME}群聊总结", "打开查看"], $"查看{sendMessages.Count}条消息", "[今日群聊总结]")
                .ConfigureAwait(false);

            // MEMO : 群聊总结内容太多了, 不写后台Log
            // MEMO : 写入前台日志
            LogExtensions.AddRunLog(new RunLog_AIRequest(sendTargetId, true, chatCompletion.Usage));
        }

        /// <param name="contentParts"><see cref="List{ContentPart}"/></param>
        extension(List<ChatMessageContentPart> contentParts)
        {
            // (群聊总结专用)
            public void AddQQChatMessage(
                AIChatSender sender,
                string messageText,
                Dictionary<string, GroupMember> groupMembers)
            {
                if (messageText.IsNullOrEmpty())
                    return;

                var message = ReplaceAt(groupMembers, messageText);
                var deleteImageJsonText = contentParts.AddQQChatImage(sender, message);
                if (!deleteImageJsonText.IsNullOrEmpty())
                    contentParts.AddQQChatTextContent(sender, WebUtility.HtmlDecode(deleteImageJsonText));
            }

            /// <summary>
            /// 添加图片(群聊总结专用)
            /// </summary>
            /// <param name="sender">发送者信息</param>
            /// <param name="messageText">消息内容</param>
            /// <param name="imageToText"></param>
            /// <returns>删除图片后的消息</returns>
            private string AddQQChatImage(AIChatSender sender, string messageText, bool imageToText = false)
            {
                if (messageText.IsNullOrEmpty())
                    return string.Empty;

                var processedMessage = messageText;
                var matches = _regCQImageFileUrl.Matches(messageText);
                var thisContentParts = new List<ChatMessageContentPart>();
                matches.ForEach(match =>
                {
                    thisContentParts.AddQQChatTextContent(sender, IMAGE_EXPIRED);
                    processedMessage = processedMessage.Replace(match.Value, string.Empty);
                });

                if (thisContentParts.Any())
                    contentParts.AddRange(thisContentParts);

                return processedMessage;
            }
        }
    }
}