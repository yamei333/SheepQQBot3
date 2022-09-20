using SheepQQBot3.Model;

namespace SheepQQBot3.View
{
    public static partial class ProcessGroupMessage
    {
        /// <summary>
        /// 空格
        /// </summary>
        private const string SPACE = " ";

        /// <summary>
        /// 空格(全角)
        /// </summary>
        private const string SPACE_FULL = "　";

        /// <summary>
        /// 回车
        /// </summary>
        private const string ENTER = "\r\n";

        /// <summary>
        /// 复读机杀手
        /// </summary>
        /// <param name="groupMessage"><see cref="GroupMessage"/></param>
        public static bool RepeaterKiller(GroupMessage groupMessage)
        {
            // 系统QQ号的行为不记录
            //if (fromQQ == LogExtensions.SystemMasterQQ)
            //{
            //    if (message.Contains(REMOVE_BAN))
            //    {
            //        var removeQQ = long.Parse(regRemoveBan.Match(message).Value);
            //        var key = new KeyValuePair<long, long>(fromGroup, removeQQ);
            //        var repeaterProtect = PublicVar.__repeaterProtect;
            //        if (repeaterProtect.TryGetValue(key, out var banProtectInfo))
            //        {
            //            var banTime = (long)(banProtectInfo - DateTime.Now).TotalSeconds;
            //            if (banTime > 0)
            //            {
            //                // 根据守护再次禁言
            //                LogExtensions.AddInfoLog($"禁言守护: \r\n群号: {fromGroup}\r\nQQ号: {removeQQ}\r\n时间: {(banTime / 60d).ToString("0")}分钟");
            //                APIExtensions.YMM_SetGroupBan(fromGroup, removeQQ, banTime);
            //            }
            //            else
            //            {
            //                repeaterProtect.Remove(key);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        LogExtensions.AddDebugLog($"不处理系统QQ号的消息: \r\n消息: {message}");
            //    }
            //    return;
            //}

            //var nowDateTime = DateTime.Now;
            //// 消息为红包记录, 屏蔽该功能1分钟
            //// 如果时间内再收到红包, 则刷新时间
            //if (message.Contains("&#91;QQ红包&#93;"))
            //{
            //    Group_RepeaterKiller_EnabledTime = nowDateTime.AddSeconds(
            //        PublicVar.__sheepConfig._repeaterKillerConfig.HongbaoPartyTime);
            //    LogExtensions.AddInfoLog($"收到红包消息: \r\n设置红包狂欢时间至{Group_RepeaterKiller_EnabledTime.ToString("yyyy-M-dd HH:mm:ss")}");
            //    return;
            //}

            //if (nowDateTime <= Group_RepeaterKiller_EnabledTime)
            //{
            //    LogExtensions.AddDebugLog($"红包狂欢时间内: \r\n消息: {message}");
            //    return;
            //}

            //// 消息为语音记录, 不处理
            //if (message.Contains("CQ:record"))
            //{
            //    LogExtensions.AddDebugLog($"不处理语音消息: \r\n消息: {message}");
            //    return;
            //}

            //foreach (var item in PublicVar.__sheepConfig._repeaterKillerConfig.FilterRuleList)
            //{
            //    if (new Regex(item.Filter).IsMatch(message) && message.Length <= item.MaxLength)
            //    {
            //        LogExtensions.AddDebugLog($"已被规则过滤: \r\n规则: {item.Filter}-{item.MaxLength}\r\n消息: {message}");
            //        return;
            //    }
            //}

            var message = groupMessage.Message
                .Replace(SPACE, string.Empty)
                .Replace(SPACE_FULL, string.Empty)
                .Replace(ENTER, string.Empty);

            ;
            //// 处理掉忽略的字符
            //message = message
            //    .Replace(SPACE, string.Empty)
            //    .Replace(SPACE_FULL, string.Empty)
            //    .Replace(ENTER, string.Empty);
            //var repeaterMember = PublicVar.__repeaterMember;
            //var checkResult = repeaterMember.CheckCacheMessages(fromGroup, message);
            //if (checkResult.Result != RepeatMessageResult.NormalMessage)
            //{
            //    var result = checkResult.Result;
            //    var repeatResult = repeaterMember.AddAndCheckRepeatTimes(fromGroup, checkResult);
            //    var repeaterKillerConfig = PublicVar.__sheepConfig._repeaterKillerConfig;
            //    switch (repeatResult)
            //    {
            //        case RepeatLimitType.RepeatFirst:
            //            // 禁言操作, 首个复读机禁言
            //            try
            //            {
            //                // 第一个复读机 +30s
            //                var banTime = repeaterKillerConfig.BanUserList.AddOrUpdate(fromGroup, fromQQ, 30);
            //                //LogExtensions.AddInfoLog($"首个复读机禁言: \r\n群号: {fromGroup}\r\nQQ号: {fromQQ}\r\n时间: {(banTime / 60d).ToString("0")}分钟");
            //                //APIExtensions.YMM_SetGroupBan(fromGroup, fromQQ, banTime, repeaterKillerConfig.BanProtect == CheckState.Checked);
            //            }
            //            catch (Exception)
            //            {
            //                // 操作禁言失败, 什么都不做
            //            }
            //            break;
            //        case RepeatLimitType.OverLimit:
            //            // 禁言操作
            //            try
            //            {
            //                var banTime = repeaterKillerConfig.BanUserList.AddOrUpdate(fromGroup, fromQQ);
            //                LogExtensions.AddInfoLog($"已设置禁言: \r\n群号: {fromGroup}\r\nQQ号: {fromQQ}\r\n时间: {(banTime / 60d).ToString("0")}分钟");
            //                APIExtensions.YMM_SetGroupBan(fromGroup, fromQQ, banTime, repeaterKillerConfig.BanProtect == CheckState.Checked && banTime >= 3600);
            //            }
            //            catch (Exception)
            //            {
            //                // 操作禁言失败, 什么都不做
            //            }
            //            break;
            //        default:
            //            //  普通复读 +10s
            //            repeaterKillerConfig.BanUserList.AddOrUpdate(fromGroup, fromQQ, 10);
            //            break;
            //    }
            //}
            //else
            //{
            //    //  不复读 -1s
            //    var key = new KeyValuePair<long, long>(fromGroup, fromQQ);
            //    var banUserList = PublicVar.__sheepConfig._repeaterKillerConfig.BanUserList;
            //    if (banUserList.ContainsKey(key))
            //        banUserList[key] -= 1;
            //    // 新增消息
            //    repeaterMember.AddMessages(fromGroup, message, messageId, processDate, PublicVar.__sheepConfig);
            //}

            return true;
        }
    }
}