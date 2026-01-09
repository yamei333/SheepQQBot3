using CommonLibrary;
using Masuit.Tools;
using Masuit.Tools.Systems;
using OpenAI.Chat;
using SheepQQBot3.Enums;
using SheepQQBot3.Model;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using System;
using System.ClientModel;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.Extensions;

public static partial class AIExtensions
{
    private const string FUNCTION_NAME_REPLY_USER = "reply_user";
    private const string FUNCTION_DESC_REPLY_USER = "Call this function to send a response to the user.";

    public const string ERROR_MESSAGE = "我脑袋好像短路了！";
    public const string ERROR_REASON = "(原因: {0})";
    public const string ERROR_JSON_ERROR = "Json解析失败";
    public const string AI_HISTORY_PATH = "AICache/History/";

    private const string SENDING_GEMINI_REQUEST = "正在发送哈基米请求";

    public const string MESSAGE_BUSY = "我...现在正忙!...不太方便!...";
    public const string MESSAGE_SLEEP = "Zzz...";
    public const string SEND_SOME_IMAGES = "(发送了图片)";

    private const string AI_KNOWLEDGE_PATH = "AICache/knowledge.txt";
    private const string AI_KNOWLEDGE_NOTE_PATH = "AICache/knowledgeNote.txt";
    private const string AI_INSPIRATION_NOTE_PATH = "AICache/inspirationNote.txt";

    private const int MAX_IMAGE_CONTENT_LIMIT = 5;
    private const string IMAGE_DUPLICATE = "[重复的图片]";
    private const string IMAGE_EXPIRED = "[历史图片已折叠]";

    private static readonly Regex _regReplaceAt = new(@"\[CQ:at,qq=(?<qqId>\d+)\] ", RegexOptions.IgnoreCase);
    private static readonly Regex _regCQImageFileUrl = RegexGenerator.CQImageFileUrl();
    private static readonly Regex _reg3LevelJson = new(@"\{([^{}]|\{([^{}]|\{[^{}]*\})*\})*\}");
    private static readonly Regex _regDeleteEmoji = new(@"\p{Cs}");

