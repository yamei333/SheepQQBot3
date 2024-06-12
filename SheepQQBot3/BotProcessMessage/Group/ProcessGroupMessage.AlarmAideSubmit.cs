using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Group;

public static partial class ProcessGroupMessage
{
    /// <summary>
    /// 闹钟助手投稿方法命令的开头
    /// </summary>
    private const string COMMAND_ALARMAIDE_SUBMIT_LIBRARY = "#TG#";

    /// <summary>
    /// 投稿文件夹名
    /// </summary>
    private const string TG_DIRECTORY_NAME = "TgImage";

    private static readonly Regex _regReplaceImage = RegexGenerator.CQCodeReplaceImage();
    private static readonly Regex _regRemoveUrl = RegexGenerator.CQCodeRemoveUrl();
    private static readonly Regex _regRemoveFileSize = RegexGenerator.CQCodeRemoveFileSize();

    /// <summary>
    /// 闹钟助手投稿
    /// <para>可在群内投稿新内容</para>
    /// </summary>
    /// <param name="alarmAideConfigs">闹钟助手配置</param>
    /// <param name="alarmAideSubmitMembers">可投稿成员列表</param>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    /// <returns></returns>
    public static async Task<bool> AlarmAideSubmit(
        Dictionary<Guid, AlarmAideConfig> alarmAideConfigs,
        HashSet<long> alarmAideSubmitMembers,
        GroupMessage groupMessage)
    {
        // MEMO : 非管理员/投稿者
        var targetId = groupMessage.Sender.UserId;
        if (!BotExtensions.IsAdmin(targetId) && !alarmAideSubmitMembers.Contains(targetId))
            return false;

        var groupId = groupMessage.GroupId;
        var message = groupMessage.Message;
        // MEMO : 命令格式检查
        var upperMessage = message.ToUpper();
        if (!upperMessage.StartsWith(COMMAND_ALARMAIDE_SUBMIT_LIBRARY))
            return false;

        // MEMO : 无可投稿配置
        var alarmAideConfig = alarmAideConfigs.Values.FirstOrDefault(each => each.IsDefault);
        if (alarmAideConfig == null)
        {
            await BotServer.SendGroupMessageAsync(groupId, $"{CQCode.At(targetId)}未设置默认投稿项, 联系管理设置!");
            return false;
        }

        var alarmMessage = message[COMMAND_ALARMAIDE_SUBMIT_LIBRARY.Length..];
        var resendAlarmMessage = alarmMessage;
        // MEMO : 有image表情的时候移除url和subType
        if (alarmMessage.IndexOf("CQ:image", StringComparison.Ordinal) > 0)
        {
            var matches = _regReplaceImage.Matches(alarmMessage);
            matches.ForEach(match =>
            {
                var picUrl = match.Groups[2].Value;
                var replaceContent = match.Groups[1].Value;
                var (isSuccessed, fileName) = HttpExtensions
                    .HttpDownloadAsync(picUrl, TG_DIRECTORY_NAME, false)
                    .Result;
                if (isSuccessed)
                {
                    alarmMessage = alarmMessage.Replace(
                        replaceContent,
                        CommonExtensions.GetPath(TG_DIRECTORY_NAME, fileName, GetPathType.CQCodePath));
                }

                alarmMessage = _regRemoveUrl.Replace(alarmMessage, string.Empty);
                alarmMessage = _regRemoveFileSize.Replace(alarmMessage, string.Empty);
                resendAlarmMessage = _regRemoveUrl.Replace(resendAlarmMessage, string.Empty);
                resendAlarmMessage = _regRemoveFileSize.Replace(resendAlarmMessage, string.Empty);
            });
        }

        try
        {
            var alarmTexts = alarmAideConfig.AlarmTexts;
            if (alarmTexts.Values.Any(each => each == alarmMessage))
            {
                // MEMO : 已存在则不添加, 发送反馈
                await BotServer.SendGroupMessageAsync(groupId, $"{CQCode.At(targetId)}投稿失败, 相同的内容已存在!");
                return false;
            }
            else
            {
                // MEMO : 添加闹钟助手内容
                alarmTexts.TryAdd(alarmTexts.GetSequence(), alarmMessage);
                // MEMO : 发送反馈
                await BotServer.SendGroupForwardMessageAsync(groupId, new GroupForwardMessage[]
                {
                    new(groupMessage.MessageId),
                    new(PublicVar.BOT_NAME, PublicVar.BotId, resendAlarmMessage),
                    new(PublicVar.BOT_NAME, PublicVar.BotId, "投稿成功!!"),
                });
                ConfigExtensions.SaveConfig();
                return true;
            }
        }
        catch (Exception)
        {
            await BotServer.SendGroupMessageAsync(groupId, $"{CQCode.At(targetId)}发生错误! 投稿内容有误!!");
            YameiLogExtensions.WriteLog(LogType.Error, $"投稿内容有误-{message}");
            return false;
        }
    }
}