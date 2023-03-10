using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using Yamei.Common;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    public static partial class ProcessGroupMessage
    {
        /// <summary>
        /// 群提醒投稿方法命令的开头
        /// </summary>
        private const string COMMAND_ALARMAIDE_SUBMIT_LIBRARY = "#TG#";

        private static readonly Regex _regRemoveUrl = RegexGenerator.CQCodeRemoveUrl();
        private static readonly Regex _regRemoveSubType = RegexGenerator.CQCodeRemoveSubType();

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
            var targetId = groupMessage.Sender.User_Id;
            if (targetId != AdminId && !alarmAideSubmitMembers.Contains(targetId))
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
                await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] 未设置默认投稿项, 联系管理设置!");
                return false;
            }

            var alarmMessage = message
                .Substring(COMMAND_ALARMAIDE_SUBMIT_LIBRARY.Length);
            // MEMO : 有image表情的时候移除url和subType
            if (alarmMessage.IndexOf("CQ:image", StringComparison.Ordinal) > 0)
            {
                alarmMessage = _regRemoveUrl.Replace(alarmMessage, string.Empty);
                alarmMessage = _regRemoveSubType.Replace(alarmMessage, string.Empty);
            }

            try
            {
                var alarmTexts = alarmAideConfig.AlarmTexts;
                if (alarmTexts.Values.Any(each => each == alarmMessage))
                {
                    // MEMO : 已存在则不添加, 发送反馈
                    await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] 投稿失败, 相同的内容已存在!");
                    return false;
                }
                else
                {
                    // MEMO : 添加闹钟助手内容
                    alarmTexts.TryAdd(alarmTexts.GetSequence(), alarmMessage);
                    // MEMO : 发送反馈
                    await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] 投稿成功!!\n{alarmMessage}");
                    ConfigExtensions.SaveConfig();
                    return true;
                }
            }
            catch (Exception)
            {
                await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] 发生错误! 投稿内容有误!!");
                return false;
            }
        }
    }
}