    public static async Task SendAsync(
        this List<ChatMessageContentPart> thisRequestParts,
        string chatKey,
        string requestTargetId,
        string groupId,
        bool isAt,
        ConcurrentDictionary<string, AIChatSender> aiChatSenderInfos,
        AIGroupConfig aiGroupConfig,
        Action<string, string> botSendMessage,
        AIModel model,
        AIRequestType aiRequestType,
        string extraSystemHint = null)
    {
        var retryTimes = 0;
        var isGroupRequest = !groupId.IsNullOrEmpty();
        var isGroupAt = isGroupRequest && requestTargetId != groupId;
        var sendTargetId = isGroupRequest ? groupId : requestTargetId;
        var isGroupChatSummary = chatKey.StartsWith("z");

        #region 预处理用户好感度等信息(不存在则追加)

        // MEMO : 替换信息
        var requestUserInfos = new ConcurrentDictionary<string, AIUserInfo>();
        for (var i = 0; i < thisRequestParts.Count; i++)
        {
            var part = thisRequestParts[i];
            if (part.Kind == ChatMessageContentPartKind.Text)
            {
                var aiChatRequest = part.Text.FromJson<AIChatRequest>();
                var qqId = aiChatRequest.SenderId;
                // MEMO : 清空SenderId
                aiChatRequest.SenderId = null;
                thisRequestParts[i] = ChatMessageContentPart.CreateTextPart(aiChatRequest.ToJsonIgnoreNull());
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
        // MEMO : 角色设计
        var systemMessageContents = new List<ChatMessageContentPart>
        {
            ChatMessageContentPart.CreateTextPart(GlobalAICharacter.SystemInstructionText),
        };
        // MEMO : 知识库
        systemMessageContents.AddKnowledge();
        // MEMO : 助手哈莉状态
        var aiStatus = await systemMessageContents.AddStatusAsync(
                isGroupRequest ? AIMessageSourceType.Group : AIMessageSourceType.Private,
                sendTargetId)
            .ConfigureAwait(false);
        // MEMO : 本次请求的用户信息
        if (requestUserInfos.Any())
            systemMessageContents.Add(ChatMessageContentPart.CreateTextPart(requestUserInfos.Values.ToJsonIgnoreNull()));
        var systemMessage = ChatMessage.CreateSystemMessage(systemMessageContents);

        #endregion 构建系统信息

        #region 构建本次发送信息

        List<ChatMessage> thisRequestMessagePrepare = [systemMessage];
        // MEMO : 历史记录
        var loadedHistories = LoadAIHistory(chatKey);
        thisRequestMessagePrepare.AddRange(loadedHistories);

        #endregion 构建本次发送信息

        #region DEBUG响应, 正在发送哈基米请求

#if DEBUG
        if (!isGroupRequest)
        {
            botSendMessage(sendTargetId, $"{SENDING_GEMINI_REQUEST}...");
        }
        else
        {
            botSendMessage(TestGroupId, isGroupAt
                ? $"{SENDING_GEMINI_REQUEST}(群:{groupId}/群友:{requestTargetId})..."
                : $"{SENDING_GEMINI_REQUEST}(群:{groupId})...");
        }
#endif

        #endregion DEBUG响应, 正在发送哈基米请求

        // MEMO : 保存最后出错的错误信息
        var lastErrorMessage = string.Empty;
        while (retryTimes <= AI_MAX_RETRY_TIMES)
        {
            try
            {
                var thisRequestMessages = new List<ChatMessage>(thisRequestMessagePrepare);
                thisRequestParts.ProcessImageContentParts();
                thisRequestMessages.Add(ChatMessage.CreateUserMessage(thisRequestParts));
                // MEMO : 系统提示
                if (!extraSystemHint.IsNullOrEmpty())
                    thisRequestMessages.Add(ChatMessage.CreateUserMessage(extraSystemHint));

                if (IsDebug && retryTimes > 0)
                    botSendMessage(TestGroupId, $"重新发送AI请求中...{retryTimes}");

                switch (aiRequestType)
                {
                    case AIRequestType.Chat:
                        var chatCompletion = await ChatRequestAsync(thisRequestParts, thisRequestMessages, loadedHistories, botSendMessage,
                                aiGroupConfig, isGroupRequest, requestTargetId, sendTargetId, chatKey, isAt)
                            .ConfigureAwait(false);
                        LogExtensions.AddRunLog(new RunLog_AIRequest(sendTargetId, isGroupRequest, chatCompletion.Usage));
                        YameiLogExtensions.WriteLog(chatCompletion, thisRequestParts, aiStatus.ToJsonIgnoreNull());
                        return;
                    case AIRequestType.Image:
                        await ImageRequestAsync(thisRequestParts, thisRequestMessages, loadedHistories,
                                botSendMessage, isGroupRequest, requestTargetId, sendTargetId, chatKey, isAt)
                            .ConfigureAwait(false);
                        return;
                    case AIRequestType.ChatSummary:
                        await GroupSummaryRequestAsync(thisRequestParts, thisRequestMessages,
                                aiGroupConfig, requestTargetId, sendTargetId)
                            .ConfigureAwait(false);
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(aiRequestType), aiRequestType, null);
                }
            }
            catch (AIException ex)
            {
                retryTimes++;
                lastErrorMessage = ex.Message;
                YameiLogExtensions.WriteLog(LogType.Error, $"[GeminiError-{ex.GetType()}]{ex.Message}{ENTER}返回内容: {ex.ResponseText}");
#if DEBUG
                botSendMessage(isGroupRequest ? TestGroupId : sendTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ERROR_JSON_ERROR)}");
#endif
            }
            catch (AIJsonException ex)
            {
                #region Json转换失败处理

                retryTimes++;
                lastErrorMessage = ex.Message;
                YameiLogExtensions.WriteJsonDeserializeLog(ex, nameof(AIChatResponse), ex.JsonText);
#if DEBUG
                botSendMessage(isGroupRequest ? TestGroupId : sendTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(ERROR_JSON_ERROR)}");
#endif

                #endregion Json转换失败处理
            }
            catch (ClientResultException ex)
            {
                #region 请求返回报错处理

                lastErrorMessage = ex.Message;
                YameiLogExtensions.WriteLog(LogType.Error, $"[GeminiError-{ex.GetType()}]{lastErrorMessage}{ENTER}请求内容: {thisRequestParts.ToJsonIgnoreNull()}");
#if DEBUG
                botSendMessage(isGroupRequest ? TestGroupId : sendTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(lastErrorMessage)}");
#endif
                // MEMO : QQ图片有有效期, 过期了则部分/所有图片不解析
                if (lastErrorMessage.Contains("mime type is not supported by Gemini", StringComparison.CurrentCultureIgnoreCase))
                {
                    // MEMO : 第1次重试只解析最后2张图片
                    if (retryTimes == 0)
                        thisRequestParts.ProcessImageContentParts(2);
                    // MEMO : 第2次重试不解析所有图片
                    else if (retryTimes == 1)
                        thisRequestParts.DeleteExpireImage();
                }

                retryTimes++;

                #endregion 请求返回报错处理
            }
            catch (Exception ex)
            {
                #region 其他错误处理

                retryTimes++;
                lastErrorMessage = ex.Message;
                YameiLogExtensions.WriteLog(LogType.Error, $"[GeminiError-{ex.GetType()}]{lastErrorMessage}{ENTER}请求内容: {thisRequestParts.ToJsonIgnoreNull()}");
#if DEBUG
                botSendMessage(isGroupRequest ? TestGroupId : sendTargetId, $"{ERROR_MESSAGE}{ERROR_REASON.CultureFormat(lastErrorMessage)}");
#endif

                #endregion 其他错误处理
            }
        }

        if (isGroupRequest)
        {
            if (isGroupAt)
                botSendMessage(sendTargetId, $"{CQCode.At(requestTargetId)} 哈基米请求失败! 重试次数超过限制!{ENTER}{lastErrorMessage}");
        }
        else
        {
            botSendMessage(sendTargetId, $"哈基米请求失败! 重试次数超过限制!{ENTER}{lastErrorMessage}");
        }
    }

