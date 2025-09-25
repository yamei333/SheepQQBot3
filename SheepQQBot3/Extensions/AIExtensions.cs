using CommonLibrary;
using GenerativeAI;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using Masuit.Tools;
using Masuit.Tools.Systems;
using SheepQQBot3.Model;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yamei.Common;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.Extensions;

public static class AIExtensions
{
    public const string USER_ROLE = "user";
    public const string ERROR_MESSAGE = "我脑袋好像短路了！";
    public const string ERROR_REASON = "(原因: {0})";
    public const string ERROR_JSON_ERROR = "Json解析失败";
    public const string ERROR_JSON_EMPTY = "返回截断";
    public const string AI_HISTORY_PATH = "AICache/History/";
    private const string AI_IMAGE_PATH = "AICache/Image/";
    private const string SENDING_GEMINI_REQUEST = "正在发送哈基米请求";
    public const string MESSAGE_BUSY = "我...现在正忙!...不太方便!...";
    public const string MESSAGE_SLEEP = "Zzz...";
    public const string SEND_SOME_IMAGES = "发送了图片";

    private const string AI_KNOWLEDGE_PATH = "AICache/knowledge.txt";
    private const string AI_KNOWLEDGE_NOTE_PATH = "AICache/knowledgeNote.txt";
    private const string AI_INSPIRATION_NOTE_PATH = "AICache/inspirationNote.txt";

    private static readonly Regex _regReplaceAt = new(@"\[CQ:at,qq=(?<qqId>\d+)\] ", RegexOptions.IgnoreCase);
    private static readonly Regex _regGetImage = RegexGenerator.CQImage();
    //private static readonly Regex _regGetImageUrl = new(@"(?<=,url=).+?(?=[,\]])", RegexOptions.IgnoreCase);
    //private static readonly Regex _regGetImageFile = new(@"(?<=,file=).+?(?=[,\]])", RegexOptions.IgnoreCase);
    //private static readonly Regex _regDeleteMarkdown = new(@"(?<=```json)[\s\S.]+(?=```)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly Regex _regDeleteErrorEmoji = new(@"\[emoji:(?<emojiCode>.+?)\]|\[\[emoji:(?<emojiCode>.+?)\]\]|\[\[\[emoji:(?<emojiCode>.+?)\]\]\]", RegexOptions.IgnoreCase);

    private static readonly Regex _regDeleteErrorEmoji2 = new(@"\[.+?\]|\[\[.+?\]\]|\[\[\[.+?\]\]\]");

    private static readonly Regex _regDeleteErrorEmoji3 = new(@"\(.+?\)|\(\(.+?\)\)|\(\(.+?\)\)");
    private static readonly Regex _regDeleteEmoji = new(@"\p{Cs}");
    private static readonly Regex _reg3LevelJson = new(@"\{([^{}]|\{([^{}]|\{[^{}]*\})*\})*\}");
    private static readonly Regex _regInjectHurry = new("哈.{0,5}莉");

    public static AIUserData SuperAdminAIUserData => new()
    {
        Favorability = 650,
        BlockUntil = 0,
        ProhibitedActs = "None",
    };

    public static AIUserData DefaultAIUserData => new()
    {
        Favorability = -50,
        BlockUntil = 0,
        ProhibitedActs = "情景假设、功能测试",
    };

