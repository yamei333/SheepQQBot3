using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.DbModel;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.QQ;
using SheepQQBot3.Model.Setu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yamei.Common;
using static Masuit.Tools.Systems.EnumExt;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.BotProcessMessage.Group;

public static partial class ProcessGroupMessage
{
    //private const string[] SETUAPI_ICONS = "https://lolicon.app/favicon.ico";
    private static readonly string[] _setuIcons = [
        "https://i0.hdslb.com/bfs/garb/66a0850681611ac4dede74823a34e197913fb97f.png",
        "https://i0.hdslb.com/bfs/garb/6f57a2e1d8cedd68f5b837a7118bba37275ba5c4.png",
        "https://i0.hdslb.com/bfs/garb/20e2b689aebcd2fa8bbe1ffa440827c3062cc420.png",
        "https://i0.hdslb.com/bfs/garb/ac9c50a5751f9b02af366c9096b703a8889d7ea4.png",
        "https://i0.hdslb.com/bfs/garb/177b264dcdc99a107481ae8e7ead45dc7448dbd7.png",
        "https://i0.hdslb.com/bfs/garb/8b6d3154bd4eb96df7603163cffc1db407d92bf2.png",
    ];

    private static readonly Regex _regCorrectSetuMessage = new("^.*?色图[a-zA-Z]?$", RegexOptions.Multiline);

    /// <summary>
    /// 色图命令的开头
    /// </summary>
    private const string COMMAND_CUSTOM_GROUP_SETU_LIBRARY = "#ST#";

    /// <summary>
    /// 色图DEBUG命令的开头
    /// </summary>
    private const string COMMAND_CUSTOM_GROUP_SETUDEBUG_LIBRARY = "#STDEBUG#";

    /// <summary>
    /// 色图CD命令
    /// </summary>
    private const string COMMAND_CUSTOM_GROUP_SETUCD_LIBRARY = "#STCD#";

    /// <summary>
    /// 色图清空CD命令
    /// </summary>
    private const string COMMAND_CUSTOM_GROUP_SETU_RESETCD_LIBRARY = "#STRESETCD#";

    /// <summary>
    /// 色图清空LV命令
    /// </summary>
    private const string COMMAND_CUSTOM_GROUP_SETU_RESETLV_LIBRARY = "#STRESETLV#";

    /// <summary>
    /// 色图解封命令
    /// </summary>
    private const string COMMAND_CUSTOM_GROUP_SETU_RESETBAN_LIBRARY = "#STRESETBAN#";

    /// <summary>
    /// 色图清空所有命令
    /// </summary>
    private const string COMMAND_CUSTOM_GROUP_SETU_RESETALL_LIBRARY = "#STRESETALL#";

    /// <summary>
    /// 色图斗士排行命令
    /// </summary>
    private const string COMMAND_CUSTOM_GROUP_SETURANK_LIBRARY = "#STRANK#";

    /// <summary>
    /// 缓存文件夹名称
    /// </summary>
    private const string PATH_CACHE_IMAGE = "Cache";

    /// <summary>
    /// 色图的基础CD, 不能发得太频繁
    /// </summary>
    private const int SendBaseDelay = 120;

    /// <summary>
    /// 色图关键字
    /// </summary>
    private const string SETU_KEYWORD = "色图";

    /// <summary>
    /// 最大色图斗士Lv
    /// </summary>
    private const int MaxSenderLv = 9;

    private static readonly Dictionary<SetuType, int> _setuWeight = new()
    {
        {SetuType.Lolicon, 30},
        {SetuType.Lolisuki, 20},
        {SetuType.Yuban, 4},
        {SetuType.NyanCatda, 10},
        {SetuType.Jitsu, 2},
        {SetuType.JitsuSelf, 6},
    };

    /// <summary>
    /// 色图不满时的反击关键词, 拉黑时间为分钟
    /// </summary>
    private static readonly Dictionary<string, int> _setuBuman = new()
    {
        {"不够", 60},
        {"这也", 60},
        {"一般", 60},
        {"不色", 120},
        {"就这", 300},
        {"太小", 120},
        {"菜", 300},
        {"不太行", 120},
        {"吗", 60},
    };

    //private static readonly string[] _setuYouwant =
    //[
    //    string.Empty, "你要的", "你点的", "请求的", "申请的", "需求的",
    //];

    //private static readonly string[] _setuGetted =
    //[
    //    "来了", "已经送出", "到了", "来咯", "lei了", "已发送", "给你了",
    //];

    private static readonly string[] _setuSource =
    [
        "原图", "大图", "查看大图", "原图查看", "源链接", "图源",
    ];

    //private static readonly string[] _setuNo =
    //[
    //    "别", "憋", "鳖", "No",
    //];

    //private static readonly string[] _setuSendLe =
    //[
    //    "发了", "要了", "整了", "冲了", "弄了",
    //];

    //private static readonly string[] _setuRequest =
    //[
    //    "获取",
    //];

    //private static readonly string[] _setuGetting =
    //[
    //    "下载中", "传送中", "获取中", "取得中", "载入中",
    //];

    //private static readonly string[] _setuCDWasAdded =
    //[
    //    "被$ADD_LEVEL$了!", "被$ADD_LEVEL$, 时间延长了!", "被$ADD_LEVEL$, 大加特加了!",
    //];

    private static readonly string[] _setuCDWasReduced =
    [
        "色图的CD发生了$ADD_LEVEL$变化, 被减少了!",
    ];

    private static readonly string[] _setuKexiStart =
    [
        "太可惜了!",
        "Taxi了!",
        "悲剧啊!",
        "尬住了!",
        "寄了!",
        "鸡了!",
        "JI了!",
    ];

    //private static readonly string[] _setuUnluck =
    //[
    //    "你运气差!",
    //    "你霉运!",
    //    "你脸黑!",
    //    "你非洲人!",
    //];

    private static readonly string[] _setuKexiEnd =
    [
        "我的我的, 哈哈",
        "都怪ruojiji2",
        "今日不宜色图",
        "吔?你的XP有点怪",
        "一定是关键字太怪了",
        "图库懂的都没你多",
    ];

    private static readonly Dictionary<string, string> _tagDictionary = new()
    {
        {"导师", "甘雨"},
        {"RJJ", "甘雨"},
        {"RJJ2", "甘雨"},
        {"RUOJIJI", "甘雨"},
        {"RUOJIJI2", "甘雨"},
        {"车万", "东方"},
        {"铜", "萝莉"},
    };

    private static readonly string[] _banKeywordList =
    [
        "蟑螂",
        "元梦之星",
        "元",
        "老鼠药",
        "丁真",
        "风控",
        "雌大鬼",
        "酬勤",
        "假",
        "不色",
        "吃",
        "GAYSHIT",
        "鸭所",
        "张小鸭",
        "原批",
        "比头大",
        "你妈",
        "充气薯",
    ];

