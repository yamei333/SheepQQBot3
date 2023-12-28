using System;
using System.Threading.Tasks;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Extension;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Private;

public static partial class ProcessPrivateMessage
{
    /// <summary>
    /// ADMIN命令
    /// </summary>
    private const string COMMAND_ADMIN = "#ADMIN#";

    /// <summary>
    /// 取得IP
    /// </summary>
    private const string COMMAND_ADMIN_IP = "IP";

    /// <summary>
    /// Admin功能
    /// </summary>
    public static async Task<bool> AdminCommandAsync(PrivateMessage privateMessage)
    {
        var targetId = privateMessage.Sender.UserId;
        var groupId = privateMessage.Sender.GroupId;
        var message = privateMessage.Message;
        // MEMO : 命令格式检查
        if (!message.StartsWith(COMMAND_ADMIN, StringComparison.CurrentCultureIgnoreCase))
            return false;

        var contentMessage = message[COMMAND_ADMIN.Length..];
        if (contentMessage.StartsWith(COMMAND_ADMIN_IP, StringComparison.CurrentCultureIgnoreCase))
        {
            var ip = await HttpExtensions.GetIPAddressAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(ip))
            {
                await Api.SendPrivateMessageAsync(targetId, groupId, "IP取得失败!").ConfigureAwait(false);
                return true;
            }

            await Api.SendPrivateMessageAsync(targetId, groupId, $"IP地址: {ip}").ConfigureAwait(false);
            return true;
        }

        await Api.SendPrivateMessageAsync(targetId, groupId, "命令格式有误!")
            .ConfigureAwait(false);
        // 暂不处理
        return true;
    }
}