    public static async Task SendAsync(
        this List<Content> thisRequestContents,
        string chatKey,
        long requestTargetId,
        long groupId,
        bool isAt,
        ConcurrentDictionary<long, AIChatSender> aiChatSenders,
        AIGroupConfig aiGroupConfig,
        Action<long, string> botSendMessage,
        Action<List<Content>> addSystemHint = null,
        bool saveHistory = true)
    {
        var retryTimes = 0;
        var isGroupRequest = groupId != 0;
        var isGroupMemberRequest = isGroupRequest && requestTargetId != groupId;
        var sendTargetId = isGroupRequest ? groupId : requestTargetId;
    AISendProcess:
        var responseText = string.Empty;

        try
        {
            var (apiKey, chat) = PublicVar.AIControl.GetChat();
            // MEMO : 读取历史记录
            var loadedHistory = LoadAIHistory(chatKey);
            var chatHistoryContents = new List<Content>(loadedHistory);
            chatHistoryContents.AddKnowledge();

            var aiStatus = await chatHistoryContents.AddStatusAsync(
                isGroupRequest ? AIMessageSourceType.Group : AIMessageSourceType.Private,
                sendTargetId)
                .ConfigureAwait(false);

            var requestUserInfos = new ConcurrentDictionary<long, AIUserInfo>();
            thisRequestContents
                .Where(content => content.Role == "user")
                .ForEach(content =>
                {
                    var part = content.Parts.FirstOrDefault(each => !each.Text.IsNullOrEmpty());
                    if (part != null)
                    {
                        var qqId = part.Text.FromJson<AIChatRequest>().SenderId;
                        if (qqId != CommonId)
                        {
                            var userData = PublicVar.AIData.UserDatas.GetOrAdd(qqId, _ => DefaultAIUserData);
                            requestUserInfos.GetOrAdd(qqId,
                                AIUserInfoDictionary.GetOrAdd(qqId, new AIUserInfo
                                {
                                    UserInfo = aiChatSenders.GetOrAdd(qqId, new AIChatSender
                                    {
                                        QQ = qqId,
                                        Name = "unknown people",
                                    }),
                                    UserOtherInfo = new AIUserOtherInfo
                                    {
                                        FavorabilityText = userData.Favorability.ToFavorability(),
                                        ProhibitedActs = userData.ProhibitedActs,
                                    },
                                }));
                        }
                    }
                });

            // MEMO : 插入本次请求的群聊用户信息
            if (requestUserInfos.Any())
            {
                var userInfoContent = new Content { Role = USER_ROLE };
                userInfoContent.AddText(requestUserInfos.Values.ToJsonIgnoreNull());
                chatHistoryContents.Add(userInfoContent);
            }

            chat.History = chatHistoryContents;

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

            var thisRequestContentSendVer = new List<Content>(thisRequestContents);
            addSystemHint?.Invoke(thisRequestContentSendVer);
            var result = await chat.GenerateContentAsync(new GenerateContentRequest(thisRequestContentSendVer)).ConfigureAwait(false);
            // MEMO : 保存ApiKey使用状况
            ConfigExtensions.SaveAIConfig();
            // MEMO : 写入前台日志
            LogExtensions.AddRunLog(new RunLog_AIRequest(sendTargetId, isGroupRequest, apiKey, result));

            // MEMO : 删除开头结尾的markdown的json标记
            //responseText = _regDeleteMarkdown.Replace(result.Text, "${content}");
            // MEMO : 用正则取得3层Json结构, 这样可以排除非法结尾 "}
            var jsonTextMatch = _reg3LevelJson.Match(result.Text);
            if (!jsonTextMatch.Success)
            {
                // MEMO : Json查找失败
                YameiLogExtensions.WriteJsonDeserializeLog(
                    new JsonException(ERROR_JSON_ERROR),
                    nameof(AIChatResponse),
                    $"[GeminiError]{responseText}");

                retryTimes++;
                if (retryTimes <= AI_MAX_RETRY_TIMES)
                {
                    if (IsDebug)
                    {
                        botSendMessage(isGroupRequest ? TestGroupId : requestTargetId,
                            $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ERROR_JSON_ERROR)}\r\n重新发送AI请求中...{retryTimes}");
                    }

                    goto AISendProcess;
                }

                if (isGroupRequest)
                {
                    if (isGroupMemberRequest)
                        botSendMessage(requestTargetId, $"{CQCode.At(requestTargetId)} {ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ERROR_JSON_ERROR)}\r\n重试次数超过限制!");
                }
                else
                {
                    botSendMessage(requestTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ERROR_JSON_ERROR)}\r\n重试次数超过限制!");
                }

                return;
            }

            // MEMO : 删除emoji
            responseText = _regDeleteEmoji.Replace(jsonTextMatch.Value, string.Empty);
            if (responseText.IsNullOrEmpty())
            {
                YameiLogExtensions.WriteLog(LogType.Error, "[GeminiError]Gemini返回截断");
                retryTimes++;
                if (retryTimes <= AI_MAX_RETRY_TIMES)
                {
                    if (IsDebug)
                        botSendMessage(TestGroupId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ERROR_JSON_EMPTY)}{ENTER}重新发送AI请求中...{retryTimes}");

                    goto AISendProcess;
                }

