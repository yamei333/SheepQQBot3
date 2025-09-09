using Masuit.Tools;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.AI;
using SheepQQBot3.Model.Extension;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static SheepQQBot3.Extensions.AIExtensions;
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
    /// 取得has剩余流量
    /// </summary>
    private const string COMMAND_ADMIN_HAS = "HAS";

    /// <summary>
    /// 取得AI当前状态
    /// </summary>
    private const string COMMAND_ADMIN_AI = "AI";

    /// <summary>
    /// 重置AI状态
    /// </summary>
    private const string COMMAND_ADMIN_AI_RESET = "AIRESET";

    /// <summary>
    /// 重置AI状态(所有)
    /// </summary>
    private const string COMMAND_ADMIN_AI_RESET_ALL = "AIRESETALL";

    /// <summary>
    /// 清除AI记忆
    /// </summary>
    private const string COMMAND_ADMIN_AI_CLEAR = "AICLEAR";

    /// <summary>
    /// Admin功能
    /// </summary>
    public static async Task<bool> AdminCommandAsync(PrivateMessage privateMessage)
    {
        var senderId = privateMessage.Sender.UserId;
        // MEMO : 通过群进行临时私聊时会有此值
        var groupId = privateMessage.Sender.GroupId;
        var message = privateMessage.Message;
        // MEMO : 命令格式检查
        if (!message.StartsWith(COMMAND_ADMIN, StringComparison.CurrentCultureIgnoreCase))
            return false;

        var contentMessage = message[COMMAND_ADMIN.Length..];
        switch (contentMessage.ToUpper())
        {
            case COMMAND_ADMIN_IP:
                if (!RouterExtension.TryGetIPAddress(out var ipResult))
                {
                    await BotServer.SendPrivateMessageAsync(senderId, groupId, $"IP取得失败!{ENTER}原因: {ipResult}").ConfigureAwait(false);
                    return true;
                }

                if (ipResult.IsNullOrEmpty())
                {
                    await BotServer.SendPrivateMessageAsync(senderId, groupId, $"IP地址为空! 请检查路由是否正确拨号").ConfigureAwait(false);
                    return true;
                }

                await BotServer.SendPrivateMessageAsync(senderId, groupId, $"IP地址: {ipResult}").ConfigureAwait(false);
                break;
            //case COMMAND_ADMIN_HAS:
            //    if (!RouterExtension.TryGetClashInfo(out var clashInfoResult, out var remainBand, out var resetDaysLeft, out var expireDate))
            //    {
            //        await BotServer.SendPrivateMessageAsync(senderId, groupId, $"Clash情报取得失败!{ENTER}原因: {clashInfoResult}").ConfigureAwait(false);
            //        return true;
            //    }

            //    var today = DateTime.Today;
            //    var nextResetDate = today.AddDays(resetDaysLeft);
            //    var hasMessage = $"流量剩余: {remainBand:0.0} GB{ENTER}重置日期: {nextResetDate.Day}号{ENTER}到期时间: {expireDate.ToYYYYMMDD()}"
            //        + $"{ENTER}每天还能高强度使用 {remainBand / (resetDaysLeft + 1):0.0} GB!";
            //    await BotServer.SendPrivateMessageAsync(senderId, groupId, hasMessage).ConfigureAwait(true);
            //    break;
            case COMMAND_ADMIN_AI:
                var moodIndex = PublicVar.AIData.AIStatusData.MoodIndexValue;
                var aiStatusMessage = $"===={BOT_NICK_NAME}状态===={ENTER}"
                    + $"当前日程: {AIStatusUtil.GetSchedule()}{ENTER}"
                    + $"心情指数: ({moodIndex}){moodIndex.ToMood()}";
                await BotServer.SendPrivateMessageAsync(senderId, groupId, aiStatusMessage).ConfigureAwait(true);
                break;
            case COMMAND_ADMIN_AI_RESET:
                await ResetAIStatus().ConfigureAwait(false);
                break;
            case COMMAND_ADMIN_AI_RESET_ALL:
                await ResetAIStatus().ConfigureAwait(false);
                await DeleteAIHistory("*.json").ConfigureAwait(false);
                break;
            case var s when s.StartsWith(COMMAND_ADMIN_AI_CLEAR):
                var clearCommand = s[COMMAND_ADMIN_AI_CLEAR.Length..];
                switch (clearCommand.ToUpper())
                {
                    case "":
                        await DeleteAIHistory("*.json").ConfigureAwait(false);
                        return true;
                    case "P":
                        await DeleteAIHistory("p*.json").ConfigureAwait(false);
                        return true;
                    case "G":
                        await DeleteAIHistory("g*.json").ConfigureAwait(false);
                        return true;
                    default:
                        if (!int.TryParse(clearCommand, out var userId))
                        {
                            await BotServer.SendPrivateMessageAsync(senderId, groupId, "命令格式有误!").ConfigureAwait(true);
                            return false;
                        }

                        await DeleteAIHistory($"*{userId}.json").ConfigureAwait(false);
                        return true;
                }

            default:
                await BotServer.SendPrivateMessageAsync(senderId, groupId, "命令格式有误!").ConfigureAwait(true);
                return false;
        }

        return true;

        Task ResetAIStatus()
        {
            var userIds = PublicVar.AIData.UserDatas.Keys.ToArray();
            userIds.ForEach(userId =>
            {
                if (userId == SuperAdminId)
                    PublicVar.AIData.UserDatas.AddOrUpdate(userId, SuperAdminAIUserData, SuperAdminAIUserData);
                else
                    PublicVar.AIData.UserDatas.AddOrUpdate(userId, DefaultAIUserData, DefaultAIUserData);
            });

            ConfigExtensions.SaveAIData();
            return BotServer.SendPrivateMessageAsync(senderId, groupId, $"{BOT_NICK_NAME} AI用户数据已重置!");
        }

        Task DeleteAIHistory(string searchPattern)
        {
            var sendMessage = string.Empty;
            Directory.EnumerateFiles(AI_HISTORY_PATH, searchPattern).ForEach(each =>
            {
                sendMessage += $"{each.Split('/')[^1]}{ENTER}";
                File.Delete(each);
            });
            return BotServer.SendPrivateMessageAsync(senderId, groupId,
                sendMessage.IsNullOrEmpty()
                    ? "没有历史记录需要删除!"
                    : $"{sendMessage}AI历史记录已删除!");
        }
    }
}