using CommonLibrary;
using Masuit.Tools;
using Masuit.Tools.Systems;
using OpenRouter.NET;
using OpenRouter.NET.Models;
using SheepQQBot3.Enums;
using SheepQQBot3.Model;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yamei.Common;
using static SheepQQBot3.PublicVar;
using Message = OpenRouter.NET.Models.Message;

namespace SheepQQBot3.Extensions;

public static class AIExtensions
{
    public const string ERROR_MESSAGE = "我脑袋好像短路了！";
    public const string ERROR_REASON = "(原因: {0})";
    public const string ERROR_JSON_ERROR = "Json解析失败";
    public const string ERROR_JSON_BLOCK = "返回截断";
    public const string ERROR_JSON_EMPTY = "返回空内容";
    public const string AI_HISTORY_PATH = "AICache/History/";
    //private const string AI_IMAGE_PATH = "AICache/Image/";
    private const string SENDING_GEMINI_REQUEST = "正在发送哈基米请求";

    public const string MESSAGE_BUSY = "我...现在正忙!...不太方便!...";
    public const string MESSAGE_SLEEP = "Zzz...";
    public const string SEND_SOME_IMAGES = "(发送了图片)";

    private const string AI_KNOWLEDGE_PATH = "AICache/knowledge.txt";
    private const string AI_KNOWLEDGE_NOTE_PATH = "AICache/knowledgeNote.txt";
    private const string AI_INSPIRATION_NOTE_PATH = "AICache/inspirationNote.txt";

    private static readonly Tool _tool = GetTool_Response();
    private static readonly Regex _regReplaceAt = new(@"\[CQ:at,qq=(?<qqId>\d+)\] ", RegexOptions.IgnoreCase);
    private static readonly Regex _regCQImageFileUrl = RegexGenerator.CQImageFileUrl();
    //private static readonly Regex _regDeleteMarkdown = new(@"(?<=```json)[\s\S.]+(?=```)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    //private static readonly Regex _regDeleteErrorEmoji = new(@"\[emoji:(?<emojiCode>.+?)\]|\[\[emoji:(?<emojiCode>.+?)\]\]|\[\[\[emoji:(?<emojiCode>.+?)\]\]\]", RegexOptions.IgnoreCase);
    //private static readonly Regex _regDeleteErrorEmoji2 = new(@"\[.+?\]|\[\[.+?\]\]|\[\[\[.+?\]\]\]");
    //private static readonly Regex _regDeleteErrorEmoji3 = new(@"\(.+?\)|\(\(.+?\)\)|\(\(.+?\)\)");
    private static readonly Regex _regDeleteEmoji = new(@"\p{Cs}");

    //private static readonly Regex _reg3LevelJson = new(@"\{([^{}]|\{([^{}]|\{[^{}]*\})*\})*\}");
    //private static readonly Regex _regInjectHurry = new("哈.{0,5}莉");

