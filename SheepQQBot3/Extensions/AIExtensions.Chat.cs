using CommonLibrary;
using Masuit.Tools;
using OpenAI.Chat;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yamei.Common;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.Extensions
{
    public static partial class AIExtensions
    {
        private static readonly ChatTool _chatTool = GetTool_ChatResponse();

        private static async Task<ChatCompletion> ChatRequestAsync(
            this List<ChatMessageContentPart> thisRequestParts,
            List<ChatMessage> requestMessages,
            List<ChatMessage> loadedHistories,
            Action<string, string> botSendMessage,
            AIGroupConfig aiGroupConfig,
            bool isGroupRequest,
            string requestTargetId,
            string sendTargetId,
            string chatKey,
            bool isAt)
        {
            // HINT : 发送AI请求
            var chatCompletion = (await AIClientChat.CompleteChatAsync(
                requestMessages,
                new ChatCompletionOptions
                {
                    Tools = { _chatTool },
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
            var needSaveAIData = false;
            var dateNow = DateTime.Now;

            #region 处理关系变化

            // MEMO : 计算关系变化
            var valueChangeMessage = string.Empty;
            var relationChangeInfos = aiChatResponse.RelationChangeInfos;
            if (relationChangeInfos?.Any() == true)
            {
                relationChangeInfos
                    .Where(relationChangeInfo => !relationChangeInfo.TargetId.IsNullOrEmpty()
                        && (relationChangeInfo.IntimacyChange != 0
                            || relationChangeInfo.RespectChange != 0
                            || relationChangeInfo.AffectionChange != 0))
                    .ForEach(relationChangeInfo =>
                    {
                        needSaveAIData = true;
                        var intimacyChange = relationChangeInfo.IntimacyChange;
                        var respectChange = relationChangeInfo.RespectChange;
                        var affectionChange = relationChangeInfo.AffectionChange;
                        GlobalAIData.UserDatas.AddOrUpdate(
                            relationChangeInfo.TargetId,
                            _ =>
                            {
                                var userData = new AIUserData();
                                userData.Relation.Intimacy += intimacyChange;
                                userData.Relation.Respect += respectChange;
                                userData.Relation.Affection += affectionChange;
                                return userData;
                            },
                            (_, oldValue) =>
                            {
                                intimacyChange = CalculateRelationChange(oldValue.Relation.Intimacy, intimacyChange);
                                respectChange = CalculateRelationChange(oldValue.Relation.Respect, respectChange);
                                affectionChange = CalculateRelationChange(oldValue.Relation.Affection, affectionChange);
                                oldValue.Relation.Intimacy += intimacyChange;
                                oldValue.Relation.Respect += respectChange;
                                oldValue.Relation.Affection += affectionChange;
                                return oldValue;
                            });

                        valueChangeMessage += $"[({relationChangeInfo.TargetId})关系变化: "
                            + $"亲密{(intimacyChange > 0 ? "+" : string.Empty)}{intimacyChange}"
                            + $"/认可{(respectChange > 0 ? "+" : string.Empty)}{respectChange}"
                            + $"/好感{(affectionChange > 0 ? "+" : string.Empty)}{affectionChange}]{ENTER}";
                    });
            }

            #endregion 处理关系变化

            #region 处理用户屏蔽

            // MEMO : 计算用户屏蔽时长
            var blockUserInfos = aiChatResponse.BlockUserInfos;
            if (blockUserInfos?.Any() == true)
            {
                blockUserInfos
                    .Where(blockUserInfo => !blockUserInfo.TargetId.IsNullOrEmpty() && blockUserInfo.Value != 0)
                    .ForEach(blockUserInfo =>
                    {
                        needSaveAIData = true;
                        var changeValue = blockUserInfo.Value;
                        GlobalAIData.UserDatas.AddOrUpdate(
                            blockUserInfo.TargetId,
                            _ =>
                            {
                                var userData = new AIUserData
                                {
                                    BlockUntil = dateNow.AddMinutes(changeValue).ToTimeStamp(),
                                };
                                return userData;
                            },
                            (_, oldValue) =>
                            {
                                oldValue.BlockUntil = dateNow.AddMinutes(changeValue).ToTimeStamp();
                                return oldValue;
                            });

                        valueChangeMessage += $"[({blockUserInfo.TargetId})封禁: {changeValue} 分钟]{ENTER}";
                    });
            }

            #endregion 处理用户屏蔽

            #region 处理心情指数

            // MEMO : 计算心情指数变化
            var statusChangeInfo = aiChatResponse.StatusChangeInfo;
            if (statusChangeInfo != null)
            {
                var moodIndexChange = statusChangeInfo.MoodIndexChange;
                if (moodIndexChange != 0)
                {
                    needSaveAIData = true;
                    GlobalAIData.AIStatusData.MoodIndexValue += moodIndexChange;
                    valueChangeMessage += $"[心情指数: {(moodIndexChange > 0 ? "+" : string.Empty)}{moodIndexChange}]{ENTER}";
                }
            }

            valueChangeMessage = valueChangeMessage.RemoveEnd(ENTER);
            if (IsDebug && !valueChangeMessage.IsNullOrEmpty())
                botSendMessage(isGroupRequest ? TestGroupId : sendTargetId, $"{valueChangeMessage}");

            #endregion 处理心情指数

            // MEMO : 保存AI数据
            if (needSaveAIData)
                ConfigExtensions.SaveAIData();

            #region 处理知识和灵感笔记

            // MEMO : 保存知识笔记内容
            var knowledgeNote = aiChatResponse.KnowledgeNote;
            if (knowledgeNote != null && !knowledgeNote.Title.IsNullOrEmpty() && !knowledgeNote.Content.IsNullOrEmpty())
            {
                // MEMO : 写入知识笔记内容
                WriteAINote(knowledgeNote);
                if (IsDebug)
                    botSendMessage(isGroupRequest ? TestGroupId : sendTargetId, $"===!新知识笔记!==={ENTER}标题: {knowledgeNote.Title}{ENTER}内容: {knowledgeNote.Content}");
            }

            // MEMO : 保存灵感笔记内容
            var inspirationNote = aiChatResponse.InspirationNote;
            if (inspirationNote != null && !inspirationNote.Title.IsNullOrEmpty() && !inspirationNote.Content.IsNullOrEmpty())
            {
                // MEMO : 写入灵感笔记内容
                WriteAINote(inspirationNote);
                if (IsDebug)
                    botSendMessage(isGroupRequest ? TestGroupId : sendTargetId, $"===!新灵感笔记!==={ENTER}标题: {inspirationNote.Title}{ENTER}内容: {inspirationNote.Content}");
            }

            #endregion 处理知识和灵感笔记

            #region 处理回复消息和回复

            // MEMO : 构建回复消息
            var needAt = isAt;
            // MEMO : 排除消息为空的内容
            //chatMessages = chatMessages.Where(each => !each.Text.IsNullOrEmpty()).ToArray();
            chatMessages[^1].Delay = 0;
            // MEMO : 处理消息回复
            var aiContentParts = new List<ChatMessageContentPart>();
            chatMessages.ForEach(chatMessage =>
            {
                var sendMessage = CreateSendMessage(aiGroupConfig, chatMessage, needAt, requestTargetId, isGroupRequest);
                if (sendMessage.IsNullOrEmpty())
                    return;

                // MEMO : 只有第一句回复需要at
                needAt = false;

                aiContentParts.Add(CreateTextPart(BOT_NAME, chatMessage.Text));

                // MEMO : 发送消息
                botSendMessage(sendTargetId, $"{sendMessage}");

                // MEMO : 延迟
                var delay = chatMessage.Delay ?? 0;
                if (delay > 0)
                    CommonExtensions.Sleep(delay * 2);
            });

            #endregion 处理回复消息和回复

            #region 历史记录保存

            thisRequestParts.DeleteExpireImage();
            // MEMO : 添加本次请求内容
            loadedHistories.Add(ChatMessage.CreateUserMessage(thisRequestParts));
            // MEMO : 添加本次回复内容
            loadedHistories.Add(ChatMessage.CreateAssistantMessage(aiContentParts));
            // MEMO : 历史记录截取 (控制在上限范围内)
            loadedHistories.LimitMessages(AppSettingExtensions.Get("maxAIHistoryCount", 100));
            // MEMO : 保存历史记录
            loadedHistories.SaveAIHistory(chatKey);

            #endregion 历史记录保存

            return chatCompletion;
        }

        private static ChatTool GetTool_ChatResponse()
        {
            return ChatTool.CreateFunctionTool(
                "reply_user",
                "Call this function to send a response to the user.",
                BinaryData.FromString(JsonSchemaGenerator.Generate(typeof(AIChatResponse))));
        }

        private static AIChatResponse GetAIChatResponse(ChatCompletion chatCompletion)
        {
            if (chatCompletion.FinishReason != ChatFinishReason.ToolCalls)
                throw new AIException("[GeminiError-非预期Reason]", chatCompletion.Content[0].Text);

            var jsonText = chatCompletion.ToolCalls[0].FunctionArguments.ToString();
            // MEMO : 删除emoji
            var responseText = _regDeleteEmoji.Replace(jsonText, string.Empty);
            if (responseText.IsNullOrEmpty())
                throw new AIException("[GeminiError-返回截断]", responseText);

            var aiChatResponse = responseText.FromJson<AIChatResponse>();
            aiChatResponse.Date = DateTime.Now;
            return aiChatResponse;
        }

        private static void WriteAINote(AIKnowledgeNote knowledgeNote)
        {
            using var fs = new FileStream(AI_KNOWLEDGE_NOTE_PATH, FileMode.Append, FileAccess.Write);
            using var sw = new StreamWriter(fs, Encoding.UTF8);
            sw.Write($"## {knowledgeNote.Title}{ENTER}{ENTER}"
                + $"{knowledgeNote.Content}{ENTER}※记录日期: {DateTime.Now.ToYYYYMDHHMMSS()}{ENTER}{ENTER}");
            sw.Close();
            fs.Close();
        }

        private static void WriteAINote(AIInspirationNote inspirationNote)
        {
            using var fs = new FileStream(AI_INSPIRATION_NOTE_PATH, FileMode.Append, FileAccess.Write);
            using var sw = new StreamWriter(fs, Encoding.UTF8);
            sw.Write($"## {inspirationNote.Title}{ENTER}{ENTER}"
                + $"{inspirationNote.Content}{ENTER}※记录日期: {DateTime.Now.ToYYYYMDHHMMSS()}{ENTER}{ENTER}");
            sw.Close();
            fs.Close();
        }

        private static int CalculateRelationChange(int currentValue, int rawChange)
        {
            // 只有在增加好感，且当前是负分时触发回归逻辑
            if (currentValue < -100 && rawChange > 0)
            {
                return rawChange switch
                {
                    >= 3 => (int)(rawChange * 2.5), // 高诚意行为加速回归
                    >= 2 => (int)(rawChange * 1.5),
                    _ => rawChange,               // 敷衍行为不加速
                };
            }

            // 正常慢热逻辑 (正分区间)
            return rawChange;
        }

        private static void LimitMessages(this List<ChatMessage> messages, int maxCount)
        {
            var currentTotal = messages.Select(x => x.Content.Count).Sum();
            while (currentTotal > maxCount)
            {
                // MEMO : 移除最早的Message
                currentTotal -= messages[0].Content.Count;
                messages.RemoveAt(0);
                // MEMO : 移除AI回复
                currentTotal--;
                messages.RemoveAt(0);
            }
        }
    }
}