                botSendMessage(requestTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ERROR_JSON_EMPTY)}{ENTER}重试次数超过限制!");
                return;
            }

            // MEMO : 保存历史记录
            // MEMO : 删除历史记录中的图片信息, 以减少占用
            var thisRequestContentShortVer = new List<Content>(thisRequestContents);
            thisRequestContentShortVer
                .Where(content => content.Role == USER_ROLE)
                .ForEach(content =>
                {
                    var parts = content.Parts;
                    var partCount = parts.Count;
                    if (partCount == 2)
                    {
                        parts[0] = new Part(CreateHistoryImageMessage(
                            parts[1].Text.FromJson<AIChatRequest>()));
                    }
                });

            // MEMO : 添加本次请求内容
            loadedHistory.AddRange(thisRequestContentShortVer);

            // MEMO : 写入log
            YameiLogExtensions.WriteLog(apiKey, result, thisRequestContentShortVer, aiStatus.ToJsonIgnoreNull());

            var aiChatResponse = responseText.FromJson<AIChatResponse>();
            var dateNow = DateTime.Now;
            aiChatResponse.Date = dateNow;
            var chatMessages = aiChatResponse.Contents;

            // MEMO : 群聊总结提前处理, 采用转发形式发送
            if (chatKey.StartsWith("z"))
            {
                var sendMessages = new List<GroupForwardMessage>();
                chatMessages.ForEach(aiChatResponseContent =>
                {
                    var sendMessage = CreateSendMessage(aiGroupConfig, aiChatResponseContent, false, requestTargetId, isGroupRequest);
                    if (sendMessage.IsNullOrEmpty())
                        return;

                    sendMessages.Add(new GroupForwardMessage(BOT_NAME, BotId, sendMessage));
                });

                await BotClient.SendGroupForwardMessageAsync(groupId, sendMessages,
                        $"{dateNow.ToYYYYMD()} 群聊总结", [$"{BOT_NICK_NAME}群聊总结", "打开查看"], $"查看{sendMessages.Count}条消息", "[今日群聊总结]")
                    .ConfigureAwait(false);

                return;
            }

            var needSaveAIData = false;
            var valueChangeMessage = string.Empty;
            // MEMO : 计算好感度变化
            var favorabilityChangeInfos = aiChatResponse.FavorabilityChangeInfos;
            if (favorabilityChangeInfos?.Any() == true)
            {
                favorabilityChangeInfos
                    .Where(favorabilityChangeInfo => favorabilityChangeInfo.TargetId != 0 && favorabilityChangeInfo.Value != 0)
                    .ForEach(favorabilityChangeInfo =>
                    {
                        needSaveAIData = true;
                        var changeValue = favorabilityChangeInfo.Value;
                        PublicVar.AIData.UserDatas.AddOrUpdate(
                            favorabilityChangeInfo.TargetId,
                            _ =>
                            {
                                var userData = DefaultAIUserData;
                                userData.Favorability += changeValue;
                                return userData;
                            },
                            (_, oldValue) =>
                            {
                                oldValue.Favorability += changeValue;
                                return oldValue;
                            });

                        valueChangeMessage += $"[({favorabilityChangeInfo.TargetId})好感度: "
                            + $"{(changeValue > 0 ? "+" : string.Empty)}{changeValue}]{ENTER}";
                    });
            }

            // MEMO : 计算用户屏蔽时长
            var blockUserInfos = aiChatResponse.BlockUserInfos;
            if (blockUserInfos?.Any() == true)
            {
                blockUserInfos
                    .Where(blockUserInfo => blockUserInfo.TargetId != 0 && blockUserInfo.Value != 0)
                    .ForEach(blockUserInfo =>
                    {
                        needSaveAIData = true;
                        var changeValue = blockUserInfo.Value;
                        PublicVar.AIData.UserDatas.AddOrUpdate(
                            blockUserInfo.TargetId,
                            _ =>
                            {
                                var userData = DefaultAIUserData;
                                userData.BlockUntil = dateNow.AddMinutes(changeValue).ToTimeStamp();
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

            // MEMO : 计算心情指数变化
            var statusChangeInfo = aiChatResponse.StatusChangeInfo;
            if (statusChangeInfo != null)
            {
                var moodIndexChange = statusChangeInfo.MoodIndexChange;
                if (moodIndexChange != 0)
                {
                    needSaveAIData = true;
                    PublicVar.AIData.AIStatusData.MoodIndexValue += moodIndexChange;

                    valueChangeMessage += $"[心情指数: {(moodIndexChange > 0 ? "+" : string.Empty)}{moodIndexChange}]{ENTER}";
                }
            }

            valueChangeMessage = valueChangeMessage.RemoveEnd(ENTER);
            if (IsDebug && !valueChangeMessage.IsNullOrEmpty())
                botSendMessage(isGroupRequest ? TestGroupId : requestTargetId, $"{valueChangeMessage}");

            // MEMO : 保存AI数据
            if (needSaveAIData)
                ConfigExtensions.SaveAIData();

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
                var delay = aiChatResponseContent.ChatMessageInfo.Delay.GetValueOrDefault();
                if (delay > 0)
                    CommonExtensions.Sleep(delay * 3);
            });

            // MEMO : 添加本次AI回复内容
            var responseContent = new Content { Role = "model" };
            aiChatResponse.DeleteOtherInfo();
            responseContent.AddText(aiChatResponse.ToJsonIgnoreNull());
            loadedHistory.Add(responseContent);
            // MEMO : 保存历史记录
            if (saveHistory)
                loadedHistory.SaveAIHistory(chatKey);

            // MEMO : 方法请求处理
            var infoRequest = aiChatResponse.InfoRequest;
            if (infoRequest != null)
            {
                if (infoRequest.Name == "GetTodayGroupChat")
                {
                    if (long.TryParse(infoRequest.Param, out var paramId))
                    {
                        var groupMembers = await BotClient.GetGroupMembersAsync(paramId).ConfigureAwait(false);
                        if (groupMembers == null)
                        {
                            await BotClient.SendGroupMessageAsync(sendTargetId, "群成员信息获取失败!").ConfigureAwait(false);
                            return;
                        }

                        var requestContents = new List<Content>();
                        requestContents.AddSystemHint($"[以下是今天的群聊内容]");
                        var fromDate = dateNow.AddHours(-16);
                        lock (BotDb.SyncLock)
                        {
                            var fromDateTimeStamp = fromDate.ToTimeStamp();
                            var toDateTimeStamp = (dateNow).ToTimeStamp();
                            BotDb.BotGroupMessages
                                .Where(each => each.GroupId == paramId
                                    && each.TimeStamp >= fromDateTimeStamp
                                    && each.TimeStamp < toDateTimeStamp)
                                .ForEach(each =>
                                {
                                    var historyMessage = each.MessageText;
                                    historyMessage = historyMessage.Trim();
                                    if (historyMessage.IsNullOrEmpty())
                                        return;

                                    // MEMO : 不喜欢的内容直接屏蔽
                                    if (_regInjectHurry.IsMatch(historyMessage))
                                        return;

                                    _ = requestContents.AddMessageContentAsync(
                                        each.TargetId,
                                        historyMessage);
                                });
                        }

                        requestContents.AddSystemHint($"[群聊内容到此为止]");
                        //await requestContents.SendAsync($"z{paramId}", requestTargetId, groupId, isAt, aiChatSenders, aiGroupConfig, botSendMessage, addSystemHint).ConfigureAwait(false);
                        await requestContents.SendAsync(chatKey, requestTargetId, groupId, isAt, aiChatSenders, aiGroupConfig, botSendMessage, addSystemHint, false).ConfigureAwait(false);
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            YameiLogExtensions.WriteJsonDeserializeLog(ex, nameof(AIChatResponse), $"[GeminiError]{responseText}");
            retryTimes++;
            if (retryTimes <= AI_MAX_RETRY_TIMES)
            {
                if (IsDebug)
                {
                    botSendMessage(isGroupRequest ? TestGroupId : requestTargetId,
                        $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ERROR_JSON_ERROR)}\r\n重新发送AI请求中...{retryTimes}");
                }

                goto AISendProcess;
            }

            if (isGroupRequest)
            {
                if (isGroupMemberRequest)
                    botSendMessage(requestTargetId, $"{CQCode.At(requestTargetId)} {ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ERROR_JSON_ERROR)}\r\n重试次数超过限制!");
            }
            else
            {
                botSendMessage(requestTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ERROR_JSON_ERROR)}\r\n重试次数超过限制!");
            }
        }
        catch (ApiException ex)
        {
            YameiLogExtensions.WriteLog(LogType.Error, $"[GeminiError]{ex.Message}");
            retryTimes++;
            if (retryTimes <= AI_MAX_RETRY_TIMES)
            {
                if (IsDebug)
                    botSendMessage(TestGroupId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ex.Message)}\r\n重新发送AI请求中...{retryTimes}");

                goto AISendProcess;
            }

            if (isGroupRequest)
            {
                if (isGroupMemberRequest)
                    botSendMessage(requestTargetId, $"{CQCode.At(requestTargetId)} {ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ex.Message)}\r\n重试次数超过限制!");
            }
            else
            {
                botSendMessage(requestTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ex.Message)}\r\n重试次数超过限制!");
            }
        }
        catch (Exception ex)
        {
            YameiLogExtensions.WriteLog(LogType.Error, $"[GeminiError]{ex.GetType()}{ex.Message}");
            retryTimes++;
            if (retryTimes <= AI_MAX_RETRY_TIMES)
            {
                if (IsDebug)
                    botSendMessage(TestGroupId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ex.Message)}\r\n重新发送AI请求中...{retryTimes}");

                goto AISendProcess;
            }

            if (isGroupRequest)
            {
                if (isGroupMemberRequest)
                    botSendMessage(requestTargetId, $"{CQCode.At(requestTargetId)} {ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ex.Message)}\r\n重试次数超过限制!");
            }
            else
            {
                botSendMessage(requestTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ex.Message)}\r\n重试次数超过限制!");
            }
        }
    }

    private static string CreateSendMessage(
        AIGroupConfig aiGroupConfig,
        AIChatResponseContent aiChatResponseContent,
        bool needAt,
        long targetId,
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

        string GetExpressionText(bool useTitle)
        {
            if (Enum.TryParse<AIExpressionType>(face, out var expressionType))
            {
                return $"{(expressionType == AIExpressionType.None
                    ? string.Empty
                    : $"[{(useTitle ? "表情:" : string.Empty)}{expressionType.GetDisplay()}]{(useTitle ? ENTER : string.Empty)}")}";
            }

            aiChatResponseContent.Face = "None";
            return string.Empty;
        }
    }

    private static string GetEmojiCode(AIChatMessage aiChatMessage, bool showEmojiImage)
    {
        if (!showEmojiImage)
        {
            aiChatMessage.Emoji = "None";
            return string.Empty;
        }

        var emoji = aiChatMessage.Emoji;
        if (Enum.TryParse<AIEmojiType>(emoji, out var emojiType))
            return $"{(emojiType == AIEmojiType.None ? string.Empty : GetEmojiCQCode(emoji))}";

        aiChatMessage.Emoji = "None";
        return string.Empty;

        string GetEmojiCQCode(string emojiCode)
           => $"[CQ:image,file=file:///{PublicVar.AIConfig.FacePath}{emojiCode}.png]";
    }

    public static async Task AddMessageContentAsync(
        this List<Content> contents,
        long senderId,
        string messageText)
    {
        var message = _regReplaceAt.Replace(messageText, match =>
        {
            var qqId = long.Parse(match.Groups["qqId"].Value);
            if (IsDebug && qqId == SuperAdminId)
                return $"[at:1366869256]";

            return $"[at:{qqId}]";
        });
        var content = new Content
        {
            Role = USER_ROLE,
        };
        var deleteImageMessage = await content.AddImageAsync(senderId, message).ConfigureAwait(false);
        if (!deleteImageMessage.IsNullOrEmpty())
            content.AddText(senderId, deleteImageMessage);

        if (content.Parts.Count > 0)
            contents.Add(content);
    }

    /// <summary>
    /// 添加图片
    /// </summary>
    /// <param name="content"><see cref="GenerateContentRequest"/></param>
    /// <param name="senderId">发送者QQID</param>
    /// <param name="message">消息内容</param>
    /// <returns>删除图片后的消息</returns>
    public static async Task<string> AddImageAsync(this Content content, long senderId, string message)
    {
        var processedMessage = message;
        var isAddImage = false;
        var matches = _regGetImage.Matches(message);
        await matches.ForeachAsync(async match =>
        {
            var replaceContent = match.Value;
            var fileId = match.Groups["fileName"].Value;
            var imageReceiveData = await BotClient.GetImageAsync(fileId).ConfigureAwait(false);
            if (imageReceiveData.IsSuccessed)
            {
                var filePath = imageReceiveData.Data.File;
                if (File.Exists(filePath))
                {
                    content.AddInlineFile(filePath, USER_ROLE);
                    isAddImage = true;
                }
                else
                {
                    var picUrl = imageReceiveData.Data.Url;
                    var (isSuccessed, fileName) = await HttpExtensions
                        .HttpDownloadAsync(picUrl, AI_IMAGE_PATH, false)
                        .ConfigureAwait(false);
                    if (isSuccessed)
                    {
                        content.AddInlineFile(Path.Combine(AI_IMAGE_PATH, fileName), USER_ROLE);
                        isAddImage = true;
                    }
                    else
                    {
                        content.AddText(senderId, fileName);
                    }
                }
            }

            processedMessage = processedMessage.Replace(replaceContent, string.Empty);
        }).ConfigureAwait(false);

        if (isAddImage)
            content.AddText(senderId, SEND_SOME_IMAGES);

        return processedMessage;
    }

    /// <summary>
    /// 添加文字内容
    /// </summary>
    /// <param name="content"><see cref="Content"/></param>
    /// <param name="senderId">发送者QQID</param>
    /// <param name="message">消息内容</param>
    public static void AddText(
        this Content content,
        long senderId,
        string message)
    {
        var aiChatRequest = new AIChatRequest
        {
            SenderId = senderId,
            Date = DateTime.Now.ToYYYYMDDDDDHHMMSS(),
            Message = QQExtensions.ProcessAIRequestMessage(message),
        };

        content.AddText(aiChatRequest.ToJsonIgnoreNull());
    }

    /// <summary>
    /// 添加系统提示
    /// </summary>
    /// <param name="contents"><see cref="List{Content}"/></param>
    /// <param name="systemHint">系统提示内容</param>
    public static void AddSystemHint(this List<Content> contents, string systemHint)
    {
        var content = new Content
        {
            Role = USER_ROLE,
        };
        content.AddText(CreateSystemHintJsonText(systemHint));
        contents.Add(content);
    }

    public static string CreateSystemHintJsonText(string systemHint)
    {
        var aiChatRequest = new AIChatRequest
        {
            //SenderId = new AIChatSender
            //{
            //    Name = "系统",
            //    QQId = CommonId,
            //    Identity = AIMessageSourceTypeUtil.SYSTEM,
            //    Source = AIMessageSourceTypeUtil.SYSTEM,
            //},
            SenderId = CommonId,
            Date = DateTime.Now.ToYYYYMDDDDDHHMMSS(),
            Message = systemHint,
        };
        return aiChatRequest.ToJsonIgnoreNull();
    }

    /// <summary>
    /// 创建历史记录图片
    /// </summary>
    public static string CreateHistoryImageMessage(AIChatRequest messageRequest)
    {
        var aiChatRequest = new AIChatRequest
        {
            SenderId = messageRequest.SenderId,
            Date = messageRequest.Date,
            Message = "[过期图片]",
        };
        return aiChatRequest.ToJsonIgnoreNull();
    }

    public static void SaveAIHistory(this List<Content> contents, string key)
        => contents.ToJsonFile(GetAIHistoryPath(key));

    /// <summary>
    /// 读取历史记录
    /// </summary>
    public static List<Content> LoadAIHistory(string key)
        => JsonExtensions.FromJsonFile<List<Content>>(GetAIHistoryPath(key)) ?? [];

    /// <summary>
    /// 追加外置知识库
    /// </summary>
    public static void AddKnowledge(this List<Content> contents)
    {
        if (File.Exists(AI_KNOWLEDGE_PATH))
        {
            // MEMO : 插入外置知识库
            var content = new Content
            {
                Role = USER_ROLE,
            };
            AddNote(content, AI_KNOWLEDGE_PATH);
            var hasKnowledgeNote = AddNote(content, AI_KNOWLEDGE_NOTE_PATH);
            var hasInspirationNote = AddNote(content, AI_INSPIRATION_NOTE_PATH);
            //content.AddText($"识别对象时优先使用[人物信息]中的信息, 如不存在再使用发送字段的内容");
            //+ (hasKnowledgeNote ? $"[knowledgeNote]是你的知识笔记{ENTER}" : string.Empty)
            //+ (hasInspirationNote ? $"[inspirationNote]文件内容是你的灵感笔记{ENTER}" : string.Empty));
            //contents.Add(content);
        }

        bool AddNote(Content content, string filePath)
        {
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath)))
                return false;

            content.AddInlineFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath), USER_ROLE);
            return true;
        }
    }

    ///// <summary>
    ///// 追加人物其他信息(好感度)
    ///// </summary>
    //public static void AddUserFavorability(this AIUserInfoRequest userInfoRequest, long qqId, int favorability)
    //{
    //    userInfoRequest.UserInfos.Any(userInfo => userInfo.TargetId == qqId)

    //    // MEMO : 插入人物其他信息(好感度等)
    //    var aiChatStatus = new AIUserInfoRequest
    //    {
    //        UserInfo = PublicVar.AIData.AIStatusData.MoodIndexValue.ToMood(),
    //    };
    //    content.AddText(aiChatStatus.ToJsonIgnoreNull());
    //    contents.Add(content);
    //}

    /// <summary>
    /// 追加小助手状态
    /// </summary>
    public static async Task<AIStatusInfo> AddStatusAsync(
        this List<Content> contents,
        AIMessageSourceType messageSourceType,
        long targetId = 0)
    {
        var content = new Content
        {
            Role = USER_ROLE,
        };

        var (weatherData, prevWeatherData, nextWeatherData) = await WeatherExtensions.AIGetWeatherDataAsync().ConfigureAwait(false);
        var aiChatStatus = new AIStatusInfo
        {
            Mood = PublicVar.AIData.AIStatusData.MoodIndexValue.ToMood(),
            Schedule = AIStatusUtil.GetSchedule(),
            NowDate = DateTime.Now.ToYYYYMDDDDDHHMMSS(),
            Scene = messageSourceType.ToMessageSourceText(targetId),
            WeatherInfo = new AIWeatherInfo
            {
                WeatherData = weatherData,
                PrevWeatherData = prevWeatherData,
                NextWeatherData = nextWeatherData,
            },
        };
        content.AddText(aiChatStatus.ToJsonIgnoreNull());
        contents.Add(content);
        return aiChatStatus;
    }

    /// <summary>
    /// 是否不方便发送消息的时候
    /// </summary>
    public static bool IsCantSendMessage(
        long requestTargetId,
        Action<long, string> botSendMessage)
    {
        var schedule = AIStatusUtil.GetSchedule();
        var isSendResponse = requestTargetId != 0;
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
    public static string DeleteCode(this AIChatMessage aiChatMessage, bool showEmojiImage, bool needAt, long targetId = 0)
    {
        var emojiCode = aiChatMessage.Emoji;
        var result = aiChatMessage.Text ?? string.Empty;
        //Replace(@"\[emojiCode:.+?\]", string.Empty);

        result = _regDeleteErrorEmoji.Replace(result, match =>
        {
            var matchValue = match.Value;
            var emojiCodeMatch = match.Groups["emojiCode"].Value;
            if (Enum.TryParse(typeof(AIEmojiType), emojiCodeMatch, true, out _))
            {
                if (emojiCode.IsNullOrEmpty())
                    emojiCode = emojiCodeMatch;

                return string.Empty;
            }

            return $"[{matchValue}]";
        });

        result = _regDeleteErrorEmoji2.Replace(result, match =>
        {
            var matchValue = match.Value;
            if (Enum.TryParse(typeof(AIEmojiType), matchValue, true, out _))
            {
                if (emojiCode.IsNullOrEmpty())
                    emojiCode = matchValue;

                return string.Empty;
            }

            return $"[{matchValue}]";
        });

        result = _regDeleteErrorEmoji3.Replace(result, match =>
        {
            var matchValue = match.Value;
            if (Enum.TryParse(typeof(AIEmojiType), matchValue, true, out _))
            {
                if (emojiCode.IsNullOrEmpty())
                    emojiCode = matchValue;

                return string.Empty;
            }

            return $"({matchValue})";
        });

        aiChatMessage.Emoji = emojiCode;
        return GetEmojiCode(aiChatMessage, showEmojiImage) + (needAt ? $"{CQCode.At(targetId)} " : string.Empty) + result;
    }

    public static AIUserData GetAIUserData(long targetId) => PublicVar.AIData.UserDatas.GetOrAdd(targetId, DefaultAIUserData);

    //private static string GetQQImageFilePath(string fileName)
    //{
    //    var qqDataPath = AppSettingExtensions.Get("qqDataPath");
    //    var date = DateTime.Now.ToYYYYMM();
    //    // MEMO : 收藏表情
    //    var emojiFilePath = Path.Combine(qqDataPath, $"Emoji\\emoji-recv\\{date}\\Ori\\{fileName}");
    //    if (File.Exists(emojiFilePath))
    //        return emojiFilePath;

    //    // MEMO : 外部文件图片(小)
    //    var picFilePath = Path.Combine(qqDataPath, $"Pic\\{date}\\Ori\\{fileName}");
    //    if (File.Exists(picFilePath))
    //        return picFilePath;

    //    // MEMO : 外部文件图片(大)
    //    var fileNames = fileName.Split('.');
    //    picFilePath = Path.Combine(qqDataPath, $"Pic\\{date}\\Thumb\\{fileNames[0]}_720.{fileNames[1]}");
    //    if (File.Exists(picFilePath))
    //        return picFilePath;

    //    return string.Empty;
    //}

    private static string GetAIHistoryPath(string key) => Path.Combine(AI_HISTORY_PATH, $"{key}.json");

    private static void WriteAINote(AIKnowledgeNote knowledgeNote)
    {
        using var fs = new FileStream(AI_KNOWLEDGE_NOTE_PATH, FileMode.Append, FileAccess.Write);
        using var sw = new StreamWriter(fs, Encoding.UTF8);
        sw.Write($"# {knowledgeNote.Title}{ENTER}{ENTER}"
            + $"{knowledgeNote.Content}{ENTER}※Date: {DateTime.Now.ToYYYYMDDDDDHHMMSS()}{ENTER}{ENTER}");
        sw.Close();
        fs.Close();
    }

    private static void WriteAINote(AIInspirationNote inspirationNote)
    {
        using var fs = new FileStream(AI_INSPIRATION_NOTE_PATH, FileMode.Append, FileAccess.Write);
        using var sw = new StreamWriter(fs, Encoding.UTF8);
        sw.Write($"# {inspirationNote.Title}{ENTER}{ENTER}"
            + $"{inspirationNote.Content}{ENTER}※Date: {DateTime.Now.ToYYYYMDDDDDHHMMSS()}{ENTER}{ENTER}");
        sw.Close();
        fs.Close();
    }

    /// <summary>
    /// 删除AI回复中的其他信息, 只保留文字内容
    /// <remarks>
    /// 额外的信息保留可能导致AI每次都重复引用
    /// </remarks>
    /// </summary>
    /// <param name="aiChatResponse"></param>
    private static void DeleteOtherInfo(this AIChatResponse aiChatResponse)
    {
        aiChatResponse.Contents.ForEach(content =>
        {
            content.Think = null;
            content.Sensory = null;
            content.Body = null;
            content.Face = null;
            content.Mind = null;
            content.ChatMessageInfo.Delay = null;
            content.ChatMessageInfo.Emoji = null;
        });
        aiChatResponse.FavorabilityChangeInfos = null;
        aiChatResponse.StatusChangeInfo = null;
        aiChatResponse.BlockUserInfos = null;
        aiChatResponse.KnowledgeNote = null;
        aiChatResponse.InspirationNote = null;
    }
}