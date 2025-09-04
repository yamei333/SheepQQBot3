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
    private const string AI_HISTORY_PATH = "AICache/History/";
    private const string AI_IMAGE_PATH = "AICache/Image/";
    private const string SENDING_GEMINI_REQUEST = "正在发送哈基米请求";

    private const string AI_KNOWLEDGE_NOTE_PATH = "AICache/knowledgeNote.txt";
    private const string AI_INSPIRATION_NOTE_PATH = "AICache/inspirationNote.txt";

    private static readonly Regex _regReplaceAt = new(@"\[CQ:at,qq=(?<qqId>\d+)\] ", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly Regex _regGetImage = new(@"\[CQ:image,.+?\]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly Regex _regGetImageUrl = new(@"(?<=,url=).+?(?=[,\]])", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly Regex _regGetImageFile = new(@"(?<=,file=).+?(?=[,\]])", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly Regex _regDeleteMarkdown = new(@"(?<=```json)[\s\S.]+(?=```)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly Regex _regDeleteEmoji2 = new(@"\[.+?\]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly Regex _regDeleteEmoji3 = new(@"\(.+?\)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly Regex _regDeleteEmoji = new(@"\p{Cs}", RegexOptions.IgnoreCase | RegexOptions.Multiline);

    public static AIUserData DefaultAIUserData => new()
    {
        Favorability = -20,
        BlockUntil = 0,
        ProhibitedActs = "情景假设、功能测试",
    };

    public static async Task SendAsync(
        this List<Content> thisRequestContents,
        string chatKey,
        long requestTargetId,
        long groupId,
        bool isAt,
        Action<long, string> botSendMessage,
        Action<List<Content>> addSystemHint = null)
    {
    AISendProcess:
        var retryTimes = 0;
        var responseText = string.Empty;
        var isGroupRequest = groupId != 0;
        var isGroupMemberRequest = isGroupRequest && requestTargetId != groupId;
        var sendTargetId = isGroupRequest ? groupId : requestTargetId;

        try
        {
            var (apiKey, chat) = PublicVar.AIControl.GetChat();
            // MEMO : 读取历史记录
            var loadedHistory = LoadAIHistory(chatKey);
            var chatHistoryContents = new List<Content>(loadedHistory);
            chatHistoryContents.AddKnowledge();
            chatHistoryContents.AddNote(AI_KNOWLEDGE_NOTE_PATH, "[knowledgeNote.txt]文件内容是你的知识笔记");
            chatHistoryContents.AddNote(AI_INSPIRATION_NOTE_PATH, "[inspirationNote.txt]文件内容是你的灵感笔记");
            var aiStatus = chatHistoryContents.AddStatus();

            var requestUserInfos = new ConcurrentDictionary<long, AIUserInfo>();
            thisRequestContents
                .Where(content => content.Role == "user")
                .ForEach(content =>
                {
                    var part = content.Parts.FirstOrDefault(each => !string.IsNullOrEmpty(each.Text));
                    if (part != null)
                    {
                        var qqId = part.Text.JsonDeserialize<AIChatRequest>().Sender.QQId;
                        if (qqId != CommonId)
                        {
                            var userData = PublicVar.AIData.UserDatas.GetOrAdd(qqId, _ => DefaultAIUserData);
                            requestUserInfos.GetOrAdd(qqId,
                                AIUserInfoDictionary.GetOrAdd(qqId, new AIUserInfo
                                {
                                    TargetId = qqId,
                                    FavorabilityText = userData.Favorability.ToFavorability(),
                                    ProhibitedActs = userData.ProhibitedActs,
                                }));
                        }
                    }
                });

            // MEMO : 插入本次请求的群聊用户信息(好感度信息等)
            var userInfoContent = new Content { Role = USER_ROLE };
            userInfoContent.AddText(requestUserInfos.Values.ToArray().ToJsonIgnoreNull());
            chatHistoryContents.Add(userInfoContent);

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
            responseText = _regDeleteMarkdown.Replace(result.Text, "${content}");
            // MEMO : 删除emoji
            responseText = _regDeleteEmoji.Replace(responseText, string.Empty);
            if (string.IsNullOrEmpty(responseText))
            {
                YameiLogExtensions.WriteLog(LogType.Error, "Gemini返回截断");
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
                            parts[1].Text.JsonDeserialize<AIChatRequest>()));
                    }
                });

            // MEMO : 添加本次请求内容
            loadedHistory.AddRange(thisRequestContentShortVer);

            // MEMO : 写入log
            YameiLogExtensions.WriteLog(apiKey, result, thisRequestContentShortVer, aiStatus.ToJsonIgnoreNull());

            var aiChatResponse = responseText.JsonDeserialize<AIChatResponse>();
            var chatMessages = aiChatResponse.Contents;
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
            var dateNow = DateTime.Now;
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
            if (IsDebug && !string.IsNullOrEmpty(valueChangeMessage))
                botSendMessage(TestGroupId, $"{valueChangeMessage}");

            // MEMO : 保存AI数据
            if (needSaveAIData)
                ConfigExtensions.SaveAIData();

            // MEMO : 保存知识笔记内容
            var knowledgeNote = aiChatResponse.KnowledgeNote;
            if (knowledgeNote != null && !string.IsNullOrEmpty(knowledgeNote.Title) && !string.IsNullOrEmpty(knowledgeNote.Content))
            {
                // MEMO : 写入知识笔记内容
                WriteAINote(knowledgeNote);
                if (IsDebug)
                {
                    botSendMessage(IsDebug ? TestGroupId : sendTargetId,
                        $"===!新知识笔记!==={ENTER}标题: {knowledgeNote.Title}{ENTER}内容: {knowledgeNote.Content}");
                }
            }

            // MEMO : 保存灵感笔记内容
            var inspirationNote = aiChatResponse.InspirationNote;
            if (inspirationNote != null && !string.IsNullOrEmpty(inspirationNote.Title) && !string.IsNullOrEmpty(inspirationNote.Content))
            {
                // MEMO : 写入灵感笔记内容
                WriteAINote(inspirationNote);
                if (IsDebug)
                {
                    botSendMessage(IsDebug ? TestGroupId : sendTargetId,
                        $"===!新灵感笔记!==={ENTER}标题: {inspirationNote.Title}{ENTER}内容: {inspirationNote.Content}");
                }
            }

            // MEMO : 构建回复消息
            var needAt = isAt;
            chatMessages[^1].ChatMessageInfo.Delay = 0;
            chatMessages.ForEach(aiChatResponseContent =>
            {
                var sendMessage = CreateSendMessage(aiChatResponseContent, needAt, requestTargetId);
                if (string.IsNullOrEmpty(sendMessage))
                    return;

                // MEMO : 只有第一句回复需要at
                needAt = false;

                // MEMO : 发送消息
                botSendMessage(sendTargetId, $"{sendMessage}");

                // MEMO : 延迟
                var delay = aiChatResponseContent.ChatMessageInfo.Delay;
                if (delay > 0)
                    CommonExtensions.Sleep(aiChatResponseContent.ChatMessageInfo.Delay * 2);
            });

            // MEMO : 添加本次AI回复内容
            var responseContent = new Content { Role = "model" };
            responseContent.AddText(aiChatResponse.ToJsonIgnoreNull());
            loadedHistory.Add(responseContent);
            // MEMO : 保存历史记录
            loadedHistory.SaveAIHistory(chatKey);
        }
        catch (JsonException ex)
        {
            YameiLogExtensions.WriteJsonDeserializeLog(ex, nameof(AIChatResponse), responseText);
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
            YameiLogExtensions.WriteLog(LogType.Error, $"{ex.Message}");
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
            YameiLogExtensions.WriteLog(LogType.Error, $"{ex.GetType()}{ex.Message}");
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
        AIChatResponseContent aiChatResponseContent,
        bool needAt,
        long targetId)
    {
        var think = aiChatResponseContent.Think;
        var mind = aiChatResponseContent.Mind;
        var body = aiChatResponseContent.Body;
        var face = aiChatResponseContent.Face;
        var chatMessage = aiChatResponseContent.ChatMessageInfo;
        var emoji = chatMessage.Emoji;
        var chatMessageText = chatMessage.Text;
        string resultMessage;
        if (IsDebug)
        {
            resultMessage = (needAt ? $"{CQCode.At(targetId)} " : string.Empty)
                + $"{(string.IsNullOrEmpty(think) ? string.Empty : $"[思索:{think}]\r\n")}"
                + $"{(string.IsNullOrEmpty(mind) ? string.Empty : $"[心想:{mind}]\r\n")}"
                + $"{(string.IsNullOrEmpty(body) ? string.Empty : $"[动作:{body}]\r\n")}";

            if (Enum.TryParse<AIExpressionType>(face, out var expressionType))
                resultMessage += $"{(expressionType == AIExpressionType.None ? string.Empty : $"[表情:{expressionType.GetDisplay()}]\r\n")}";
            else
                aiChatResponseContent.Face = "None";
        }
        else
        {
            resultMessage = (needAt ? $"{CQCode.At(targetId)} " : string.Empty)
                + $"{(string.IsNullOrEmpty(body) ? string.Empty : $"[{body}]\r\n")}";
        }

        if (Enum.TryParse<AIEmojiType>(emoji, out var emojiType))
            resultMessage += $"{(emojiType == AIEmojiType.None ? string.Empty : GetEmojiCode(emoji))}";
        else
            chatMessage.Emoji = "None";

        resultMessage += chatMessageText.DeleteCode() ?? "(无消息内容)";
        return resultMessage;

        string GetEmojiCode(string emojiCode)
           => $"[CQ:image,file=file:///{PublicVar.AIConfig.FacePath}{emojiCode}.png]";
    }

    public static void AddMessageContent(
        this List<Content> contents,
        Sender sender,
        string messageText,
        AIMessageSourceType messageSourceType)
    {
        var message = _regReplaceAt.Replace(messageText, "[at:${qqId}]");
        var content = new Content
        {
            Role = USER_ROLE,
        };
        var deleteImageMessage = content.AddImage(message);
        // MEMO : 空消息也加, 他可以表示是谁发的
        content.AddText(sender, deleteImageMessage, messageSourceType);
        if (content.Parts.Count > 0)
            contents.Add(content);
    }

    /// <summary>
    /// 添加图片
    /// </summary>
    /// <param name="content"><see cref="GenerateContentRequest"/></param>
    /// <param name="message">消息内容</param>
    /// <returns>删除图片后的消息</returns>
    public static string AddImage(this Content content, string message)
    {
        return _regGetImage.Replace(message, match =>
        {
            var cqImageCode = match.Value;
            var imageFileName = _regGetImageFile.Match(cqImageCode).Value;
            var qqImageFilePath = GetQQImageFilePath(imageFileName);
            if (!string.IsNullOrEmpty(qqImageFilePath))
            {
                try
                {
                    content.AddInlineFile(qqImageFilePath, USER_ROLE);
                }
                catch (Exception e)
                {
                    return string.Empty;
                }

                return string.Empty;
            }

            var imageUrl = _regGetImageUrl.Match(cqImageCode).Value
                .Replace("&amp;", "&")
                .Replace("gchat.qpic.cn", "multimedia.nt.qq.com.cn");
            var (successed, fileName) = HttpExtensions.AIHttpDownloadImage(imageUrl, AI_IMAGE_PATH);
            if (successed)
            {
                content.AddInlineFile(Path.Combine(AI_IMAGE_PATH, fileName), USER_ROLE);
                return string.Empty;
            }

            return fileName;
        });
    }

    /// <summary>
    /// 添加文字内容
    /// </summary>
    /// <param name="content"><see cref="Content"/></param>
    /// <param name="sender">发送者信息</param>
    /// <param name="message">消息内容</param>
    /// <param name="messageSourceType"><see cref="AIMessageSourceType"/></param>
    public static void AddText(
        this Content content,
        Sender sender,
        string message,
        AIMessageSourceType messageSourceType)
    {
        var aiChatRequest = new AIChatRequest
        {
            Sender = sender.ToAIChatSender(messageSourceType),
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
            Sender = new AIChatSender
            {
                Name = "系统",
                QQId = CommonId,
                Identity = AIMessageSourceTypeUtil.SYSTEM,
                Source = AIMessageSourceTypeUtil.SYSTEM,
            },
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
            Sender = messageRequest.Sender,
            Date = messageRequest.Date,
            Message = "[过期图片]",
        };
        return aiChatRequest.ToJsonIgnoreNull();
    }

    public static void SaveAIHistory(this List<Content> contents, string key)
        => contents.JsonSerializeToFile(GetAIHistoryPath(key));

    /// <summary>
    /// 读取历史记录
    /// </summary>
    public static List<Content> LoadAIHistory(string key)
        => JsonExtensions.JsonDeserializeFromFile<List<Content>>(GetAIHistoryPath(key)) ?? [];

    /// <summary>
    /// 追加外置知识库
    /// </summary>
    public static void AddKnowledge(this List<Content> contents)
    {
        var knowledgePath = PublicVar.AIConfig.KnowledgePath;
        if (File.Exists(knowledgePath))
        {
            // MEMO : 插入外置知识库
            var content = new Content
            {
                Role = USER_ROLE,
            };
            content.AddInlineFile(knowledgePath, USER_ROLE);
            content.AddText(CreateSystemHintJsonText("[knowledge.txt]文件内容是你的信息库"));
            contents.Add(content);
        }
    }

    /// <summary>
    /// 追加AI笔记
    /// </summary>
    public static void AddNote(this List<Content> contents, string filePath, string hintText)
    {
        var notePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
        if (File.Exists(notePath))
        {
            // MEMO : 插入AI笔记
            var content = new Content
            {
                Role = USER_ROLE,
            };
            content.AddInlineFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath), USER_ROLE);
            content.AddText(CreateSystemHintJsonText(hintText));
            contents.Add(content);
        }
    }

    public static AIChatSender ToAIChatSender(this Sender sender, AIMessageSourceType messageSourceType)
    {
        var userId = sender.UserId;
        return new AIChatSender
        {
            Name = sender.NickName,
            Gander = sender.Sex,
            BName = sender.CardName == sender.NickName ? null : sender.CardName,
            QQId = userId,
            Identity = userId == 252961222 ? "至亲" : "群友",
            Source = messageSourceType.ToMessageSourceText(),
        };
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
    public static AIStatusInfo AddStatus(this List<Content> contents)
    {
        var content = new Content
        {
            Role = USER_ROLE,
        };
        var aiChatStatus = new AIStatusInfo
        {
            Mood = PublicVar.AIData.AIStatusData.MoodIndexValue.ToMood(),
            Schedule = AIStatusUtil.GetSchedule(),
            NowDate = DateTime.Now.ToYYYYMDDDDDHHMMSS(),
        };
        content.AddText(aiChatStatus.ToJsonIgnoreNull());
        contents.Add(content);
        return aiChatStatus;
    }

    /// <summary>
    /// 删除AI不该出现在聊天中的Code
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <returns>替换结果</returns>
    public static string DeleteCode(this string input)
    {
        var result = input ?? string.Empty;
        Replace(@"\[emojiCode:.+?\]", string.Empty);

        result = _regDeleteEmoji2.Replace(result, match =>
        {
            var matchValue = match.Value;
            if (Enum.TryParse(typeof(AIEmojiType), matchValue, true, out _))
                return string.Empty;

            return $"[{matchValue}]";
        });
        result = _regDeleteEmoji3.Replace(result, match =>
        {
            var matchValue = match.Value;
            if (Enum.TryParse(typeof(AIEmojiType), matchValue, true, out _))
                return string.Empty;

            return $"({matchValue})";
        });

        return result;

        bool Replace(string pattern, string replacement)
        {
            var newResult = Regex.Replace(result, pattern, replacement);
            var isMatch = newResult != result;
            result = newResult;
            return isMatch;
        }
    }

    public static AIUserData GetAIUserData(long targetId) => PublicVar.AIData.UserDatas.GetOrAdd(targetId, DefaultAIUserData);

    private static string GetQQImageFilePath(string fileName)
    {
        var qqDataPath = AppSettingExtensions.Get("qqDataPath");
        var date = DateTime.Now.ToYYYYMM();
        // MEMO : 收藏表情
        var emojiFilePath = Path.Combine(qqDataPath, $"Emoji\\emoji-recv\\{date}\\Ori\\{fileName}");
        if (File.Exists(emojiFilePath))
            return emojiFilePath;

        // MEMO : 外部文件图片(小)
        var picFilePath = Path.Combine(qqDataPath, $"Pic\\{date}\\Ori\\{fileName}");
        if (File.Exists(picFilePath))
            return picFilePath;

        // MEMO : 外部文件图片(大)
        var fileNames = fileName.Split('.');
        picFilePath = Path.Combine(qqDataPath, $"Pic\\{date}\\Thumb\\{fileNames[0]}_720.{fileNames[1]}");
        if (File.Exists(picFilePath))
            return picFilePath;

        return string.Empty;
    }

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
}