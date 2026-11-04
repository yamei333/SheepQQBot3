using System;
using OpenAI.Chat;
using SheepQQBot3.Model.AI;

namespace SheepQQBot3.Extensions;

public static partial class AIExtensions
{
    private const string FUNCTION_NAME_REPLY_USER = "reply_user";
    private const string FUNCTION_DESC_REPLY_USER = "Call this function to send a response to the user.";

    private const string FUNCTION_NAME_REPLY_GROUPCHAT_SUMMARY = "reply_groupchat_summary";
    private const string FUNCTION_DESC_REPLY_GROUPCHAT_SUMMARY = "Call this function to send a group chat summary to the user.";

    private const string FUNCTION_NAME_WEB_SEARCH = "web_search";
    private const string FUNCTION_DESC_WEB_SEARCH
        = "Search the web for up-to-date information, real-time news, niche knowledge, verifying facts, or looking up details not found in training data.";

    private static readonly ChatTool _chatTool = ChatTool.CreateFunctionTool(
        FUNCTION_NAME_REPLY_USER,
        FUNCTION_DESC_REPLY_USER,
        GetChatToolData<AIChatResponse>());

    private static readonly ChatTool _groupSummaryTool = ChatTool.CreateFunctionTool(
        FUNCTION_NAME_REPLY_GROUPCHAT_SUMMARY,
        FUNCTION_DESC_REPLY_GROUPCHAT_SUMMARY,
        GetChatToolData<AIGroupChatSummaryResponse>());

    private static readonly ChatTool _webSearchTool = ChatTool.CreateFunctionTool(
        FUNCTION_NAME_WEB_SEARCH,
        FUNCTION_DESC_WEB_SEARCH,
        GetChatToolData<AIWebSearchArgs>());

    private static BinaryData GetChatToolData<T>()
        => BinaryData.FromString(JsonSchemaGenerator.Generate(typeof(T)));
}