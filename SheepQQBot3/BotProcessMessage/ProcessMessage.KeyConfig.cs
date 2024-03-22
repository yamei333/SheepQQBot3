using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Enums;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage;

public static partial class ProcessMessage
{
    /// <summary>
    /// KEY配置命令开头
    /// </summary>
    private const string COMMAND_CONFIG = "#CFG#";

    /// <summary>
    /// BARK配置
    /// </summary>
    private const string COMMAND_CONFIG_BARK = "BARK#";

    /// <summary>
    /// KEY配置
    /// </summary>
    public static async Task<bool> KeyConfigAsync(PrivateMessage privateMessage)
    {
        var targetId = privateMessage.Sender.UserId;
        var groupId = privateMessage.Sender.GroupId;
        var messageId = privateMessage.MessageId;
        var message = privateMessage.Message;
        // MEMO : 命令格式检查
        if (!message.StartsWith(COMMAND_CONFIG, StringComparison.CurrentCultureIgnoreCase))
            return false;

        var contentMessage = message[COMMAND_CONFIG.Length..];
        if (contentMessage.StartsWith(COMMAND_CONFIG_BARK, StringComparison.CurrentCultureIgnoreCase))
        {
            var barkKey = contentMessage[COMMAND_CONFIG_BARK.Length..];
            BotConfig.UserConfigs.TryAdd(targetId, new Dictionary<UserConfigType, string>());
            var isUpdate = BotConfig.UserConfigs[targetId].ContainsKey(UserConfigType.BarkKey);
            BotConfig.UserConfigs[targetId][UserConfigType.BarkKey] = barkKey;
            ConfigExtensions.SaveConfig();
            await BotServer.SendPrivateMessageAsync(targetId, groupId,
                $"BarkKey已{(isUpdate ? "更新" : "配置")}")
                .ConfigureAwait(false);
            return true;
        }

        await BotServer.SendPrivateMessageAsync(targetId, groupId, "命令格式有误!")
            .ConfigureAwait(false);
        // 暂不处理
        return true;
    }
}