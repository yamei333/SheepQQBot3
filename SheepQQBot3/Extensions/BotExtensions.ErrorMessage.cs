using SheepQQBot3.Model.Extension;

namespace SheepQQBot3.Extensions;

public static partial class BotExtensions
{
    /// <summary>
    /// 生成错误消息(命令格式错误)
    /// </summary>
    /// <param name="targetId"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    public static string GetMessage_CommandTypeError(string targetId, string messageId)
        => GetMessage_CommonError(targetId, messageId, "命令格式错误!");

    /// <summary>
    /// 生成错误消息(参数范围错误)
    /// </summary>
    /// <param name="targetId"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    public static string GetMessage_ParameterRangeError(string targetId, string messageId)
        => GetMessage_CommonError(targetId, messageId, "参数范围错误!");

    /// <summary>
    /// 生成错误消息(该命令仅管理可用)
    /// </summary>
    /// <param name="targetId"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    public static string GetMessage_CanOnlyAdminUseError(string targetId, string messageId)
        => GetMessage_CommonError(targetId, messageId, "该命令仅管理可用!");

    /// <summary>
    /// 生成错误消息(自定义消息)
    /// </summary>
    /// <param name="targetId"></param>
    /// <param name="messageId"></param>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public static string GetMessage_CommonError(string targetId, string messageId, string errorMessage)
        => $"{CQCode.Reply(targetId, messageId)}{errorMessage}";
}