    // 删除过期图片信息
    private static void DeleteExpireImage(this List<ChatMessageContentPart> parts)
    {
        if (parts == null || parts.Count == 0) return;

        // 缓存当前上下文的名字，初始为默认值
        string currentNickName = "未知用户";

        // 倒序遍历
        for (var i = parts.Count - 1; i >= 0; i--)
        {
            var part = parts[i];

            // 1. 如果是文本：尝试提取名字，更新上下文
            if (part.Kind == ChatMessageContentPartKind.Text)
            {
                if (string.IsNullOrEmpty(part.Text))
                    continue;

                try
                {
                    // 解析 JSON 获取名字
                    // 这里的 try-catch 保证即使某条消息格式错误，也不影响整体清理流程
                    var aiChatRequest = part.Text.FromJson<AIChatRequest>();
                    if (!string.IsNullOrEmpty(aiChatRequest?.NickName))
                    {
                        currentNickName = aiChatRequest.NickName;
                    }
                }
                catch
                {
                    // 解析失败忽略，继续使用上一个有效的名字
                }
            }
            // 2. 如果是图片：直接替换，使用手里拿着的最新（其实是后面）的名字
            else if (part.Kind == ChatMessageContentPartKind.Image)
            {
                parts[i] = CreateTextPart(currentNickName, IMAGE_EXPIRED);
            }
        }
    }

