using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Masuit.Tools;
using OpenAI.Chat;
using SheepQQBot3.Model;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Config;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.Extensions;

public static partial class AIExtensions
{
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
                    Tools = {_groupSummaryTool},
                    ToolChoice = ChatToolChoice.CreateFunctionChoice(FUNCTION_NAME_REPLY_GROUPCHAT_SUMMARY),
#pragma warning disable OPENAI001
                    ReasoningEffortLevel = ChatReasoningEffortLevel.High,
#pragma warning restore OPENAI001
                    Temperature = GlobalAIConfig.Temperature,
                    TopP = GlobalAIConfig.TopP,
                    MaxOutputTokenCount = GlobalAIConfig.MaxToken,
                })
            .ConfigureAwait(false)).Value;

        var (aiGroupChatSummaryResponse, _) = await GetAIResponseAsync<AIGroupChatSummaryResponse>(chatCompletion).ConfigureAwait(false);
        // MEMO : 消息数
        var sendMessages = new List<GroupForwardMessage>
        {
            new(BOT_NAME, BotId, $"总结消息数: {thisRequestParts.Count - 2}"),
        };
        // MEMO : 群聊总结反馈消息
        ProcessMessage(aiGroupChatSummaryResponse.ReplyContent);
        // MEMO : 群聊总结内容
        aiGroupChatSummaryResponse.GroupChatSummarys.ForEach(groupChatSummary =>
        {
            var message = $"【{groupChatSummary.Theme}】{ENTER}{string.Join(ENTER, groupChatSummary.Contents)}";
            sendMessages.Add(new GroupForwardMessage(BOT_NAME, BotId, message));
        });
        // MEMO : 今日怪话
        var shitpostMessage = "【今日怪话】";
        aiGroupChatSummaryResponse.GroupChatShitposts.ForEach(groupChatShitpost =>
        {
            shitpostMessage += $"{ENTER}> {groupChatShitpost.Sender}: {groupChatShitpost.Content}";
        });
        sendMessages.Add(new GroupForwardMessage(BOT_NAME, BotId, shitpostMessage));
        // MEMO : 群聊颁奖
        var awardMessage = "【群聊颁奖】";
        var groupChatAward = aiGroupChatSummaryResponse.GroupChatAward;
        AddGroupChatAward(groupChatAward.Award_Huoyue, "活跃奖");
        AddGroupChatAward(groupChatAward.Award_Xiaochou, "小丑奖");
        AddGroupChatAward(groupChatAward.Award_Feiwu, "废物奖");
        AddGroupChatAward(groupChatAward.Award_Sese, "色色奖");
        sendMessages.Add(new GroupForwardMessage(BOT_NAME, BotId, awardMessage));
        // MEMO : 一句话总结
        ProcessMessage(aiGroupChatSummaryResponse.OneSentenceSummary);

        await GlobalBotClient.SendGroupForwardMessageAsync(IsDebug ? TestGroupId : sendTargetId, sendMessages,
                $"{DateTime.Now.ToYYYYMD()} 群聊总结", [$"{BOT_NICK_NAME}群聊总结", "打开查看"], $"查看{sendMessages.Count}条消息", "[今日群聊总结]")
            .ConfigureAwait(false);

        // MEMO : 群聊总结内容太多了, 不写后台Log
        // MEMO : 写入前台日志
        LogExtensions.AddRunLog(new RunLog_AIRequest(sendTargetId, true, chatCompletion.Usage));

        void ProcessMessage(AIGroupChatSummaryResponseContent chatMessage)
        {
            var sendMessage = CreateSendMessage(aiGroupConfig, chatMessage, false, requestTargetId, true);
            if (sendMessage.IsNullOrEmpty())
                return;

            sendMessages.Add(new GroupForwardMessage(BOT_NAME, BotId, sendMessage));
        }

        void AddGroupChatAward(AIGroupChatSummaryResponse_Award award, string awardName)
            => awardMessage += $"{ENTER}{awardName} - {award.AwardUser} ({award.AwardReason})";
    }

    /// <param name="contentParts"><see cref="List{ContentPart}" /></param>
    extension(List<ChatMessageContentPart> contentParts)
    {
        /// <summary>
        /// 添加消息(群聊总结专用)
        /// </summary>
        /// <param name="sender">发送者信息</param>
        /// <param name="messageText">消息内容</param>
        /// <param name="groupMembers">群友信息转换用</param>
        public void AddQQChatMessage(
            AIChatSender sender,
            string messageText,
            Dictionary<string, GroupMember> groupMembers)
        {
            if (messageText.IsNullOrEmpty())
                return;

            var message = List<ChatMessageContentPart>.ReplaceAt(groupMembers, messageText);
            var deleteImageJsonText = contentParts.AddQQChatImage(sender, message);
            if (!deleteImageJsonText.IsNullOrEmpty())
                contentParts.AddQQChatTextContent(sender, WebUtility.HtmlDecode(deleteImageJsonText));
        }

        /// <summary>
        /// 添加图片(群聊总结专用)
        /// </summary>
        /// <param name="sender">发送者信息</param>
        /// <param name="messageText">消息内容</param>
        /// <param name="imageToText">是否强制将图片转换为文字(暂不使用)</param>
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