    public static async Task SendAsync(
        this List<ContentPart> thisRequestContentParts,
        string chatKey,
        string requestTargetId,
        string groupId,
        bool isAt,
        ConcurrentDictionary<string, AIChatSender> aiChatSenderInfos,
        AIGroupConfig aiGroupConfig,
        Action<string, string> botSendMessage,
        AIModel model,
        string extraSystemHint = null,
        bool saveHistory = true)
    {
        var retryTimes = 0;
        var isGroupRequest = !groupId.IsNullOrEmpty();
        var isGroupMemberRequest = isGroupRequest && requestTargetId != groupId;
        var sendTargetId = isGroupRequest ? groupId : requestTargetId;
        var thisRequestMessageSendVer = new List<Message>();
        var loadedHistories = new List<Message>();
        var aiStatus = new AIStatusInfo();
        var isGroupChatSummary = chatKey.StartsWith("z");
        var responseText = string.Empty;

        #region 预处理用户好感度等信息(不存在则追加)

        // MEMO : 替换信息
        var requestUserInfos = new ConcurrentDictionary<string, AIUserInfo>();
        for (var i = 0; i < thisRequestContentParts.Count; i++)
        {
            var contentPart = thisRequestContentParts[i];
            if (contentPart is TextContent textContent)
            {
                var aiChatRequest = textContent.Text.FromJson<AIChatRequest>();
                var qqId = aiChatRequest.SenderId;
                // MEMO : 清空SenderId
                aiChatRequest.SenderId = null;
                thisRequestContentParts[i] = new TextContent(aiChatRequest.ToJsonIgnoreNull());
                if (qqId != AISystemId)
                {
                    var aiChatSender = aiChatSenderInfos.GetOrAdd(qqId, new AIChatSender
                    {
                        QQ = qqId,
                        NickName = "unknown",
                    });
                    var userData = GlobalAIData.UserDatas.GetOrAdd(qqId, _ => new AIUserData
                    {
                        Relation = aiChatSender.GetRelation(),
                        BlockUntil = 0,
                        AllowedActs = aiChatSender.GetAllowedActs(),
                    });
                    requestUserInfos.GetOrAdd(qqId,
                        AIUserInfoDictionary.GetOrAdd(qqId, new AIUserInfo
                        {
                            UserInfo = aiChatSender,
                            // MEMO : 群聊总结不携带 UserOtherInfo 信息, 减少Token
                            UserOtherInfo = isGroupChatSummary
                                ? null
                                : new AIUserOtherInfo
                                {
                                    Relation = new AIRelation
                                    {
                                        Intimacy = userData.Relation.Intimacy.ToIntimacyText(),
                                        Respect = userData.Relation.Respect.ToRespectText(),
                                        Affection = userData.Relation.Affection.ToAffectionText(),
                                    },
                                },
                        }));
                }
            }
        }

        #endregion 预处理用户好感度等信息(不存在则追加)

        #region 构建系统信息

        // MEMO : 系统信息
        var systemMessage = Message.FromSystem(string.Empty);
        // MEMO : 角色设计
        var systemMessageContents = new List<ContentPart> { new TextContent(GlobalAICharacter.SystemInstructionText) };
        // MEMO : 知识库
        systemMessageContents.AddKnowledge();
        // MEMO : 助手哈莉状态
        aiStatus = await systemMessageContents.AddStatusAsync(
                isGroupRequest ? AIMessageSourceType.Group : AIMessageSourceType.Private,
                sendTargetId)
            .ConfigureAwait(false);
        // MEMO : 本次请求的用户信息
        if (requestUserInfos.Any())
            systemMessageContents.Add(new TextContent(requestUserInfos.Values.ToJsonIgnoreNull()));
        systemMessage.Content = systemMessageContents;

        #endregion 构建系统信息

        #region 构建本次发送信息

        thisRequestMessageSendVer = [systemMessage];
        // MEMO : 历史记录
        loadedHistories = LoadAIHistory(chatKey);
        thisRequestMessageSendVer.AddRange(loadedHistories);
        thisRequestMessageSendVer.Add(Message.FromUser(thisRequestContentParts));
        // MEMO : 系统提示
        if (!extraSystemHint.IsNullOrEmpty())
            thisRequestMessageSendVer.Add(Message.FromUser(extraSystemHint));

        #endregion 构建本次发送信息

        #region DEBUG响应, 正在发送哈基米请求

        if (IsDebug)
        {
            if (!isGroupRequest)
            {
                botSendMessage(sendTargetId, $"{SENDING_GEMINI_REQUEST}...");
            }
            else
            {
                botSendMessage(TestGroupId, isGroupMemberRequest
                    ? $"{SENDING_GEMINI_REQUEST}(群:{groupId}/群友:{requestTargetId})..."
                    : $"{SENDING_GEMINI_REQUEST}(群:{groupId})...");
            }
        }

        #endregion DEBUG响应, 正在发送哈基米请求

        while (retryTimes <= AI_MAX_RETRY_TIMES)
        {
            try
            {
                if (IsDebug && retryTimes > 0)
                    botSendMessage(TestGroupId, $"重新发送AI请求中...{retryTimes}");

                #region 发送AI请求

                // HINT : 发送AI请求
                var response = await (model.SupportImage ? AIClientImage : AIClient)
                    .CreateChatCompletionAsync(new ChatCompletionRequest
                    {
                        Model = model.Model,
                        Messages = thisRequestMessageSendVer,
                        Tools = [_tool],
                        ToolChoice = new
                        {
                            type = "function",
                            function = new { name = "reply_user" },
                        },
                        Reasoning = new ReasoningConfig
                        {
                            Effort = "high",
                            Enabled = true,
                            Exclude = true,
                        },
                        Temperature = GlobalAIConfig.Temperature,
                    }).ConfigureAwait(false);

                #endregion 发送AI请求

                #region Error处理 - 无返回内容

                if (response.Choices == null)
                {
                    // MEMO : 无返回内容
                    retryTimes++;
                    YameiLogExtensions.WriteJsonDeserializeLog(
                        new JsonException(ERROR_JSON_BLOCK),
                        nameof(AIChatResponse),
                        $"[GeminiError]无返回内容");
                    YameiLogExtensions.WriteLog(LogType.Error, "[GeminiError]Gemini无返回内容");
                    if (IsDebug)
                        botSendMessage(isGroupRequest ? TestGroupId : requestTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ERROR_JSON_EMPTY)}");

                    continue;
                }

                #endregion Error处理 - 无返回内容

                #region JsonText提取

                var responseChoice = response.Choices[0];
                var finishReason = responseChoice.FinishReason;
                var jsonText = finishReason == "stop"
                    ? responseChoice.Message.Content.ToString()
                    : responseChoice.Message.ToolCalls[0].Function.Arguments;

                #region 图片模型特殊处理

                if (model.SupportImage)
                {
                    if (finishReason == "stop")
                    {
                        var sendMessage = string.Empty;
                        var imageUrls = ExtractImageUrls(jsonText);
                        await imageUrls.ForeachAsync(async imageUrl =>
                        {
                            var (getSuccessed, fileName) = await HttpExtensions.HttpDownloadAsync(
                                imageUrl, "Cache", true).ConfigureAwait(false);
                            if (getSuccessed)
                                sendMessage += CQCode.Image(CommonExtensions.GetPath("Cache", fileName, GetPathType.CQCodePath));
                        }).ConfigureAwait(false);

                        if (!sendMessage.IsNullOrEmpty())
                        {
                            // MEMO : 正常生成图片并发送
                            botSendMessage(sendTargetId, $"{sendMessage}{(isAt ? $"{CQCode.At(requestTargetId)} " : string.Empty)}你要的图片来了!");

                            // MEMO : 删除过期图片信息
                            //DeleteExpireImage();
                            // MEMO : 添加本次请求内容
                            loadedHistories.Add(Message.FromUser(thisRequestContentParts));
                            // MEMO : 添加本次回复内容
                            loadedHistories.Add(Message.FromAssistant("[图片已过期]你要的图片来了!"));
                            // MEMO : 保存历史记录
                            if (saveHistory)
                                loadedHistories.SaveAIHistory(chatKey);
                        }
                        else
                        {
                            botSendMessage(sendTargetId, "返回消息中不存在图片地址!");
                        }
                    }
                    else
                    {
                        #region Error处理 - 图片生成返回非Stop

                        // MEMO : 生成图片返回非Stop
                        retryTimes++;
                        YameiLogExtensions.WriteJsonDeserializeLog(
                            new JsonException(ERROR_JSON_ERROR),
                            nameof(AIChatResponse),
                            $"[GeminiError]生成图片返回非Stop");
                        YameiLogExtensions.WriteLog(LogType.Error, "[GeminiError]生成图片返回非Stop");
                        if (IsDebug)
                            botSendMessage(isGroupRequest ? TestGroupId : requestTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat("生成图片返回非Stop")}");

                        continue;

                        #endregion Error处理 - 图片生成返回非Stop
                    }

                    WriteLog();
                    return;
                }

                #endregion 图片模型特殊处理

                #endregion JsonText提取

                #region 删除返回消息中的emoji字符

                // MEMO : 删除emoji
                responseText = _regDeleteEmoji.Replace(jsonText, string.Empty);
                if (responseText.IsNullOrEmpty())
                {
                    retryTimes++;
                    YameiLogExtensions.WriteLog(LogType.Error, "[GeminiError]Gemini返回截断");
                    if (IsDebug)
                        botSendMessage(isGroupRequest ? TestGroupId : requestTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ERROR_JSON_BLOCK)}");

                    continue;
                }

                #endregion 删除返回消息中的emoji字符

                #region ChatMessages获取

                var aiChatResponse = finishReason == "stop"
                    ? responseText.StartsWith('[')
                        ? responseText.FromJson<AIStopResponse[]>().First().ChatResponse
                        : responseText.FromJson<AIChatResponse>()
                    : responseText.FromJson<AIChatResponse>();
                var dateNow = DateTime.Now;
                aiChatResponse.Date = dateNow;
                var chatMessages = aiChatResponse.Contents;

                #endregion ChatMessages获取

                #region 群聊总结处理

                // MEMO : 群聊总结提前处理, 采用转发形式发送
                if (isGroupChatSummary)
                {
                    var sendMessages = new List<GroupForwardMessage>
                {
                    new(BOT_NAME, BotId, $"总结消息数: {thisRequestContentParts.Count - 2}"),
                };
                    chatMessages.ForEach(aiChatResponseContent =>
                    {
                        var sendMessage = CreateSendMessage(aiGroupConfig, aiChatResponseContent, false, requestTargetId, isGroupRequest);
                        if (sendMessage.IsNullOrEmpty())
                            return;

                        sendMessages.Add(new GroupForwardMessage(BOT_NAME, BotId, sendMessage));
                    });

                    await GlobalBotClient.SendGroupForwardMessageAsync(IsDebug ? TestGroupId : groupId, sendMessages,
                            $"{dateNow.ToYYYYMD()} 群聊总结", [$"{BOT_NICK_NAME}群聊总结", "打开查看"], $"查看{sendMessages.Count}条消息", "[今日群聊总结]")
                        .ConfigureAwait(false);
                    WriteLog();
                    return;
                }

                #endregion 群聊总结处理

                var needSaveAIData = false;

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
                        PublicVar.GlobalAIData.AIStatusData.MoodIndexValue += moodIndexChange;

                        valueChangeMessage += $"[心情指数: {(moodIndexChange > 0 ? "+" : string.Empty)}{moodIndexChange}]{ENTER}";
                    }
                }

                valueChangeMessage = valueChangeMessage.RemoveEnd(ENTER);
                if (IsDebug && !valueChangeMessage.IsNullOrEmpty())
                    botSendMessage(isGroupRequest ? TestGroupId : requestTargetId, $"{valueChangeMessage}");

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
                        botSendMessage(isGroupRequest ? TestGroupId : requestTargetId, $"===!新知识笔记!==={ENTER}标题: {knowledgeNote.Title}{ENTER}内容: {knowledgeNote.Content}");
                }

                // MEMO : 保存灵感笔记内容
                var inspirationNote = aiChatResponse.InspirationNote;
                if (inspirationNote != null && !inspirationNote.Title.IsNullOrEmpty() && !inspirationNote.Content.IsNullOrEmpty())
                {
                    // MEMO : 写入灵感笔记内容
                    WriteAINote(inspirationNote);
                    if (IsDebug)
                        botSendMessage(isGroupRequest ? TestGroupId : requestTargetId, $"===!新灵感笔记!==={ENTER}标题: {inspirationNote.Title}{ENTER}内容: {inspirationNote.Content}");
                }

                #endregion 处理知识和灵感笔记

                #region 处理回复消息和回复

                // MEMO : 构建回复消息
                var needAt = isAt;
                // MEMO : 排除消息为空的内容
                chatMessages = chatMessages.Where(each => each.ChatMessageInfo != null).ToArray();
                chatMessages[^1].ChatMessageInfo.Delay = 0;
                // MEMO : 处理消息回复
                chatMessages.ForEach(aiChatResponseContent =>
                {
                    var sendMessage = CreateSendMessage(aiGroupConfig, aiChatResponseContent, needAt, requestTargetId, isGroupRequest);
                    if (sendMessage.IsNullOrEmpty())
                        return;

                    // MEMO : 只有第一句回复需要at
                    needAt = false;

                    // MEMO : 发送消息
                    botSendMessage(sendTargetId, $"{sendMessage}");

                    // MEMO : 延迟
                    var delay = aiChatResponseContent.ChatMessageInfo.Delay ?? 0;
                    if (delay > 0)
                        CommonExtensions.Sleep(delay * 2);
                });

                #endregion 处理回复消息和回复

                #region 历史记录保存

                DeleteExpireImage();
                // MEMO : 添加本次请求内容
                loadedHistories.Add(Message.FromUser(thisRequestContentParts));
                // MEMO : 添加本次回复内容
                var aiContentParts = new List<ContentPart>();
                aiChatResponse.Contents
                    .ForEach(each => aiContentParts.Add(new TextContent(new AIChatRequest
                    {
                        NickName = BOT_NAME,
                        Message = each.ChatMessageInfo.Text,
                    }.ToJsonIgnoreNull())));
                var aiMessage = Message.FromAssistant(string.Empty);
                aiMessage.Content = aiContentParts;
                // MEMO : AI回复只保留text
                //loadedHistories.Add(Message.FromAssistant(aiChatResponse.ToJsonIgnoreNull()));
                loadedHistories.Add(aiMessage);
                // MEMO : 保存历史记录
                if (saveHistory)
                    loadedHistories.SaveAIHistory(chatKey);

                #endregion 历史记录保存

                WriteLog();

                void WriteLog()
                {
                    // MEMO : 写入前台日志
                    var apiKey = GlobalAIControl.AIConfig.ApiKeyChat;
                    LogExtensions.AddRunLog(new RunLog_AIRequest(sendTargetId, isGroupRequest, apiKey, response.Usage));
                    // MEMO : 写入Log
                    YameiLogExtensions.WriteLog(apiKey, response, thisRequestContentParts, aiStatus.ToJsonIgnoreNull());
                }
            }
            catch (JsonException ex)
            {
                #region Json转换失败处理

                retryTimes++;
                YameiLogExtensions.WriteJsonDeserializeLog(ex, nameof(AIChatResponse), $"[GeminiError]{responseText}");
                if (IsDebug)
                    botSendMessage(isGroupRequest ? TestGroupId : requestTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ERROR_JSON_ERROR)}");

                continue;

                #endregion Json转换失败处理
            }
            catch (OpenRouterException ex)
            {
                #region OpenRouter错误处理

                retryTimes++;
                YameiLogExtensions.WriteLog(LogType.Error, $"[GeminiError]{ex.Message}");
                if (IsDebug)
                    botSendMessage(isGroupRequest ? TestGroupId : requestTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ex.Message)}");

                continue;

                #endregion OpenRouter错误处理
            }
            catch (Exception ex)
            {
                #region 其他错误处理

                retryTimes++;
                YameiLogExtensions.WriteLog(LogType.Error, $"[GeminiError]{ex.GetType()}{ex.Message}");
                if (IsDebug)
                    botSendMessage(isGroupRequest ? TestGroupId : requestTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ex.Message)}");

                continue;

                #endregion 其他错误处理
            }

            return;
        }

        if (isGroupRequest)
        {
            if (isGroupMemberRequest)
                botSendMessage(requestTargetId, $"{CQCode.At(requestTargetId)} 哈基米请求失败! 请求重试次数超过限制!");
        }
        else
        {
            botSendMessage(requestTargetId, $"哈基米请求失败! 请求重试次数超过限制!");
        }

        // 删除过期图片信息
        void DeleteExpireImage()
        {
            var aiCharRequest = new AIChatRequest();
            for (var i = thisRequestContentParts.Count - 1; i >= 0; i--)
            {
                var contentPart = thisRequestContentParts[i];
                if (contentPart.Type == "text")
                {
                    aiCharRequest = ((TextContent)contentPart).Text.FromJson<AIChatRequest>();
                }
                else
                {
                    thisRequestContentParts[i] = new TextContent(new AIChatRequest
                    {
                        NickName = aiCharRequest.NickName,
                        Message = "[图片已过期]",
                    }.ToJsonIgnoreNull());
                }
            }

            //thisRequestContentParts.ForEach(contentPart =>
            //{
            //    if (contentPart.Content is List<ContentPart> contentParts && contentParts.Count >= 2)
            //    {
            //        var count = contentParts.Count;
            //        var userName = ((TextContent)contentParts[count - 1]).Text.FromJson<AIChatRequest>().NickName;
            //        for (var i = 0; i <= contentParts.Count - 2; i++)
            //        {
            //            contentParts[i] = new TextContent(new AIChatRequest
            //            {
            //                NickName = userName,
            //                Message = "[图片已过期]",
            //            }.ToJsonIgnoreNull());
            //        }
            //    }
            //});
        }
    }

    private static Tool GetTool_Response() => Tool.CreateFunctionTool(
        "reply_user",
        "Call this function to send a response to the user.",
        SchemaDeepPatcher.Generate(typeof(AIChatResponse)));

    private static string CreateSendMessage(
        AIGroupConfig aiGroupConfig,
        AIChatResponseContent aiChatResponseContent,
        bool needAt,
        string targetId,
        bool isGroupRequest)
    {
        var think = aiChatResponseContent.Think;
        var body = aiChatResponseContent.Body;
        var sensory = aiChatResponseContent.Sensory;
        var mind = aiChatResponseContent.Mind;
        var face = aiChatResponseContent.Face;
        var chatMessage = aiChatResponseContent.ChatMessageInfo;
        string resultMessage;
        if (IsDebug)
        {
            resultMessage = $"{(think.IsNullOrEmpty() ? string.Empty : $"[思考:{think}]{ENTER}")}"
                + $"{(sensory.IsNullOrEmpty() ? string.Empty : $"[感受:{sensory}]{ENTER}")}"
                + $"{(mind.IsNullOrEmpty() ? string.Empty : $"[心想:{mind}]{ENTER}")}"
                + GetExpressionText(true)
                + $"{(body.IsNullOrEmpty() ? string.Empty : $"[动作:{body}]{ENTER}")}";

            resultMessage += chatMessage.DeleteCode(true, needAt, targetId);
        }
        else
        {
            resultMessage = string.Empty;
            if (isGroupRequest)
            {
                if (aiGroupConfig.ShowThinking)
                    resultMessage += $"{(think.IsNullOrEmpty() ? string.Empty : $"[思考:{think}]{ENTER}")}";

                if (aiGroupConfig.ShowSensory)
                    resultMessage += $"{(sensory.IsNullOrEmpty() ? string.Empty : $"[感受:{sensory}]{ENTER}")}";

                if (aiGroupConfig.ShowPsychologicalDesc)
                    resultMessage += $"{(mind.IsNullOrEmpty() ? string.Empty : $"[心想:{mind}]{ENTER}")}";

                if (!aiGroupConfig.ShowThinking && !aiGroupConfig.ShowSensory && !aiGroupConfig.ShowPsychologicalDesc)
                {
                    resultMessage += $"{(aiGroupConfig.ShowExpression ? GetExpressionText(false) : string.Empty)}"
                        + $"{(aiGroupConfig.ShowBodyLanguage ? (body.IsNullOrEmpty() ? string.Empty : $"[{body}]{ENTER}") : string.Empty)}";
                }
                else
                {
                    if (aiGroupConfig.ShowExpression)
                        resultMessage += GetExpressionText(true);

                    if (aiGroupConfig.ShowBodyLanguage)
                        resultMessage += $"{(body.IsNullOrEmpty() ? string.Empty : $"[动作:{body}]{ENTER}")}";
                }

                resultMessage += chatMessage.DeleteCode(aiGroupConfig.ShowEmojiImage, needAt, targetId);
            }
            else
            {
                if (targetId == SuperAdminId)
                {
                    resultMessage += $"{(sensory.IsNullOrEmpty() ? string.Empty : $"[感受:{sensory}]{ENTER}")}"
                        + $"{(mind.IsNullOrEmpty() ? string.Empty : $"[心想:{mind}]{ENTER}")}"
                        + $"{(body.IsNullOrEmpty() ? string.Empty : $"[动作:{body}]{ENTER}")}"
                        + GetExpressionText(true);
                }
                else
                {
                    resultMessage += $"{GetExpressionText(false)}{(body.IsNullOrEmpty() ? string.Empty : $"[{body}]{ENTER}")}";
                }

                resultMessage += chatMessage.DeleteCode(true, false);
            }
        }

        return resultMessage;

        string GetExpressionText(bool useTitle) => face is null or AIExpressionType.None
            ? string.Empty
            : $"[{(useTitle ? "表情:" : string.Empty)}{face.GetDisplay()}]{(useTitle ? ENTER : string.Empty)}";
    }

    private static string GetEmojiCode(AIChatMessage aiChatMessage, bool showEmojiImage)
    {
        if (!showEmojiImage)
        {
            aiChatMessage.Emoji = null;
            return string.Empty;
        }

        var emoji = aiChatMessage.Emoji;
        return emoji is null or AIEmojiType.None
            ? string.Empty
            : CQCode.Image($"file:///{GlobalAIConfig.FacePath}{emoji.ToString()}.png", isBiaoQing: true);
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

    /// <param name="contentParts"><see cref="List{ContentPart}"/></param>
    extension(List<ContentPart> contentParts)
    {
        /// <summary>
        /// 追加小助手状态
        /// </summary>
        public async Task<AIStatusInfo> AddStatusAsync(AIMessageSourceType messageSourceType, string targetId = "")
        {
            var weatherContext = await WeatherExtensions.AIGetWeatherDataAsync().ConfigureAwait(false);
            var aiChatStatus = new AIStatusInfo
            {
                Mood = GlobalAIData.AIStatusData.MoodIndexValue.ToMood(),
                Schedule = AIStatusUtil.GetSchedule(),
                NowDate = DateTime.Now.ToYYYYMDHHMMSS(),
                Scene = messageSourceType.ToMessageSourceText(targetId),
                WeatherContext = weatherContext,
            };

            contentParts.Add(new TextContent(aiChatStatus.ToJsonIgnoreNull()));
            return aiChatStatus;
        }

        public async Task AddQQChatMessageAsync(
            AIChatSender sender,
            string messageText,
            Dictionary<string, GroupMember> groupMembers,
            bool imageToText = false)
        {
            if (messageText.IsNullOrEmpty())
                return;

            var message = messageText;
            if (groupMembers?.Any() == true)
            {
                message = _regReplaceAt.Replace(message, match =>
                {
                    var qqId = long.Parse(match.Groups["qqId"].Value).ToString();
                    if (IsDebug && qqId == SuperAdminId)
                        return $"[at:{BOT_NAME}]";

                    return $"[at:{groupMembers[qqId].ToAIChatSender(AIUserInfos).NickName}]";
                });
            }

            var deleteImageJsonText = await contentParts.AddQQChatImageAsync(sender, message, imageToText).ConfigureAwait(false);
            if (!deleteImageJsonText.IsNullOrEmpty())
                contentParts.AddQQChatTextContent(sender, WebUtility.HtmlDecode(deleteImageJsonText));
        }

        public Task AddQQChatMessageAsync(Sender sender, string messageText, Dictionary<string, GroupMember> groupMembers)
            => AddQQChatMessageAsync(contentParts, sender.ToAIChatSender(AIUserInfos), messageText, groupMembers);

        /// <summary>
        /// 添加图片
        /// </summary>
        /// <param name="sender">发送者信息</param>
        /// <param name="messageText">消息内容</param>
        /// <returns>删除图片后的消息</returns>
        private async Task<string> AddQQChatImageAsync(AIChatSender sender, string messageText, bool imageToText = false)
        {
            if (messageText.IsNullOrEmpty())
                return string.Empty;

            var processedMessage = messageText;
            var isAddImage = false;
            var matches = _regCQImageFileUrl.Matches(messageText);
            var thisContentParts = new List<ContentPart>();
            await matches.ForeachAsync(match =>
            {
                var replaceContent = match.Value;
                if (imageToText)
                    thisContentParts.AddQQChatTextContent(sender, "[图片已过期]");
                else
                    thisContentParts.Add(new ImageContent(WebUtility.HtmlDecode(match.Groups["url"].Value)));

                isAddImage = true;
                processedMessage = processedMessage.Replace(replaceContent, string.Empty);
                return Task.CompletedTask;
            }).ConfigureAwait(false);

            if (isAddImage)
                thisContentParts.AddQQChatTextContent(sender, SEND_SOME_IMAGES);

            if (thisContentParts.Any())
                contentParts.AddRange(thisContentParts);

            return processedMessage;
        }

        /// <summary>
        /// 添加聊天文字内容
        /// </summary>
        /// <param name="sender">发送者信息</param>
        /// <param name="messageText">消息内容</param>
        private void AddQQChatTextContent(AIChatSender sender, string messageText)
        {
            if (messageText.IsNullOrEmpty())
                return;

            var contentPart = new AIChatRequest
            {
                SenderId = sender.QQ,
                NickName = sender.NickName,
                Message = QQExtensions.ProcessAIRequestMessage(messageText),
            };
            contentParts.Add(new TextContent(contentPart.ToJsonIgnoreNull()));
        }

        /// <summary>
        /// 追加外置知识库
        /// </summary>
        public void AddKnowledge()
        {
            if (File.Exists(AI_KNOWLEDGE_PATH))
            {
                // MEMO : 插入外置知识库
                AddNote(AI_KNOWLEDGE_PATH);
                AddNote(AI_KNOWLEDGE_NOTE_PATH);
                AddNote(AI_INSPIRATION_NOTE_PATH);

                void AddNote(string filePath)
                {
                    if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath)))
                        return;

                    contentParts.Add(new TextContent(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath), Encoding.UTF8)));
                }
            }
        }

        /// <summary>
        /// 添加系统提示
        /// </summary>
        /// <param name="systemHint">系统提示内容</param>
        public void AddSystemHint(string systemHint)
        {
            if (systemHint.IsNullOrEmpty())
                return;

            var contentPart = new AIChatRequest
            {
                SenderId = AISystemId,
                NickName = AISystemHintName,
                Message = systemHint,
            };
            contentParts.Add(new TextContent(contentPart.ToJsonIgnoreNull()));
        }
    }

    public static void SaveAIHistory(this List<Message> messages, string key)
        => messages.ToJsonFile(GetAIHistoryPath(key));

    /// <summary>
    /// 读取历史记录
    /// </summary>
    public static List<Message> LoadAIHistory(string key)
        => JsonExtensions.FromJsonFile<List<Message>>(GetAIHistoryPath(key)) ?? [];

    /// <summary>
    /// 是否不方便发送消息的时候
    /// </summary>
    public static bool IsCantSendMessage(string requestTargetId, Action<string, string> botSendMessage)
    {
        var schedule = AIStatusUtil.GetSchedule();
        var isSendResponse = !requestTargetId.IsNullOrEmpty();
        // MEMO : 日程在深度睡眠时, 不回应
        if (schedule.Contains("deep sleep"))
        {
            if (isSendResponse)
                botSendMessage(requestTargetId, MESSAGE_SLEEP);

            return true;
        }

        // MEMO : 自读时间不该回复...
        if (schedule.Contains("masturbation"))
        {
            if (isSendResponse)
                botSendMessage(requestTargetId, MESSAGE_BUSY);

            return true;
        }

        return false;
    }

    /// <summary>
    /// 删除AI不该出现在聊天中的Code
    /// </summary>
    /// <param name="aiChatMessage"><see cref="AIChatMessage"/></param>
    /// <param name="showEmojiImage">是否显示表示</param>
    /// <param name="needAt">是否文字开头加at</param>
    /// <param name="targetId">at对象QQ号</param>
    /// <returns>替换结果</returns>
    public static string DeleteCode(this AIChatMessage aiChatMessage, bool showEmojiImage, bool needAt, string targetId = "")
    {
        //var emojiCode = aiChatMessage.Emoji;
        var result = aiChatMessage.Text ?? string.Empty;
        //Replace(@"\[emojiCode:.+?\]", string.Empty);

        //result = _regDeleteErrorEmoji.Replace(result, match =>
        //{
        //    var matchValue = match.Value;
        //    var emojiCodeMatch = match.Groups["emojiCode"].Value;
        //    if (Enum.TryParse(typeof(AIEmojiType), emojiCodeMatch, true, out _))
        //    {
        //        if (emojiCode is null or AIEmojiType.None)
        //            emojiCode = emojiCodeMatch;

        //        return string.Empty;
        //    }

        //    return $"[{matchValue}]";
        //});

        //result = _regDeleteErrorEmoji2.Replace(result, match =>
        //{
        //    var matchValue = match.Value;
        //    if (Enum.TryParse(typeof(AIEmojiType), matchValue, true, out _))
        //    {
        //        if (emojiCode.IsNullOrEmpty())
        //            emojiCode = matchValue;

        //        return string.Empty;
        //    }

        //    return $"[{matchValue}]";
        //});

        //result = _regDeleteErrorEmoji3.Replace(result, match =>
        //{
        //    var matchValue = match.Value;
        //    if (Enum.TryParse(typeof(AIEmojiType), matchValue, true, out _))
        //    {
        //        if (emojiCode.IsNullOrEmpty())
        //            emojiCode = matchValue;

        //        return string.Empty;
        //    }

        //    return $"({matchValue})";
        //});

        //aiChatMessage.Emoji = emojiCode;
        return GetEmojiCode(aiChatMessage, showEmojiImage) + (needAt ? $"{CQCode.At(targetId)} " : string.Empty) + result;
    }

    public static AIUserData GetAIUserData(string targetId) => GlobalAIData.UserDatas.GetOrAdd(targetId,
        () =>
        {
            var userInfo = AIUserInfos.GetValueOrDefault(targetId);
            var aiUserData = new AIUserData();
            var userInfoRelation = userInfo?.GetRelation() ?? new AIRelationData();
            aiUserData.Relation = new AIRelationData
            {
                Intimacy = userInfoRelation.Intimacy,
                Respect = userInfoRelation.Respect,
                Affection = userInfoRelation.Affection,
            };

            return aiUserData;
        });

    private static string GetAIHistoryPath(string key) => Path.Combine(AI_HISTORY_PATH, $"{key}.json");

    private static void WriteAINote(AIKnowledgeNote knowledgeNote)
    {
        using var fs = new FileStream(AI_KNOWLEDGE_NOTE_PATH, FileMode.Append, FileAccess.Write);
        using var sw = new StreamWriter(fs, Encoding.UTF8);
        sw.Write($"# {knowledgeNote.Title}{ENTER}{ENTER}"
            + $"{knowledgeNote.Content}{ENTER}※Date: {DateTime.Now.ToYYYYMDHHMMSS()}{ENTER}{ENTER}");
        sw.Close();
        fs.Close();
    }

    private static void WriteAINote(AIInspirationNote inspirationNote)
    {
        using var fs = new FileStream(AI_INSPIRATION_NOTE_PATH, FileMode.Append, FileAccess.Write);
        using var sw = new StreamWriter(fs, Encoding.UTF8);
        sw.Write($"# {inspirationNote.Title}{ENTER}{ENTER}"
            + $"{inspirationNote.Content}{ENTER}※Date: {DateTime.Now.ToYYYYMDHHMMSS()}{ENTER}{ENTER}");
        sw.Close();
        fs.Close();
    }

    ///// <summary>
    ///// 删除AI回复中的其他信息(思考, 动作, 表情等), 只保留文字内容
    ///// <remarks>
    ///// 额外的信息保留可能导致AI复读自己的内容
    ///// </remarks>
    ///// </summary>
    ///// <param name="aiChatResponse"></param>
    //private static void DeleteOtherInfo(this AIChatResponse aiChatResponse)
    //{
    //    aiChatResponse.Contents.ForEach(content =>
    //    {
    //        content.Think = null;
    //        content.Sensory = null;
    //        content.Body = null;
    //        content.Face = null;
    //        content.Mind = null;
    //        content.ChatMessageInfo.Delay = null;
    //        content.ChatMessageInfo.Emoji = null;
    //    });
    //    aiChatResponse.FavorabilityChangeInfos = null;
    //    aiChatResponse.StatusChangeInfo = null;
    //    aiChatResponse.BlockUserInfos = null;
    //    aiChatResponse.KnowledgeNote = null;
    //    aiChatResponse.InspirationNote = null;
    //}

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