    // MEMO : 重复图片处理
    private static void ProcessImageContentParts(this List<ChatMessageContentPart> parts, int maxImageLimit = MAX_IMAGE_CONTENT_LIMIT)
    {
        if (parts == null || parts.Count == 0) return;

        try
        {
            // 记录已遇到的唯一图片 URL (用于去重)
            var seenImageUrls = new HashSet<string>();

            // 缓存当前上下文的 NickName
            // 初始值设为 "未知用户"，防止列表末尾全是图片、没有文本的情况
            string currentContextNickName = "未知用户";

            // 计数器：记录我们从后往前保留了多少张有效图片
            int validImageCount = 0;

            // [关键]倒序遍历：从最后一个元素往回走
            for (var i = parts.Count - 1; i >= 0; i--)
            {
                var part = parts[i];

                // ---------------
                // 1. 处理文本部分
                // ---------------
                if (part.Kind == ChatMessageContentPartKind.Text)
                {
                    try
                    {
                        // 只有遇到文本时才解析 JSON，且每个文本只解析一次！
                        // 相比旧代码，极大地减少了 FromJson 的调用次数
                        if (!string.IsNullOrEmpty(part.Text))
                        {
                            var aiChatRequest = part.Text.FromJson<AIChatRequest>();
                            if (!string.IsNullOrEmpty(aiChatRequest?.NickName))
                            {
                                // 更新当前上下文的名字，供前面的图片使用
                                currentContextNickName = aiChatRequest.NickName;
                            }
                        }
                    }
                    catch
                    {
                        // 容错：JSON 解析失败时，保持沿用上一个有效的名字
                    }
                    continue;
                }

                // ---------------
                // 2. 处理图片部分
                // ---------------
                if (part.Kind == ChatMessageContentPartKind.Image)
                {
                    var url = part.ImageUri?.AbsoluteUri;

                    // 防御性检查
                    if (string.IsNullOrEmpty(url)) continue;

                    if (!seenImageUrls.Add(url))
                    {
                        // [重复图片逻辑]
                        // 因为是倒序，既然 Set 里已经有了，说明后面（未来）已经出现过这张图
                        // 所以当前这张（前面的）是重复的
                        parts[i] = CreateTextPart(currentContextNickName, IMAGE_DUPLICATE);
                    }
                    else
                    {
                        // 这是一个新的（相对于倒序视角）图片
                        validImageCount++;

                        // [过期图片逻辑]
                        // 如果有效图片数量已经超过限制，剩下的（前面的）都是过期
                        if (validImageCount > maxImageLimit)
                        {
                            parts[i] = CreateTextPart(currentContextNickName, IMAGE_EXPIRED);
                        }
                        // 否则：正常保留，不需要做任何操作
                    }
                }
            }
        }
        catch (Exception ex)
        {
            YameiLogExtensions.WriteJsonSerializeLog(ex, "ProcessImageContentParts.originalParts", parts);
            throw;
        }
    }

