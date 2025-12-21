using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Enums;
using System;
using System.Threading.Tasks;
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
        var targetId = privateMessage.Sender.UserId.ToString();
        var groupId = privateMessage.Sender.GroupId.ToString();
        var messageId = privateMessage.MessageId;
        var message = privateMessage.Message;
        // MEMO : 命令格式检查
        if (!message.StartsWith(COMMAND_CONFIG, StringComparison.CurrentCultureIgnoreCase))
            return false;

        var contentMessage = message[COMMAND_CONFIG.Length..];
        if (contentMessage.StartsWith(COMMAND_CONFIG_BARK, StringComparison.CurrentCultureIgnoreCase))
        {
            var barkKey = contentMessage[COMMAND_CONFIG_BARK.Length..];
            GlobalBotConfig.UserConfigs.TryAdd(targetId, []);
            var isUpdate = GlobalBotConfig.UserConfigs[targetId].ContainsKey(UserConfigType.BarkKey);
            GlobalBotConfig.UserConfigs[targetId][UserConfigType.BarkKey] = barkKey;
            ConfigExtensions.SaveConfig();
            await GlobalBotClient.SendPrivateMessageAsync(targetId, groupId,
                $"BarkKey已{(isUpdate ? "更新" : "配置")}")
                .ConfigureAwait(false);
            return true;
        }

        await GlobalBotClient.SendPrivateMessageAsync(targetId, groupId, "命令格式有误!")
            .ConfigureAwait(false);
        // 暂不处理
        return true;
    }
}