    /// <summary>
    /// 随机色图
    /// </summary>
    /// <param name="botConfig">配置</param>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    public static async Task<bool> RandomSetuAsync(BotConfig botConfig, GroupMessage groupMessage)
    {
        var groupId = groupMessage.GroupId;
        var senderId = groupMessage.Sender.UserId;
        var messageId = groupMessage.MessageId;
        var message = groupMessage.Message;
        var dateNow = DateTime.Now;

        var setuDoushiInfo = await GetSetuDoushiInfoAsync(senderId).ConfigureAwait(false);
        if ((dateNow - setuDoushiInfo.BlackListCD.ToDateTime()).TotalMicroseconds < 0)
        {
            // MEMO : 黑名单的人, 给色图请求打标记
            if (_regCorrectSetuMessage.IsMatch(message))
                await BotServer.SendMessageEmojiAsync(messageId, Emoji.DogeBig).ConfigureAwait(false);

            return false;
        }

        var setuDoushiLv = setuDoushiInfo.SetuDoushiLv;
        var setuCd = setuDoushiInfo.SetuCD.ToDateTime();
        var isAdmin = BotExtensions.IsAdmin(senderId);
        if (message.StartsWith(COMMAND_CUSTOM_GROUP_SETUCD_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
        {
            if (message.Equals(COMMAND_CUSTOM_GROUP_SETUCD_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
            {
                // MEMO : 显Lv
                var sendMessage = $"当前色图斗士Lv{setuDoushiInfo.CalcSetuDoushiLv(dateNow)}, " +
                    $"{BotExtensions.GetSetuSuccessPercent(setuDoushiInfo, dateNow)}";
                await BotServer.SendGroupMessageAsync(groupId, sendMessage).ConfigureAwait(false);
            }
            else if (isAdmin)
            {
                if (long.TryParse(message[COMMAND_CUSTOM_GROUP_SETUCD_LIBRARY.Length..], out var searchUserId))
                {
                    if (searchUserId < 100)
                    {
                        var vtuberSetuDoushiInfo = new SetuDoushiInfo
                        {
                            TargetId = 0,
                            SetuCD = dateNow.ToTimeStamp(),
                            SetuDoushiLv = searchUserId,
                        };
                        var sendMessage = $"虚拟色图斗士Lv{searchUserId}, " +
                            $"{BotExtensions.GetSetuSuccessPercent(vtuberSetuDoushiInfo, dateNow)}";
                        await BotServer.SendGroupMessageAsync(groupId, sendMessage).ConfigureAwait(false);
                    }
                    else
                    {
                        var searchSetuDoushiInfo = await GetSetuDoushiInfoAsync(searchUserId, false).ConfigureAwait(false);
                        // MEMO : 显CD
                        var sendMessage = $"目标色图斗士Lv{searchSetuDoushiInfo.CalcSetuDoushiLv(dateNow)} " +
                            $"CD[{GetCD(searchSetuDoushiInfo)}], " +
                            $"{BotExtensions.GetSetuSuccessPercent(searchSetuDoushiInfo, dateNow)}";
                        await BotServer.SendGroupMessageAsync(groupId, sendMessage).ConfigureAwait(false);
                    }
                }
                else
                {
                    await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_Question).ConfigureAwait(false);
                }
            }

            return true;
        }

        if (isAdmin)
        {
            if (message.StartsWith(COMMAND_CUSTOM_GROUP_SETU_RESETCD_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
            {
                if (long.TryParse(message[COMMAND_CUSTOM_GROUP_SETU_RESETCD_LIBRARY.Length..], out var searchUserId))
                {
                    // MEMO : 清空CD
                    var targetDoushiInfo = await GetSetuDoushiInfoAsync(searchUserId).ConfigureAwait(false);
                    targetDoushiInfo.SetuCD = 0;
                    UpdateSetuDoushiInfo(targetDoushiInfo);

                    await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_Congratolation).ConfigureAwait(false);
                }
                else
                {
                    await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_Question).ConfigureAwait(false);
                }

                return true;
            }

            if (message.StartsWith(COMMAND_CUSTOM_GROUP_SETU_RESETLV_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
            {
                if (long.TryParse(message[COMMAND_CUSTOM_GROUP_SETU_RESETLV_LIBRARY.Length..], out var searchTargetId))
                {
                    // MEMO : 清空Lv
                    var targetDoushiInfo = await GetSetuDoushiInfoAsync(searchTargetId).ConfigureAwait(false);
                    targetDoushiInfo.SetuDoushiLv = 0;
                    UpdateSetuDoushiInfo(targetDoushiInfo);

                    await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_Congratolation).ConfigureAwait(false);
                }
                else
                {
                    await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_Question).ConfigureAwait(false);
                }

                return true;
            }

            if (message.StartsWith(COMMAND_CUSTOM_GROUP_SETU_RESETBAN_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
            {
                if (long.TryParse(message[COMMAND_CUSTOM_GROUP_SETU_RESETBAN_LIBRARY.Length..], out var searchTargetId))
                {
                    // MEMO : 解封
                    var targetDoushiInfo = await GetSetuDoushiInfoAsync(searchTargetId).ConfigureAwait(false);
                    targetDoushiInfo.BlackListCD = 0;
                    UpdateSetuDoushiInfo(targetDoushiInfo);

                    await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_Congratolation).ConfigureAwait(false);
                }
                else
                {
                    await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_Question).ConfigureAwait(false);
                }

                return true;
            }

            if (message.StartsWith(COMMAND_CUSTOM_GROUP_SETU_RESETALL_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
            {
                if (long.TryParse(message[COMMAND_CUSTOM_GROUP_SETU_RESETALL_LIBRARY.Length..], out var searchTargetId))
                {
                    // MEMO : 清空CD以及解封
                    var targetDoushiInfo = await GetSetuDoushiInfoAsync(searchTargetId).ConfigureAwait(false);
                    targetDoushiInfo.SetuDoushiLv = 0;
                    targetDoushiInfo.SetuCD = 0;
                    targetDoushiInfo.BlackListCD = 0;
                    UpdateSetuDoushiInfo(targetDoushiInfo);

                    await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_Congratolation).ConfigureAwait(false);
                }
                else
                {
                    await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_Question).ConfigureAwait(false);
                }

                return true;
            }
        }

        if (message.Equals(COMMAND_CUSTOM_GROUP_SETURANK_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
        {
            var groupMembers = await BotServer.GetGroupMembersAsync(groupId).ConfigureAwait(false);
            if (groupMembers == null)
                return false;

            var sendMessage = "=====色图斗士排行=====";
            var rankIndex = 1;
            BotDb.SetuSendHistorys
                .Where(each => each.IsGetSuccessed == 1)
                .GroupBy(each => each.TargetId)
                .AsEnumerable()
                .Select(each => (each.Key, each.Count()))
                .OrderByDescending(each => each.Item2)
                .Take(10)
                .ForEach(info =>
                {
                    sendMessage += $"\r\n{rankIndex++}. " +
                        $"{GetSetuSenderName(info.Key)} [色图数 {info.Item2}]";
                });
            await BotServer.SendGroupMessageAsync(groupId, sendMessage).ConfigureAwait(false);
            return true;

            string GetSetuSenderName(long userId)
            {
                if (groupMembers.TryGetValue(userId, out var groupMember))
                {
                    return string.IsNullOrEmpty(groupMember.Card)
                        ? $"{groupMember.NickName}({userId})"
                        : $"{groupMember.Card}({userId})";
                }
                else
                {
                    return $"无名色图斗士({userId})";
                }
            }
        }

        var isSetuDebug = false;
        var tag = string.Empty;
        var setuKeywordCheckOK = false;
        SetuType? targetSetuApiType = null;
        // MEMO : #st# (#st#支持关键字)
        if (message.StartsWith(COMMAND_CUSTOM_GROUP_SETU_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
        {
            setuKeywordCheckOK = true;
            tag = message[COMMAND_CUSTOM_GROUP_SETU_LIBRARY.Length..];
            goto StartSetu;
        }

        // MEMO : 处理色图类型关键字
        var messageEndWithChar = message[^1..].ToUpper();
        switch (messageEndWithChar)
        {
            case "L":
                message = message[..^1];
                targetSetuApiType = SetuType.Lolicon;
                break;
            case "S":
                message = message[..^1];
                targetSetuApiType = SetuType.Lolisuki;
                break;
            case "N":
                message = message[..^1];
                targetSetuApiType = SetuType.NyanCatda;
                break;
            case "Y":
                message = message[..^1];
                targetSetuApiType = SetuType.Yuban;
                break;
            case "J":
                message = message[..^1];
                targetSetuApiType = SetuType.Jitsu;
                break;
        }

        // MEMO : 字数在8字以内, 并包含色图关键字 (支持前置关键字)
        if (message.GetByteCount() <= 45 && message.EndsWith(SETU_KEYWORD))
        {
            setuKeywordCheckOK = true;
            tag = message[..^2];
            goto StartSetu;
        }

        // MEMO : #stdebug#
        if (message.Equals(COMMAND_CUSTOM_GROUP_SETUDEBUG_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
        {
            isSetuDebug = true;
            setuKeywordCheckOK = true;
            goto StartSetu;
        }

    StartSetu:
        if (_banKeywordList.Contains(tag))
        {
            setuDoushiInfo.BlackListCD = dateNow.AddHours(336).ToTimeStamp();
            setuDoushiInfo.SetuCD = dateNow.ToTimeStamp();
            UpdateSetuDoushiInfo(setuDoushiInfo);
            await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_Beat).ConfigureAwait(false);
            return true;
        }

        var sourceTag = tag;
        // MEMO : tag映射
        if (_tagDictionary.TryGetValue(tag.ToUpper(), out var changeTag))
            tag = changeTag;

        var isSearchTag = !string.IsNullOrEmpty(tag);
        List<SetuSendHistory> targetSetuSendHistorys;
        lock (BotDb.SyncLock)
        {
            targetSetuSendHistorys = BotDb.SetuSendHistorys
                .Where(each => each.TargetId == senderId)
                .ToList();
        }

        var lastHistory = Enumerable.MaxBy(targetSetuSendHistorys, each => each.TimeStamp);
        var lastKeyword = lastHistory?.SearchKeyword ?? string.Empty;
        if (!string.IsNullOrEmpty(lastKeyword) && lastKeyword == sourceTag)
        {
            const int CHECK_TIMES = 2;
            // MEMO : 最后2次色图都有关键字
            List<SetuSendHistory> last2Historys;
            lock (BotDb.SyncLock)
            {
                last2Historys = targetSetuSendHistorys
                    .OrderByDescending(history => history.TimeStamp)
                    .Where(history => !history.IsR18Bonus.ToBool()
                        && history.IsRequestSuccessed.ToBool())
                    .Take(CHECK_TIMES)
                    .ToList();
            }

            if (last2Historys.Count == CHECK_TIMES
                && last2Historys.All(history => history.SearchKeyword == lastKeyword
                    && !history.IsGetSuccessed.ToBool()))
            {
                setuDoushiInfo.BlackListCD = dateNow.AddHours(336).ToTimeStamp();
                setuDoushiInfo.SetuCD = dateNow.ToTimeStamp();
                UpdateSetuDoushiInfo(setuDoushiInfo);
                await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_Beat).ConfigureAwait(false);
                return true;
            }
        }

        var setuSendHistory = lastHistory?.TimeStamp.ToDateTime() ?? DateTime.MinValue;
        if (setuKeywordCheckOK)
        {
            if (!isSetuDebug && (dateNow - setuSendHistory).TotalSeconds <= 5 + setuDoushiLv * 5)
            {
                if (setuDoushiInfo.ToFastTimes >= 2)
                {
                    // MEMO : 连续刷则关小黑屋
                    await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_Beat).ConfigureAwait(false);
                    setuDoushiInfo.ToFastTimes = 0;
                    setuDoushiInfo.BlackListCD = dateNow.AddHours(1).ToTimeStamp();
                }
                else
                {
                    // MEMO : 一定时间内不连续响应
                    await BotServer.SendMessageEmojiAsync(messageId, Emoji.Coffee).ConfigureAwait(false);
                    setuDoushiInfo.ToFastTimes += 1;
                }

                UpdateSetuDoushiInfo(setuDoushiInfo);
                return true;
            }

            setuDoushiInfo.ToFastTimes = 0;

            //botConfig.SetuSendLastRecords[targetId] = dateNow;
            var r18Bonus = false;
            var addSecond = 0;
            var addLevel = SetuAddLevel.Normal;
            var addCDReason = AddCDReason.RequestSuccessed;
            var canSendSetu = false;

            var oldSetuSenderLv = setuDoushiLv;
            var changeLvTime = 0;
            var changeLvTag = 0;
            var changeLvFast = 0;
            if ((!IsDebug || isSetuDebug) && isAdmin)
            {
                // MEMO : ADMIN无限制要色图
                canSendSetu = true;
            }
            else
            {
                // MEMO : 色图Lv减少
                if (setuDoushiLv > 0 && setuCd != DateTime.MinValue)
                {
                    var changeLvTimePoint = (long)((dateNow - setuCd).TotalMinutes / 90);
                    while (setuDoushiLv > 0 && changeLvTimePoint >= setuDoushiLv)
                    {
                        changeLvTimePoint -= setuDoushiLv;
                        changeLvTime--;
                        setuDoushiLv--;
                    }
                }

                List<RandomWeight<SendSetuConfig>> randActions;
                if (dateNow > setuCd)
                {
                    // MEMO : 0.13.3.16 除了基数其他都x6, 以维持10%暴击率
                    randActions = new List<RandomWeight<SendSetuConfig>>
                    {
                        new(20000, new SendSetuConfig(SendBaseDelay + (int)(60 * Math.Pow(setuDoushiLv, 2)) + Rand.Next(-60, 60),
                            AddCDReason.RequestSuccessed, SetuAddLevel.Normal, true)),
                        new(2100, new SendSetuConfig(
                            SendBaseDelay + (int)(60 * Math.Pow(setuDoushiLv, 2)) + Rand.Next(-60, 60),
                            AddCDReason.RequestSuccessed, SetuAddLevel.ExtraDouble, true)),
                        new((int)(900 * Math.Pow(setuDoushiLv, 2.5)), new SendSetuConfig(
                            Rand.Next(1 + (int)(1 * Math.Pow(setuDoushiLv, 2)), 15 + (int)(5 * Math.Pow(setuDoushiLv, 2))),
                            AddCDReason.RequestFailed, SetuAddLevel.Normal)),
                        new((int)(900 * Math.Pow(setuDoushiLv, 2.5)), new SendSetuConfig(
                            Rand.Next(3 + (int)(2 * Math.Pow(setuDoushiLv, 2)), 30 + (int)(10 * Math.Pow(setuDoushiLv, 2))),
                            AddCDReason.RequestFailed, SetuAddLevel.Normal)),
                        new((int)(450 * Math.Pow(setuDoushiLv, 2.5)), new SendSetuConfig(
                            Rand.Next(5 + (int)(3 * Math.Pow(setuDoushiLv, 2)), 45 + (int)(15 * Math.Pow(setuDoushiLv, 2))),
                            AddCDReason.RequestFailed, SetuAddLevel.Double)),
                        new((int)(240 * Math.Pow(setuDoushiLv, 2.5)), new SendSetuConfig(
                            Rand.Next(7 + (int)(4 * Math.Pow(setuDoushiLv, 2)), 60 + (int)(20 * Math.Pow(setuDoushiLv, 2))),
                            AddCDReason.RequestFailed, SetuAddLevel.SuperDouble)),
                        new(900 - (int)(setuDoushiLv * 135.0 / MaxSenderLv), new SendSetuConfig(0,
                            AddCDReason.RequestSuccessed, SetuAddLevel.Free, true)),
                        new(540 - (int)(setuDoushiLv * 135.0 / MaxSenderLv), new SendSetuConfig(0,
                            AddCDReason.RequestSuccessed, SetuAddLevel.FreeExtraDouble, true)),
                        new(180 - (int)(setuDoushiLv * 27 / MaxSenderLv), new SendSetuConfig(
                            SendBaseDelay + (int)(60 * Math.Pow(setuDoushiLv, 2)) + Rand.Next(-60, 60),
                            AddCDReason.RequestSuccessed, SetuAddLevel.Normal, true, true)),
                        new(120 - (int)(setuDoushiLv * 27 / MaxSenderLv), new SendSetuConfig(
                            SendBaseDelay + (int)(60 * Math.Pow(setuDoushiLv, 2)) + Rand.Next(-60, 60),
                            AddCDReason.RequestSuccessed, SetuAddLevel.ExtraDouble, true, true)),
                        new(60 - (int)(setuDoushiLv * 9.0 / MaxSenderLv), new SendSetuConfig(0,
                            AddCDReason.RequestSuccessed, SetuAddLevel.Free, true, true)),
                        new(36 - (int)(setuDoushiLv * 9.0 / MaxSenderLv), new SendSetuConfig(0,
                            AddCDReason.RequestSuccessed, SetuAddLevel.FreeExtraDouble, true, true)),
                    };
                }
                else
                {
                    if ((setuCd - dateNow).TotalSeconds >= 300 - (int)(setuDoushiLv * 150.0 / 15))
                    {
                        // MEMO : CD5分钟以上, 老实等着吧
                        BotServer.SendMessageEmojiAsync(messageId, Emoji.Coffee);
                        return true;
                    }

                    randActions = new List<RandomWeight<SendSetuConfig>>
                    {
                        new(10000, new SendSetuConfig((int)(Rand.Next(5, 45) * Math.Pow(1.1, setuDoushiLv)),
                            AddCDReason.NotReady, SetuAddLevel.Normal)),
                        new(3500, new SendSetuConfig((int)(Rand.Next(5, 45) * 2 * Math.Pow(1.1, setuDoushiLv)),
                            AddCDReason.NotReady, SetuAddLevel.Double)),
                        new(1500, new SendSetuConfig((int)(Rand.Next(5, 45) * 4 * Math.Pow(1.1, setuDoushiLv)),
                            AddCDReason.NotReady, SetuAddLevel.SuperDouble)),
                        new(750, new SendSetuConfig((int)(Rand.Next(5, 45) * 8 * Math.Pow(1.1, setuDoushiLv)),
                            AddCDReason.NotReady, SetuAddLevel.Golden)),
                        new(300, new SendSetuConfig((int)(Rand.Next(5, 45) * 16 * Math.Pow(1.1, setuDoushiLv)),
                            AddCDReason.NotReady, SetuAddLevel.Platinum)),
                        new(150, new SendSetuConfig((int)(Rand.Next(5, 45) * 32 * Math.Pow(1.1, setuDoushiLv)),
                            AddCDReason.NotReady, SetuAddLevel.Diamond)),
                        new(10, new SendSetuConfig((int)(Rand.Next(5, 45) * 256 * Math.Pow(1.1, setuDoushiLv)),
                            AddCDReason.NotReady, SetuAddLevel.Death)),
                        new(1500, new SendSetuConfig(Rand.Next(10, 90) * -1, AddCDReason.NotReady, SetuAddLevel.Luck)),
                        new(600, new SendSetuConfig(Rand.Next(10, 90) * -4, AddCDReason.NotReady, SetuAddLevel.LuckSuper)),
                        new(200, new SendSetuConfig(Rand.Next(10, 90) * -16, AddCDReason.NotReady, SetuAddLevel.LuckGolden)),
                    };
                }

                randActions.TryGetRandomWeight(out var resultAction);
                SetSetuValues(resultAction.Value);
                if (!canSendSetu)
                {
                    var last9Historys = targetSetuSendHistorys
                        .OrderByDescending(each => each.TimeStamp)
                        .Take(9)
                        .ToArray();
                    // MEMO : 色图10连跪, 冷静24小时
                    if (last9Historys.Length == 9 && last9Historys.All(each => !each.IsRequestSuccessed.ToBool()))
                    {
                        setuDoushiInfo.BlackListCD = dateNow.AddHours(24).ToTimeStamp();
                        UpdateSetuDoushiInfo(setuDoushiInfo);
                        BotServer.SendMessageEmojiAsync(messageId, Emoji.E_Beat);
                        return true;
                    }
                }

                // MEMO : 参数注入意图
                if (tag!.Contains('&') || tag!.Contains("%26"))
                {
                    setuDoushiInfo.BlackListCD = dateNow.AddHours(24).ToTimeStamp();
                    setuDoushiInfo.SetuCD = dateNow.ToTimeStamp();
                    UpdateSetuDoushiInfo(setuDoushiInfo);
                    BotServer.SendMessageEmojiAsync(messageId, Emoji.E_Beat);
                    return true;
                }

                // MEMO : 色图Lv增加
                if (canSendSetu)
                {
                    if (isSearchTag && Rand.CheckPercent(60))
                    {
                        changeLvTag = 1;
                        setuDoushiLv++;
                    }

                    if ((dateNow - setuCd).TotalSeconds <= 90)
                    {
                        changeLvFast = 1;
                        setuDoushiLv++;
                    }
                }

                if (setuDoushiLv > MaxSenderLv)
                    setuDoushiLv = MaxSenderLv;

                setuDoushiInfo.SetuDoushiLv = setuDoushiLv;
                UpdateSetuDoushiInfo(setuDoushiInfo);
            }

            if (PublicVar.IsDebug)
            {
                await BotServer.SendGroupMessageAsync(groupId, "[DEBUG]"
                        + $"{ENTER}目标对象: {senderId}"
                        + $"{ENTER}色图Lv: {setuDoushiLv}"
                        + $"{ENTER}是否发送: {canSendSetu}"
                        + $"{ENTER}增加时间: {addSecond}s"
                        + $"{ENTER}色图CD: {GetCD(setuDoushiInfo)}")
                    .ConfigureAwait(false);
            }

            var addSetuSenderLv = setuDoushiLv - oldSetuSenderLv;
            var isFree = false;
            if (!canSendSetu)
            {
                AddCD();
                var sendMessage = string.Empty;
                if (addSecond > 0)
                {
                    // MEMO : CD增加
                    switch (addCDReason)
                    {
                        case AddCDReason.RequestFailed:
                            //sendMessage = $"{CQCode.At(senderId)}"
                            //    + $"{_setuKexiStart.Random()} {_setuUnluck.Random()} {SETU_KEYWORD}{_setuRequest.Random()[..2]}失败!"
                            //    + $"{SETU_KEYWORD}的CD{_setuCDWasAdded.Random().Replace("$ADD_LEVEL$", addLevel.ToAddLevelString())}"
                            //    + GetSetuLvInfo();
                            await BotServer.SendMessageEmojiAsync(messageId, Emoji.Moyu).ConfigureAwait(false);
                            break;
                        case AddCDReason.NotReady:
                            //sendMessage = $"{CQCode.At(senderId)}"
                            //    + $"{_setuNo.Random()}{_setuSendLe.Random()}, {SETU_KEYWORD}CD还没到呢!"
                            //    + $"{SETU_KEYWORD}的CD{_setuCDWasAdded.Random().Replace("$ADD_LEVEL$", addLevel.ToAddLevelString())}"
                            //    + GetSetuLvInfo();
                            await BotServer.SendMessageEmojiAsync(messageId, Emoji.Coffee).ConfigureAwait(false);
                            break;
                        default:
                            // MEMO : 应该不会有此Case
                            //sendMessage = $"{CQCode.At(senderId)}"
                            //    + $"{_setuKexiStart.Random()} {_setuUnluck.Random()} {SETU_KEYWORD}{_setuRequest.Random()[..2]}失败!"
                            //    + $"{SETU_KEYWORD}的CD{_setuCDWasAdded.Random().Replace("$ADD_LEVEL$", addLevel.ToAddLevelString())}"
                            //    + GetSetuLvInfo();
                            await BotServer.SendMessageEmojiAsync(messageId, Emoji.Boom).ConfigureAwait(false);
                            break;
                    }
                }
                else
                {
                    // MEMO : 幸运(CD减少)
                    sendMessage = $"{CQCode.At(senderId)}"
                        + $"运气好, {_setuCDWasReduced.Random().Replace("$ADD_LEVEL$", addLevel.ToAddLevelString())}"
                        + $" ({addSecond}s)";
                }

                await BotDb.AddAsync(new SetuSendHistory(senderId, dateNow, sourceTag, false, false, false, false))
                    .ConfigureAwait(false);
                if (!string.IsNullOrEmpty(sendMessage))
                {
                    await BotServer.SendGroupMessageAsync(groupId, sendMessage)
                        .ConfigureAwait(false);
                }

                return true;
            }
            else
            {
                if (!isAdmin && addSecond == 0)
                {
                    // MEMO : 白嫖
                    isFree = true;
                    await BotServer.SendMessageEmojiAsync(messageId, Emoji.Plus1).ConfigureAwait(false);
                    goto SendSetu;
                }

                // MEMO : 白嫖不加CD
                AddCD();
            }

            var revertCd = DateTime.MinValue;
        SendSetu:
            await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);

            try
            {
                var isR18 = false;
                var randomSetuKeyword = targetSetuApiType switch
                {
                    SetuType.Lolicon => GetRandomWeightSetuInfo(isR18, SetuType.Lolicon),
                    SetuType.Lolisuki => GetRandomWeightSetuInfo(isR18, SetuType.Lolisuki),
                    SetuType.NyanCatda => GetRandomWeightSetuInfo(isR18, SetuType.NyanCatda),
                    SetuType.Yuban => GetRandomWeightSetuInfo(isR18, SetuType.Yuban),
                    SetuType.Jitsu => GetRandomWeightSetuInfo(isR18, SetuType.Jitsu),
                    _ => GetRandomWeightSetuInfo(isR18,
                        SetuType.Lolicon, SetuType.Lolisuki, SetuType.NyanCatda, SetuType.Yuban, SetuType.Jitsu),
                };
                var randomSetu = targetSetuApiType switch
                {
                    SetuType.Lolicon => GetRandomWeightSetuInfo(isR18, SetuType.Lolicon),
                    SetuType.Lolisuki => GetRandomWeightSetuInfo(isR18, SetuType.Lolisuki),
                    SetuType.NyanCatda => GetRandomWeightSetuInfo(isR18, SetuType.NyanCatda),
                    SetuType.Yuban => GetRandomWeightSetuInfo(isR18, SetuType.Yuban),
                    SetuType.Jitsu => GetRandomWeightSetuInfo(isR18, SetuType.Jitsu),
                    SetuType.JitsuSelf => GetRandomWeightSetuInfo(isR18, SetuType.JitsuSelf),
                    _ => GetRandomWeightSetuInfo(isR18, Enum.GetValues<SetuType>()),
                };
                Func<string, Task<SetuInfo>>[] randomSetuDefault =
                [
                    SetuExtensions.GetSetu_LoliconAsync,
                    SetuExtensions.GetSetu_NyanCatdaAsync,
                    SetuExtensions.GetSetu_YubanAsync,
                    SetuExtensions.GetSetu_JitsuAsync,
                    SetuExtensions.GetSetu_JitsuSelfAsync,
                ];
                var (setuInfo, fileName) = await GetSetu(() => isSearchTag
                        ? randomSetuKeyword.TryGetRandomWeight(out var funcResult)
                            ? funcResult.Value(tag)
                            : randomSetuDefault.Random()(tag)
                        : randomSetu.TryGetRandomWeight(out var funcResult2)
                            ? funcResult2.Value(tag)
                            : randomSetuDefault.Random()(tag),
                    true, false).ConfigureAwait(false);
                if (setuInfo == null)
                    return false;

                switch (setuInfo.Result)
                {
                    case SetuResult.Successed:
                        break;
                    case SetuResult.NoSearchResult:
                        await BotDb.AddAsync(new SetuSendHistory(senderId, dateNow, sourceTag, true, false, isFree,
                                r18Bonus))
                            .ConfigureAwait(false);
                        // MEMO : 最后3次有关键字的色图检索都失败了, 加本次4连败了
                        const int CHECK_TIMES = 3;
                        var last5Historys = targetSetuSendHistorys
                            .OrderByDescending(history => history.TimeStamp)
                            .Where(history => history.IsSearchTag && history.IsRequestSuccessed.ToBool())
                            .Take(CHECK_TIMES)
                            .ToList();
                        if (last5Historys.Count == CHECK_TIMES &&
                            last5Historys.All(history => !history.IsGetSuccessed.ToBool()))
                        {
                            setuDoushiInfo.BlackListCD = dateNow.AddHours(72).ToTimeStamp();
                            setuDoushiInfo.SetuCD = dateNow.ToTimeStamp();
                            UpdateSetuDoushiInfo(setuDoushiInfo);
                            await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_Beat).ConfigureAwait(false);
                            return true;
                        }

                        await BotServer.SendMessageEmojiAsync(messageId, Emoji.Knock).ConfigureAwait(false);
                        return true;
                    case SetuResult.ApiError:
                    case SetuResult.Timeout:
                    case SetuResult.OtherError:
                    case SetuResult.ApiR18ReviewError:
                        await BotServer.SendMessageEmojiAsync(messageId, Emoji.Boom).ConfigureAwait(false);
                        setuDoushiInfo.SetuCD = dateNow.AddSeconds(20).ToTimeStamp();
                        UpdateSetuDoushiInfo(setuDoushiInfo);
                        await BotDb.AddAsync(new SetuSendHistory(senderId, dateNow, sourceTag, true, false, isFree, false))
                            .ConfigureAwait(false);
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException(setuInfo.Result.ToString());
                }

                var sendMessages = new List<GroupForwardMessage>
                {
                    //new(messageId),
                    //new(groupMessage.Sender.NickName, senderId, $"{CQCode.Reply(senderId, messageId)}"),
                    //new(groupMessage.Sender.NickName, senderId, message),
                    //new(BOT_NAME, BotId, $"{GetSetuLvInfo()}"),

                    // MEMO : LLOneBot转发里面不能用Reply, 因为他是先发给自己, 再群组转发
                    //new(BOT_NAME, BotId, $"{CQCode.Reply(targetId, messageId)}{GetSetuLvInfo()}"),
                    new($"{setuInfo.SetuType}", SystemId,
                        CQCode.Image(CommonExtensions.GetPath(PATH_CACHE_IMAGE, fileName, GetPathType.CQCodePath))),
                    // MEMO : 0.14.3.0 使用json卡片发送
                    // MEMO : 0.14.4.3 json卡片出问题了, 返回原本的发送方式
                    new(setuInfo.Author, SystemId, setuInfo.SourceUrl),
                    //new(BOT_NAME, BotId, await CQCode.JsonCard_TianxuanShareAsync(
                    //    "查看大图", setuInfo.SourceText, $"{setuInfo.SetuType}",
                    //    setuInfo.SourceUrl, _setuIcons.Random()).ConfigureAwait(false)),
                };

                var bonusTimes = 1;
                if (addLevel is SetuAddLevel.ExtraDouble or SetuAddLevel.FreeExtraDouble)
                {
                    while (Rand.CheckPercent(GetExtraPercent()))
                    {
                        var (bonusSetuInfo, bonusFileName) = await GetSetu(() => isSearchTag
                                ? randomSetuKeyword.TryGetRandomWeight(out var funcResult)
                                    ? funcResult.Value(tag)
                                    : randomSetuDefault.Random()(tag)
                                : randomSetu.TryGetRandomWeight(out var funcResult2)
                                    ? funcResult2.Value(tag)
                                    : randomSetuDefault.Random()(tag),
                            false, false).ConfigureAwait(false);
                        if (bonusSetuInfo is { Result: SetuResult.Successed })
                        {
                            sendMessages.Add(new GroupForwardMessage(BOT_NAME, BotId,
                                $"你获得了额外的色图+{bonusTimes}{new string('!', bonusTimes)}"));
                            sendMessages.Add(new GroupForwardMessage($"{bonusSetuInfo.SetuType}", SystemId,
                                CQCode.Image(CommonExtensions.GetPath(PATH_CACHE_IMAGE, bonusFileName, GetPathType.CQCodePath))));
                            //sendMessages.Add(new GroupForwardMessage($"{bonusSetuInfo.SetuType}", BotId,
                            //    $"{bonusSetuInfo.SourceText}" +
                            //    $"{ENTER}{_setuSource.Random()}:{bonusSetuInfo.SourceUrl}"));
                            sendMessages.Add(new GroupForwardMessage(bonusSetuInfo.Author, SystemId, bonusSetuInfo.SourceUrl));
                            //sendMessages.Add(new GroupForwardMessage(
                            //    BOT_NAME, BotId, await CQCode.JsonCard_TianxuanShareAsync(
                            //        "查看大图", bonusSetuInfo.SourceText, $"{bonusSetuInfo.SetuType}",
                            //        bonusSetuInfo.SourceUrl, _setuIcons.Random()).ConfigureAwait(false)));
                        }
                        else
                        {
                            sendMessages.Add(new GroupForwardMessage(BOT_NAME, BotId,
                                $"你获得了额外的色图+{bonusTimes}{new string('!', bonusTimes)}" +
                                $"{ENTER}但是获取失败了!"));
                        }

                        bonusTimes++;
                    }

                    int GetExtraPercent()
                    {
                        return bonusTimes switch
                        {
                            1 => 100,
                            2 => 60,
                            3 => 45,
                            4 => 35,
                            5 => 30,
                            6 => 25,
                            _ => 20,
                        };
                    }
                }

                if (r18Bonus)
                {
                    isR18 = true;
                    var randomSetuR18Keyword = GetRandomWeightSetuInfo(isR18,
                        SetuType.Lolicon, SetuType.Lolisuki, SetuType.NyanCatda, SetuType.Yuban, SetuType.Jitsu);
                    var randomSetuR18 = GetRandomWeightSetuInfo(isR18, Enum.GetValues<SetuType>());
                    var (setuInfoR18, _) = await GetSetu(() => isSearchTag
                            ? randomSetuR18Keyword.TryGetRandomWeight(out var funcResult)
                                ? funcResult.Value(tag)
                                : randomSetuDefault.Random()(tag)
                            : randomSetuR18.TryGetRandomWeight(out var funcResult2)
                                ? funcResult2.Value(tag)
                                : randomSetuDefault.Random()(tag),
                        false, true).ConfigureAwait(false);
                    if (setuInfoR18 == null)
                        return false;

                    switch (setuInfoR18.Result)
                    {
                        case SetuResult.Successed:
                            //sendMessages.Add(new GroupForwardMessage(BOT_NAME, BotId,
                            //    $"[这是一张额外的金色传说{sourceTag}色图, 不可预览]"));
                            //sendMessages.Add(new GroupForwardMessage(BOT_NAME, BotId, $"{setuInfoR18.SourceText}"));
                            //sendMessages.Add(new GroupForwardMessage(BOT_NAME, BotId,
                            //    await CQCode.JsonCard_StructMsg("点击查看大图", $"API提供: {setuInfo.SetuType}",
                            //    setuInfo.SourceUrl, SETUAPI_ICON).ConfigureAwait(false)));
                            sendMessages.Add(new GroupForwardMessage($"{setuInfoR18.SetuType}", SystemId,
                                $"[这是一张额外的金色传说{sourceTag}色图, 不可预览]"));
                            //sendMessages.Add(new GroupForwardMessage($"{setuInfoR18.SetuType}", BotId,
                            //    $"{setuInfoR18.SourceText}" +
                            //    $"{ENTER}{_setuSource.Random()}:{setuInfoR18.SourceUrl}"));
                            sendMessages.Add(new GroupForwardMessage(setuInfoR18.Author, SystemId, setuInfoR18.SourceUrl));
                            //sendMessages.Add(new GroupForwardMessage(
                            //    BOT_NAME, BotId, await CQCode.JsonCard_TianxuanShareAsync(
                            //        "查看大图", setuInfoR18.SourceText, $"{setuInfoR18.SetuType}",
                            //        setuInfoR18.SourceUrl, _setuIcons.Random()).ConfigureAwait(false)));
                            break;
                        case SetuResult.NoSearchResult:
                            sendMessages.Add(new GroupForwardMessage($"{setuInfo.SetuType}", SystemId,
                                $"{_setuKexiStart.Random()} " +
                                $"色图库中没找到金色传说色图~, {_setuKexiEnd.Random()}"));
                            break;
                        case SetuResult.ApiError:
                        case SetuResult.Timeout:
                        case SetuResult.OtherError:
                        case SetuResult.ApiR18ReviewError:
                            sendMessages.Add(new GroupForwardMessage(BOT_NAME, BotId,
                                $"{_setuKexiStart.Random()} " +
                                $"{setuInfo.Result.GetDisplay()}[{setuInfo.SetuType}],金色传说色图取得失败!{_setuKexiEnd.Random()}"));
                            //await Api.SendGroupMessageAsync(groupId,
                            //    $"{CQCode.At(targetId)}{_setuKexiStart.Random()}" +
                            //    $"{setuInfo.Result.GetDisplay()}[{setuInfo.SetuType}],金色传说色图取得失败!{_setuKexiEnd.Random()} {GetSetuLvInfo()}")
                            //    .ConfigureAwait(false);
                            await BotDb.AddAsync(new SetuSendHistory(senderId, dateNow, sourceTag, true, false, isFree, true))
                                .ConfigureAwait(false);
                            break;
                    }
                }

                await BotDb.AddAsync(new SetuSendHistory(senderId, dateNow, sourceTag, true, true, isFree, r18Bonus))
                    .ConfigureAwait(false);
                await BotServer.SendGroupForwardMessageAsync(groupId, sendMessages,
                        $"{groupMessage.Sender.NickName}的色图",
                        (isSearchTag ? new[] { $"关键字: {sourceTag}" } : []).Concat(GetSetuLvInfo()).ToArray(),
                        $"查看所有{bonusTimes + (r18Bonus ? 1 : 0)}张色图", "[色图]",
                        15, RunAction)
                    .ConfigureAwait(false);

                async void RunAction(ClientReceiveData clientReceiveData)
                {
                    if (clientReceiveData?.IsSuccessed != false)
                        return;

                    // MEMO : 消息被风控, 发送文字消息
                    sendMessages[2] = new GroupForwardMessage($"{setuInfo.SetuType}", BotId, "[该消息已被风控拦截!]");
                    await BotServer.SendGroupForwardMessageAsync(groupId, sendMessages).ConfigureAwait(false);
                }
            }
            catch
            {
                return false;
            }

            void SetSetuValues(SendSetuConfig sendSetuConfig)
            {
                addLevel = sendSetuConfig.SetuAddLevel;
                addCDReason = sendSetuConfig.AddCDReason;
                addSecond = sendSetuConfig.AddSecond > 0
                    ? (int)(sendSetuConfig.AddSecond * (setuDoushiLv + 3.0) / 3)
                    : sendSetuConfig.AddSecond;
                canSendSetu = sendSetuConfig.CanSend;
                r18Bonus = sendSetuConfig.R18;
            }

            void AddCD()
            {
                setuDoushiInfo.SetuCD = (setuCd > dateNow ? setuCd : dateNow)
                    .AddSeconds(addSecond).ToTimeStamp();
                UpdateSetuDoushiInfo(setuDoushiInfo);
            }

            string[] GetSetuLvInfo()
            {
                var addString = string.Empty;
                if (changeLvTime < 0)
                    addString += $",冷却{changeLvTime.ToSignString()}";
                if (isSearchTag && changeLvTag > 0)
                    addString += $",搜索{changeLvTag.ToSignString()}";
                if (changeLvFast > 0)
                    addString += $",频率快{changeLvFast.ToSignString()}";
                if (!string.IsNullOrEmpty(addString))
                    addString = addString[1..];

                if (addSetuSenderLv != 0)
                {
                    return
                    [
                        $"[斗士Lv{oldSetuSenderLv}] 本次{addSetuSenderLv.ToSignString()}",
                        addString,
                    ];
                }
                else
                {
                    return
                    [
                        $"[斗士Lv{oldSetuSenderLv}]",
                    ];
                }
            }

            async Task<(SetuInfo, string)> GetSetu(
                Func<Task<SetuInfo>> getSetuInfoFunc,
                bool sendDownloadingMessage,
                bool checkImageOnly)
            {
                var setuInfo = await getSetuInfoFunc().ConfigureAwait(false);
                //if (sendDownloadingMessage)

                if (setuInfo.Result == SetuResult.Successed
                    && File.Exists(Path.Combine(PATH_CACHE_IMAGE, setuInfo.FullCacheFileName)))
                {
                    if (PublicVar.IsDebug)
                    {
                        await BotServer.SendGroupMessageAsync(groupId, $"[DEBUG]已存在缓存{setuInfo.FullCacheFileName}!")
                            .ConfigureAwait(false);
                    }

                    // MEMO : 缓存中存在该图, 跳过下载
                    return (setuInfo, setuInfo.FullCacheFileName);
                }
                DebugSendSetuInfo(setuInfo);

                var fileName = string.Empty;
                var getSuccessed = false;
                const int maxRetryTimes = 4;
                var retryTimes = 0;
                while (!getSuccessed && retryTimes <= maxRetryTimes)
                {
                    if (setuInfo.Result == SetuResult.Successed)
                    {
                        (getSuccessed, fileName) = await HttpExtensions.HttpDownloadAsync(
                            setuInfo.ImageUrl, PATH_CACHE_IMAGE, true, checkImageOnly, setuInfo.CacheFileName).ConfigureAwait(false);
                        if (getSuccessed)
                            continue;
                    }
                    else if (setuInfo.Result == SetuResult.NoSearchResult)
                    {
                        getSuccessed = true;
                        continue;
                    }

                    retryTimes++;
                    setuInfo = await getSetuInfoFunc().ConfigureAwait(false);
                    if (File.Exists(Path.Combine(PATH_CACHE_IMAGE, setuInfo.FullCacheFileName)))
                    {
                        if (PublicVar.IsDebug)
                        {
                            await BotServer.SendGroupMessageAsync(groupId, $"[DEBUG]已存在缓存{setuInfo.FullCacheFileName}!")
                                .ConfigureAwait(false);
                        }

                        // MEMO : 缓存中存在该图, 跳过下载
                        return (setuInfo, setuInfo.FullCacheFileName);
                    }

                    DebugSendSetuInfo(setuInfo);
                    CommonExtensions.Sleep(500);
                }

                if (retryTimes > maxRetryTimes)
                {
                    //await BotServer.SendGroupMessageAsync(groupId, "超过重试次数上限,放弃下载!").ConfigureAwait(false);
                    setuInfo.Result = SetuResult.ApiError;
                    return (setuInfo, string.Empty);
                }

                if (!checkImageOnly && setuInfo.Result == SetuResult.Successed)
                {
                    var isFileExists = false;
                    while (!isFileExists)
                    {
                        isFileExists = File.Exists($"{PATH_CACHE_IMAGE}/{fileName}");
                        CommonExtensions.Sleep(100);
                    }

                    CommonExtensions.DeleteExpiredCache();
                }

                return (setuInfo, fileName);

                async void DebugSendSetuInfo(SetuInfo stInfo)
                {
                    if (!PublicVar.IsDebug)
                        return;

                    if (stInfo.Result == SetuResult.Successed)
                    {
                        await BotServer.SendGroupMessageAsync(groupId, "[DEBUG]SetuInfo" +
                                $"{ENTER}SetuType: {stInfo.SetuType}" +
                                $"{ENTER}SetuResult: {stInfo.Result}" +
                                $"{ENTER}SmallUrl: {stInfo.ImageUrl}" +
                                $"{ENTER}SourceUrl: {stInfo.SourceUrl}")
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        await BotServer.SendGroupMessageAsync(groupId, "[DEBUG]SetuInfo" +
                                $"{ENTER}SetuType: {stInfo.SetuType}" +
                                $"{ENTER}SetuResult: {stInfo.Result}")
                            .ConfigureAwait(false);
                    }
                }
            }
        }
        else
        {
            if (message.ContainsAny(_setuBuman.Keys, out var findedStr) && (dateNow - setuSendHistory).TotalSeconds <= 15)
            {
                var randActions = new List<RandomWeight<int>>
                {
                    new(1000, 0),
                    new(100, 1),
                    new(30, 2),
                    new(5, 3),
                };
                var addSetuSenderLv = 0;
                if (randActions.TryGetRandomWeight(out var item))
                    addSetuSenderLv = item.Value;

                setuDoushiInfo.SetuDoushiLv = setuDoushiLv + addSetuSenderLv;
                var addMinutes = _setuBuman[findedStr];
                setuDoushiInfo.BlackListCD = dateNow.AddMinutes(Rand.Next(addMinutes / 2, addMinutes)).ToTimeStamp();
                UpdateSetuDoushiInfo(setuDoushiInfo);
                await BotServer.SendMessageEmojiAsync(messageId, Emoji.E_Beat).ConfigureAwait(false);
                return true;
            }

            return true;
        }

        return true;

        string GetCD(SetuDoushiInfo stdsInfo)
        {
            var cd = (stdsInfo?.SetuCD ?? 0).ToDateTime();
            return dateNow >= cd ? "可使用" : cd.ToString("HH:mm:ss");
        }
    }

    private static List<RandomWeight<Func<string, Task<SetuInfo>>>> GetRandomWeightSetuInfo(
        bool isR18,
        params SetuType[] setuTypes)
    {
        var list = new List<RandomWeight<Func<string, Task<SetuInfo>>>>();
        setuTypes.ForEach(setuType => list.Add(GetRandomWeight(setuType)));
        return list;

        RandomWeight<Func<string, Task<SetuInfo>>> GetRandomWeight(SetuType setuType)
            => setuType switch
            {
                SetuType.Lolicon => new RandomWeight<Func<string, Task<SetuInfo>>>(_setuWeight[setuType],
                    isR18 ? SetuExtensions.GetSetu_Lolicon_R18Async : SetuExtensions.GetSetu_LoliconAsync),
                SetuType.Lolisuki => new RandomWeight<Func<string, Task<SetuInfo>>>(_setuWeight[setuType],
                    isR18 ? SetuExtensions.GetSetu_Lolisuki_R18Async : SetuExtensions.GetSetu_LolisukiAsync),
                SetuType.Yuban => new RandomWeight<Func<string, Task<SetuInfo>>>(_setuWeight[setuType],
                    isR18 ? SetuExtensions.GetSetu_Yuban_R18Async : SetuExtensions.GetSetu_YubanAsync),
                SetuType.NyanCatda => new RandomWeight<Func<string, Task<SetuInfo>>>(_setuWeight[setuType],
                    isR18 ? SetuExtensions.GetSetu_NyanCatda_R18Async : SetuExtensions.GetSetu_NyanCatdaAsync),
                SetuType.Jitsu => new RandomWeight<Func<string, Task<SetuInfo>>>(_setuWeight[setuType],
                    isR18 ? SetuExtensions.GetSetu_Jitsu_R18Async : SetuExtensions.GetSetu_JitsuAsync),
                SetuType.JitsuSelf => new RandomWeight<Func<string, Task<SetuInfo>>>(_setuWeight[setuType],
                    isR18 ? SetuExtensions.GetSetu_JitsuSelf_R18Async : SetuExtensions.GetSetu_JitsuSelfAsync),
                _ => throw new ArgumentOutOfRangeException(nameof(setuType), setuType, null),
            };
    }

    /// <summary>
    /// 取得色图斗士信息(带缓存)
    /// </summary>
    /// <param name="senderId">QQID</param>
    /// <param name="addToCache">是否添加到缓存</param>
    /// <returns>色图斗士信息</returns>
    private static async Task<SetuDoushiInfo> GetSetuDoushiInfoAsync(long senderId, bool addToCache = true)
    {
        if (SetuDoushiInfoCache.TryGetValue(senderId, out var setuDoushiInfo))
            return setuDoushiInfo;

        lock (BotDb.SyncLock)
            setuDoushiInfo = BotDb.SetuDoushiInfos.FindAsync(senderId).Result;

        if (setuDoushiInfo == null && addToCache)
        {
            setuDoushiInfo = new SetuDoushiInfo(senderId);
            await BotDb.AddAsync(setuDoushiInfo).ConfigureAwait(false);
        }

        if (addToCache)
        {
            SetuDoushiInfoCache.AddOrUpdate(senderId,
                _ => setuDoushiInfo,
                (_, __) => setuDoushiInfo);
        }

        return setuDoushiInfo;
    }

    private static void UpdateSetuDoushiInfo(SetuDoushiInfo setuDoushiInfo)
    {
        var targetId = setuDoushiInfo.TargetId;
        SetuDoushiInfoCache.AddOrUpdate(targetId,
            _ => setuDoushiInfo,
            (_, __) => setuDoushiInfo);
        BotDb.Update(setuDoushiInfo);
    }
}

/// <summary>
/// 色图发送配置(加时间等)
/// </summary>
public class SendSetuConfig
{
    public int AddSecond { get; set; }
    public AddCDReason AddCDReason { get; set; }
    public SetuAddLevel SetuAddLevel { get; set; }
    public bool CanSend { get; set; }
    public bool R18 { get; set; }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public SendSetuConfig(
        int addSecond,
        AddCDReason addCDReason,
        SetuAddLevel setuAddLevel,
        bool canSend = false,
        bool r18 = false)
    {
        AddSecond = addSecond;
        AddCDReason = addCDReason;
        SetuAddLevel = setuAddLevel;
        CanSend = canSend;
        R18 = r18;
    }
}

/// <summary>
/// 色图Lv拓展方法
/// </summary>
public static class SetuAddLevelUtil
{
    /// <summary>
    ///
    /// </summary>
    public static string ToAddLevelString(this SetuAddLevel setuAddLevel)
        => setuAddLevel switch
        {
            SetuAddLevel.Normal => "增加",
            SetuAddLevel.Double => "加倍",
            SetuAddLevel.SuperDouble => "超级加倍",
            SetuAddLevel.Golden => "黄金加倍",
            SetuAddLevel.Platinum => "白金加倍",
            SetuAddLevel.Diamond => "钻石加倍",
            SetuAddLevel.Death => "死亡加倍",
            SetuAddLevel.Luck => "幸运",
            SetuAddLevel.LuckSuper => "超级幸运",
            SetuAddLevel.LuckGolden => "黄金幸运",
            SetuAddLevel.Free => "白嫖",
            SetuAddLevel.ExtraDouble => "双倍色图",
            _ => throw new ArgumentOutOfRangeException(nameof(setuAddLevel), setuAddLevel, null),
        };
}

/// <summary>
/// 增加CD原因
/// </summary>
public enum AddCDReason
{
    /// <summary>
    /// 请求成功(正常加CD)
    /// </summary>
    RequestSuccessed,

    /// <summary>
    /// 请求失败(脸黑)
    /// </summary>
    RequestFailed,

    /// <summary>
    /// CD没准备好
    /// </summary>
    NotReady,
}

/// <summary>
/// 色图加倍等级
/// </summary>
public enum SetuAddLevel
{
    /// <summary>
    /// 普通
    /// </summary>
    Normal,

    /// <summary>
    /// 加倍
    /// </summary>
    Double,

    /// <summary>
    /// 超级加倍
    /// </summary>
    SuperDouble,

    /// <summary>
    /// 黄金加倍
    /// </summary>
    Golden,

    /// <summary>
    /// 白金加倍
    /// </summary>
    Platinum,

    /// <summary>
    /// 钻石加倍
    /// </summary>
    Diamond,

    /// <summary>
    /// 死亡加倍
    /// </summary>
    Death,

    /// <summary>
    /// 幸运
    /// </summary>
    Luck,

    /// <summary>
    /// 超级幸运
    /// </summary>
    LuckSuper,

    /// <summary>
    /// 黄金幸运
    /// </summary>
    LuckGolden,

    /// <summary>
    /// 白嫖
    /// </summary>
    Free,

    /// <summary>
    /// 双倍色图
    /// </summary>
    ExtraDouble,

    /// <summary>
    /// 白嫖双倍色图
    /// </summary>
    FreeExtraDouble,
}