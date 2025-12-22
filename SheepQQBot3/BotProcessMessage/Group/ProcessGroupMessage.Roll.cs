using Masuit.Tools;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Group;

public static partial class ProcessGroupMessage
{
    /// <summary>
    /// Roll点命令开头
    /// </summary>
    private const string COMMAND_ROLL = "#R#";

    /// <summary>
    /// Roll点命令(简化)
    /// </summary>
    private const string COMMAND_ROLL_SIMPLE = "R";

    /// <summary>
    /// Roll点
    /// </summary>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    /// <returns></returns>
    public static async Task RollAsync(GroupMessage groupMessage)
    {
        var groupId = groupMessage.GroupId;
        var targetId = groupMessage.Sender.UserId.ToString();
        var messageId = groupMessage.MessageId;
        var message = groupMessage.Message;
        // MEMO : 简单命令r
        if (message.Equals(COMMAND_ROLL_SIMPLE, StringComparison.CurrentCultureIgnoreCase))
            message = COMMAND_ROLL;

        // MEMO : 命令格式检查
        if (!message.StartsWith(COMMAND_ROLL, StringComparison.CurrentCultureIgnoreCase))
            return;

        var contentMessage = message[3..];
        if (contentMessage.IsNullOrEmpty())
        {
            await SendRollResult(100).ConfigureAwait(false);
            return;
        }

        var regNumber = new Regex(@"\d+");
        if (regNumber.IsMatch(contentMessage))
        {
            await SendRollResult(int.Parse(contentMessage)).ConfigureAwait(false);
        }
        else
        {
            await GlobalBotClient.SendGroupMessageAsync(groupId, BotExtensions.GetMessage_CommandTypeError(targetId, messageId))
                .ConfigureAwait(false);
        }

        // 无匹配结果,或API超过使用次数限制
        // 暂不处理
        return;

        Task SendRollResult(int maxRollNumber)
            => GlobalBotClient.SendGroupMessageAsync(groupId,
                $"[{groupMessage.Sender.CardName}]的Roll点结果 {Rand.Next(maxRollNumber) + 1}");
    }
}