    private static string CreateSendMessage(
        AIGroupConfig aiGroupConfig,
        AIChatResponseContent content,
        bool needAt,
        string targetId,
        bool isGroupRequest)
    {
        var think = content.Think;
        var body = content.Body;
        var sensory = content.Sensory;
        var mind = content.Mind;
        var face = content.Face;
        string resultMessage;
        if (IsDebug)
        {
            resultMessage = $"{(think.IsNullOrEmpty() ? string.Empty : $"[思考:{think}]{ENTER}")}"
                + $"{(sensory.IsNullOrEmpty() ? string.Empty : $"[感受:{sensory}]{ENTER}")}"
                + $"{(mind.IsNullOrEmpty() ? string.Empty : $"[心想:{mind}]{ENTER}")}"
                + GetExpressionText(true)
                + $"{(body.IsNullOrEmpty() ? string.Empty : $"[动作:{body}]{ENTER}")}";

            resultMessage += content.DeleteCode(true, needAt, targetId);
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

                resultMessage += content.DeleteCode(aiGroupConfig.ShowEmojiImage, needAt, targetId);
            }
            else
            {
                if (targetId == SuperAdminId)
                {
                    resultMessage += string.Empty
                        + $"{(think.IsNullOrEmpty() ? string.Empty : $"[思考:{think}]{ENTER}")}"
                        + $"{(sensory.IsNullOrEmpty() ? string.Empty : $"[感受:{sensory}]{ENTER}")}"
                        + $"{(mind.IsNullOrEmpty() ? string.Empty : $"[心想:{mind}]{ENTER}")}"
                        + $"{(body.IsNullOrEmpty() ? string.Empty : $"[动作:{body}]{ENTER}")}"
                        + GetExpressionText(true);
                }
                else
                {
                    resultMessage += $"{GetExpressionText(false)}{(body.IsNullOrEmpty() ? string.Empty : $"[{body}]{ENTER}")}";
                }

                resultMessage += content.DeleteCode(true, false);
            }
        }

        return resultMessage;

        string GetExpressionText(bool useTitle) => face == AIExpressionType.None
            ? string.Empty
            : $"[{(useTitle ? "表情:" : string.Empty)}{face.GetDisplay()}]{(useTitle ? ENTER : string.Empty)}";
    }

    private static string GetEmojiCode(AIChatResponseContent content, bool showEmojiImage)
    {
        if (!showEmojiImage)
        {
            content.Emoji = null;
            return string.Empty;
        }

        var emoji = content.Emoji;
        return emoji is null or AIEmojiType.None
            ? string.Empty
            : CQCode.Image(Path.Combine(GlobalAIConfig.FacePath, $"{emoji.ToString()}.gif")/*, summary: emoji.GetDisplay()*/);
    }

    /// <param name="contentParts"><see cref="List{ContentPart}"/></param>
    extension(List<ChatMessageContentPart> contentParts)
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

            contentParts.Add(ChatMessageContentPart.CreateTextPart(aiChatStatus.ToJsonIgnoreNull()));
            return aiChatStatus;
        }

        public async Task AddQQChatMessageAsync(
            AIChatSender sender,
            string messageText,
            Dictionary<string, GroupMember> groupMembers,
            bool imageToText = false,
            int imageNumLimit = 0)
        {
            if (messageText.IsNullOrEmpty())
                return;

            var message = ReplaceAt(groupMembers, messageText);
            var deleteImageJsonText = await contentParts.AddQQChatImageAsync(sender, message, imageToText, imageNumLimit).ConfigureAwait(false);
            if (!deleteImageJsonText.IsNullOrEmpty())
                contentParts.AddQQChatTextContent(sender, WebUtility.HtmlDecode(deleteImageJsonText));
        }

        private static string ReplaceAt(Dictionary<string, GroupMember> groupMembers, string message)
        {
            if (groupMembers?.Any() != true)
                return message;

            return _regReplaceAt.Replace(message, match =>
            {
                var qqId = long.Parse(match.Groups["qqId"].Value).ToString();
                if (IsDebug && qqId == SuperAdminId)
                    return $"[at:{BOT_NAME}]";

                return $"[at:{groupMembers[qqId].ToAIChatSender(AIUserInfos).NickName}]";
            });
        }

        public Task AddQQChatMessageAsync(Sender sender, string messageText, Dictionary<string, GroupMember> groupMembers,
            bool imageToText = false, int imageNumLimit = 0)
            => AddQQChatMessageAsync(contentParts, sender.ToAIChatSender(AIUserInfos), messageText, groupMembers, imageToText, imageNumLimit);

        /// <summary>
        /// 添加图片
        /// </summary>
        /// <param name="sender">发送者信息</param>
        /// <param name="messageText">消息内容</param>
        /// <param name="imageToText"></param>
        /// <param name="imageNumLimit">解析图片上限</param>
        /// <returns>删除图片后的消息</returns>
        private async Task<string> AddQQChatImageAsync(
            AIChatSender sender,
            string messageText,
            bool imageToText = false,
            int imageNumLimit = 0)
        {
            if (messageText.IsNullOrEmpty())
                return string.Empty;

            var processedMessage = messageText;
            var isAddImage = false;
            var matches = _regCQImageFileUrl.Matches(messageText);
            // MEMO : QQ群at+图片时, 图片数量>2会丢失at指令
            if (imageToText || (imageNumLimit > 0 && matches.Count > imageNumLimit))
            {
                var noImageContents = new List<ChatMessageContentPart>();
                noImageContents.AddQQChatTextContent(sender, IMAGE_EXPIRED);
                contentParts.AddRange(noImageContents);
                return _regCQImageFileUrl.Replace(messageText, string.Empty);
            }

            var thisContentParts = new List<ChatMessageContentPart>();
            await matches.ForeachAsync(async match =>
            {
                var file = WebUtility.HtmlDecode(match.Groups["file"].Value);
                var imageReceiveData = await GlobalBotClient.GetImageAsync(file).ConfigureAwait(false);
                if (imageReceiveData.IsSuccessed)
                {
                    if (ImageExtensions.IsGifFile(imageReceiveData.Data.File))
                    {
                        thisContentParts.AddQQChatTextContent(sender, $"[GIF动图]");
                    }
                    else
                    {
                        isAddImage = true;
                        thisContentParts.Add(ChatMessageContentPart.CreateImagePart(new Uri(imageReceiveData.Data.Url)));
                    }
                }
                else
                {
                    thisContentParts.AddQQChatTextContent(sender, $"[加载失败图片]");
                }

                processedMessage = processedMessage.Replace(match.Value, string.Empty);
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
            contentParts.Add(ChatMessageContentPart.CreateTextPart(contentPart.ToJsonIgnoreNull()));
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
                AddNote(AI_KNOWLEDGE_NOTE_PATH, $"# 知识笔记{ENTER}{ENTER}");
                AddNote(AI_INSPIRATION_NOTE_PATH, $"# 灵感笔记{ENTER}{ENTER}");

                void AddNote(string filePath, string noteTitle = "")
                {
                    if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath)))
                        return;

                    contentParts.Add(ChatMessageContentPart.CreateTextPart(noteTitle
                        + File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath), Encoding.UTF8)));
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
            contentParts.Add(ChatMessageContentPart.CreateTextPart(contentPart.ToJsonIgnoreNull()));
        }
    }

    public static void SaveAIHistory(this List<ChatMessage> messages, string key)
        => ChatHistorySerializer.Save(messages, GetAIHistoryPath(key));

    /// <summary>
    /// 读取历史记录
    /// </summary>
    public static List<ChatMessage> LoadAIHistory(string key)
        => ChatHistorySerializer.Load(GetAIHistoryPath(key));

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
    /// <param name="aiChatResponseContent"><see cref="AIChatMessage"/></param>
    /// <param name="showEmojiImage">是否显示表示</param>
    /// <param name="needAt">是否文字开头加at</param>
    /// <param name="targetId">at对象QQ号</param>
    /// <returns>替换结果</returns>
    public static string DeleteCode(this AIChatResponseContent aiChatResponseContent, bool showEmojiImage, bool needAt, string targetId = "")
    {
        var result = aiChatResponseContent.Text ?? string.Empty;
        return GetEmojiCode(aiChatResponseContent, showEmojiImage) + (needAt ? $"{CQCode.At(targetId)} " : string.Empty) + result;
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

    private static ChatMessageContentPart CreateTextPart(string userName, string message)
        => ChatMessageContentPart.CreateTextPart(new AIChatRequest
        {
            NickName = userName,
            Message = message,
        }.ToJsonIgnoreNull());
}