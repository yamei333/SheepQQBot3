using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Enums;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.QQ;
using SheepQQBot3.Model.Setu;
using System;
using System.Collections.Concurrent;
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
    /// <summary>
    /// 色图斗士信息缓存
    /// </summary>
    private static readonly ConcurrentDictionary<string, SetuDoushiInfo> SetuDoushiInfoCache = [];

    /// <summary>
    /// 色图请求历史记录
    /// </summary>
    private static readonly ConcurrentDictionary<string, DateTime> SetuRequestHistories = [];

    //private const string[] SETUAPI_ICONS = "https://lolicon.app/favicon.ico";
    private static readonly string[] _setuIcons = [
        "https://i0.hdslb.com/bfs/garb/66a0850681611ac4dede74823a34e197913fb97f.png",
        "https://i0.hdslb.com/bfs/garb/6f57a2e1d8cedd68f5b837a7118bba37275ba5c4.png",
        "https://i0.hdslb.com/bfs/garb/20e2b689aebcd2fa8bbe1ffa440827c3062cc420.png",
        "https://i0.hdslb.com/bfs/garb/ac9c50a5751f9b02af366c9096b703a8889d7ea4.png",
        "https://i0.hdslb.com/bfs/garb/177b264dcdc99a107481ae8e7ead45dc7448dbd7.png",
        "https://i0.hdslb.com/bfs/garb/8b6d3154bd4eb96df7603163cffc1db407d92bf2.png",
    ];

    private static readonly Regex _regCorrectSetuMessage = new(
        @"^(?!.*(\[CQ:|@)).*?色图[a-zA-Z]?$",
        RegexOptions.Singleline
    );

    /// <summary>
    /// 缓存文件夹名称
    /// </summary>
    private const string PATH_CACHE_IMAGE = "Cache";

    /// <summary>
    /// 色图的基础CD, 不能发得太频繁
    /// </summary>
    private const int SETU_BASE_CD = 120;

    private static readonly Dictionary<SetuType, int> _setuWeight = new()
    {
        {SetuType.Lolicon, 30},
        {SetuType.Lolisuki, 20},
        {SetuType.Yuban, 4},
        {SetuType.NyanCatda, 10},
        {SetuType.Jitsu, 2},
        {SetuType.JitsuSelf, 6},
        {SetuType.NekosiaCat, 15},
        {SetuType.Mossia, 25},
    };

    #region 不用的变量集

    ///// <summary>
    ///// 色图不满时的反击关键词, 拉黑时间为分钟
    ///// </summary>
    //private static readonly Dictionary<string, int> _setuBuman = new()
    //{
    //    {"这也", 60},
    //    {"一般", 60},
    //    {"不.{0,3}色", 120},
    //    {"不.{0,3}行", 120},
    //    {"太小", 120},
    //    {"就这", 300},
    //    {"菜", 300},
    //    {"^.{1,5}吗", 60},
    //};

    //private static readonly string[] _setuYouwant =
    //[
    //    string.Empty, "你要的", "你点的", "请求的", "申请的", "需求的",
    //];

    //private static readonly string[] _setuGetted =
    //[
    //    "来了", "已经送出", "到了", "来咯", "lei了", "已发送", "给你了",
    //];

    //private static readonly string[] _setuSource =
    //[
    //    "原图", "大图", "查看大图", "原图查看", "源链接", "图源",
    //];

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

    #endregion 不用的变量集

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
        "男",
    ];

    /// <summary>
    /// 随机色图
    /// </summary>
    /// <param name="blackListUserConfig">黑名单配置</param>
    /// <param name="groupMessage"><see cref="GroupMessage"/></param>
    public static async Task RandomSetuAsync(BlackListUserConfig blackListUserConfig, GroupMessage groupMessage)
    {
        if (blackListUserConfig.BanedSetu)
            return;

        var message = groupMessage.Message;
        // MEMO : 色图请求格式检查
        if (!_regCorrectSetuMessage.IsMatch(message))
            return;

        var groupId = groupMessage.GroupId;
        var senderId = groupMessage.Sender.UserId.ToString();
        var messageId = groupMessage.MessageId;
        var dateNow = DateTime.Now;

        var setuDoushiInfo = SetuDoushiInfoCache.GetOrAdd(senderId, () => new SetuDoushiInfo(senderId));
        // MEMO : 色图黑名单检测
        if ((dateNow - setuDoushiInfo.BlackListCD.ToDateTime()).TotalMicroseconds < 0)
        {
            await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.DogeBig).ConfigureAwait(false);
            return;
        }

        var setuCd = setuDoushiInfo.SetuCD.ToDateTime();
        var isAdmin = BotExtensions.IsAdmin(senderId);

        SetuType? targetSetuApiType = null;

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
            case "C":
                message = message[..^1];
                targetSetuApiType = SetuType.NekosiaCat;
                break;
            case "M":
                message = message[..^1];
                targetSetuApiType = SetuType.Mossia;
                break;
        }

        // MEMO : 字数在一定以内
        if (message.GetByteCount() > 45)
            return;

        var tag = message[..^2];
        if (_banKeywordList.Contains(tag))
        {
            setuDoushiInfo.BlackListCD = dateNow.AddHours(336).ToTimeStamp();
            setuDoushiInfo.SetuCD = dateNow.ToTimeStamp();
            UpdateSetuDoushiInfo(setuDoushiInfo);
            await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.E_Beat).ConfigureAwait(false);
            return;
        }

        var sourceTag = tag;
        // MEMO : tag映射
        if (_tagDictionary.TryGetValue(tag.ToUpper(), out var changeTag))
            tag = changeTag;

        var isSearchTag = !tag.IsNullOrEmpty();
        if ((dateNow - SetuRequestHistories.GetOrAdd(senderId, () => DateTime.MinValue)).TotalSeconds <= 5)
        {
            if (setuDoushiInfo.ToFastTimes >= 2)
            {
                // MEMO : 连续刷则关小黑屋
                await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.E_Beat).ConfigureAwait(false);
                setuDoushiInfo.ToFastTimes = 0;
                setuDoushiInfo.BlackListCD = dateNow.AddHours(1).ToTimeStamp();
            }
            else
            {
                // MEMO : 一定时间内不连续响应
                await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.Coffee).ConfigureAwait(false);
                setuDoushiInfo.ToFastTimes += 1;
            }

            UpdateSetuDoushiInfo(setuDoushiInfo);
            return;
        }

        SetuRequestHistories.AddOrUpdate(senderId, dateNow, (_, _) => dateNow);
        setuDoushiInfo.ToFastTimes = 0;
        var r18Bonus = false;
        var addSecond = 0;
        var addLevel = SetuAddLevel.Normal;
        var addCDReason = AddCDReason.RequestSuccessed;
        var canSendSetu = false;

        if (!IsDebug && isAdmin)
        {
            // MEMO : ADMIN无限制要色图
            canSendSetu = true;
        }
        else
        {
            List<RandomWeight<SendSetuConfig>> randActions;
            if (dateNow > setuCd)
            {
                // MEMO : 0.13.3.16 除了基数其他都x6, 以维持10%暴击率
                randActions = new List<RandomWeight<SendSetuConfig>>
                {
                    new(20000, new SendSetuConfig(SETU_BASE_CD + Rand.Next(120), AddCDReason.RequestSuccessed, SetuAddLevel.Normal, true)),
                    new(2100, new SendSetuConfig(SETU_BASE_CD + Rand.Next(120), AddCDReason.RequestSuccessed, SetuAddLevel.ExtraDouble, true)),
                    new(900, new SendSetuConfig(0, AddCDReason.RequestSuccessed, SetuAddLevel.Free, true)),
                    new(540, new SendSetuConfig(0, AddCDReason.RequestSuccessed, SetuAddLevel.FreeExtraDouble, true)),
                    new(180, new SendSetuConfig(SETU_BASE_CD + Rand.Next(-60, 60), AddCDReason.RequestSuccessed, SetuAddLevel.Normal, true, true)),
                    new(120, new SendSetuConfig(SETU_BASE_CD + Rand.Next(-60, 60), AddCDReason.RequestSuccessed, SetuAddLevel.ExtraDouble, true, true)),
                    new(60, new SendSetuConfig(0, AddCDReason.RequestSuccessed, SetuAddLevel.Free, true, true)),
                    new(36, new SendSetuConfig(0, AddCDReason.RequestSuccessed, SetuAddLevel.FreeExtraDouble, true, true)),
                };
            }
            else
            {
                if ((setuCd - dateNow).TotalSeconds >= 300)
                {
                    // MEMO : CD5分钟以上, 老实等着吧
                    await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.Coffee).ConfigureAwait(false);
                    return;
                }

                randActions = new List<RandomWeight<SendSetuConfig>>
                {
                    new(10000, new SendSetuConfig(Rand.Next(5, 45), AddCDReason.NotReady, SetuAddLevel.Normal)),
                    new(3500, new SendSetuConfig(Rand.Next(5, 45) * 2, AddCDReason.NotReady, SetuAddLevel.Double)),
                    new(1500, new SendSetuConfig(Rand.Next(5, 45) * 4, AddCDReason.NotReady, SetuAddLevel.SuperDouble)),
                    new(750, new SendSetuConfig(Rand.Next(5, 45) * 8, AddCDReason.NotReady, SetuAddLevel.Golden)),
                    new(300, new SendSetuConfig(Rand.Next(5, 45) * 16, AddCDReason.NotReady, SetuAddLevel.Platinum)),
                    new(150, new SendSetuConfig(Rand.Next(5, 45) * 32, AddCDReason.NotReady, SetuAddLevel.Diamond)),
                    new(10, new SendSetuConfig(Rand.Next(5, 45) * 256, AddCDReason.NotReady, SetuAddLevel.Death)),
                    new(1500, new SendSetuConfig(Rand.Next(10, 90) * -1, AddCDReason.NotReady, SetuAddLevel.Luck)),
                    new(600, new SendSetuConfig(Rand.Next(10, 90) * -4, AddCDReason.NotReady, SetuAddLevel.LuckSuper)),
                    new(200, new SendSetuConfig(Rand.Next(10, 90) * -16, AddCDReason.NotReady, SetuAddLevel.LuckGolden)),
                };
            }

            randActions.TryGetRandomWeight(out var resultAction);
            SetSetuValues(resultAction.Value);

            // MEMO : 参数注入意图
            if (tag!.Contains('&') || tag!.Contains("%26"))
            {
                setuDoushiInfo.BlackListCD = dateNow.AddHours(24).ToTimeStamp();
                setuDoushiInfo.SetuCD = dateNow.ToTimeStamp();
                UpdateSetuDoushiInfo(setuDoushiInfo);
                await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.E_Beat).ConfigureAwait(false);
                return;
            }

            UpdateSetuDoushiInfo(setuDoushiInfo);
        }

        if (IsDebug)
        {
            await GlobalBotClient.SendGroupMessageAsync(groupId, "[DEBUG]"
                    + $"{ENTER}目标对象: {senderId}"
                    + $"{ENTER}是否发送: {canSendSetu}"
                    + $"{ENTER}增加时间: {addSecond}s"
                    + $"{ENTER}色图CD: {GetCD(setuDoushiInfo)}")
                .ConfigureAwait(false);
        }

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
                        await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.Moyu).ConfigureAwait(false);
                        break;
                    case AddCDReason.NotReady:
                        await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.Coffee).ConfigureAwait(false);
                        break;
                    default:
                        // MEMO : 应该不会有此Case
                        await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.Boom).ConfigureAwait(false);
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

            if (!sendMessage.IsNullOrEmpty())
                await GlobalBotClient.SendGroupMessageAsync(groupId, sendMessage).ConfigureAwait(false);

            return;
        }
        else
        {
            if (!isAdmin && addSecond == 0)
            {
                // MEMO : 白嫖
                await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.Plus1).ConfigureAwait(false);
                goto SendSetu;
            }

            // MEMO : 白嫖不加CD
            AddCD();
        }

    SendSetu:
        await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.E_OK).ConfigureAwait(false);

        try
        {
            var randomSetuKeyword = targetSetuApiType switch
            {
                SetuType.Lolicon => GetRandomWeightSetuInfo(false, SetuType.Lolicon),
                SetuType.Lolisuki => GetRandomWeightSetuInfo(false, SetuType.Lolisuki),
                SetuType.NyanCatda => GetRandomWeightSetuInfo(false, SetuType.NyanCatda),
                SetuType.Yuban => GetRandomWeightSetuInfo(false, SetuType.Yuban),
                SetuType.Jitsu => GetRandomWeightSetuInfo(false, SetuType.Jitsu),
                _ => GetRandomWeightSetuInfo(false,
                    SetuType.Lolicon, SetuType.Lolisuki, SetuType.NyanCatda, SetuType.Yuban, SetuType.Jitsu),
            };

            var randomSetuR18Keyword = GetRandomWeightSetuInfo(true,
                SetuType.Lolicon, SetuType.Lolisuki, SetuType.NyanCatda, SetuType.Yuban, SetuType.Jitsu);

            var randomSetu = targetSetuApiType switch
            {
                SetuType.Lolicon => GetRandomWeightSetuInfo(false, SetuType.Lolicon),
                SetuType.Lolisuki => GetRandomWeightSetuInfo(false, SetuType.Lolisuki),
                SetuType.NyanCatda => GetRandomWeightSetuInfo(false, SetuType.NyanCatda),
                SetuType.Yuban => GetRandomWeightSetuInfo(false, SetuType.Yuban),
                SetuType.Jitsu => GetRandomWeightSetuInfo(false, SetuType.Jitsu),
                SetuType.JitsuSelf => GetRandomWeightSetuInfo(false, SetuType.JitsuSelf),
                SetuType.NekosiaCat => GetRandomWeightSetuInfo(false, SetuType.NekosiaCat),
                SetuType.Mossia => GetRandomWeightSetuInfo(false, SetuType.Mossia),
                _ => GetRandomWeightSetuInfo(false, Enum.GetValues<SetuType>()),
            };
            Func<string, Task<SetuInfo>>[] randomSetuDefault =
            [
                SetuExtensions.GetSetu_LoliconAsync,
                SetuExtensions.GetSetu_LolisukiAsync,
                SetuExtensions.GetSetu_NyanCatdaAsync,
                SetuExtensions.GetSetu_YubanAsync,
                SetuExtensions.GetSetu_JitsuAsync,
                SetuExtensions.GetSetu_JitsuSelfAsync,
                SetuExtensions.GetSetu_NekosiaCat_Async,
                SetuExtensions.GetSetu_MossiaAsync,
            ];
            var (setuInfo, fileName) = await GetSetu(() => isSearchTag
                    ? randomSetuKeyword.TryGetRandomWeight(out var funcResult)
                        ? funcResult.Value(tag)
                        : randomSetuDefault.Random()(tag)
                    : randomSetu.TryGetRandomWeight(out var funcResult2)
                        ? funcResult2.Value(tag)
                        : randomSetuDefault.Random()(tag),
                false).ConfigureAwait(false);
            if (setuInfo == null)
                return;

            switch (setuInfo.Result)
            {
                case SetuResult.Successed:
                    break;
                case SetuResult.NoSearchResult:
                    await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.Knock).ConfigureAwait(false);
                    return;
                case SetuResult.ApiError:
                case SetuResult.Timeout:
                case SetuResult.OtherError:
                case SetuResult.ApiR18ReviewError:
                    await GlobalBotClient.SendMessageEmojiAsync(messageId, Emoji.Boom).ConfigureAwait(false);
                    return;
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
                        false).ConfigureAwait(false);
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
                var randomSetuR18 = GetRandomWeightSetuInfo(true, Enum.GetValues<SetuType>());
                var (setuInfoR18, _) = await GetSetu(() => isSearchTag
                        ? randomSetuR18Keyword.TryGetRandomWeight(out var funcResult)
                            ? funcResult.Value(tag)
                            : randomSetuDefault.Random()(tag)
                        : randomSetuR18.TryGetRandomWeight(out var funcResult2)
                            ? funcResult2.Value(tag)
                            : randomSetuDefault.Random()(tag),
                    true).ConfigureAwait(false);
                if (setuInfoR18 == null)
                    return;

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
                        break;
                }
            }

            await GlobalBotClient.SendGroupForwardMessageAsync(groupId, sendMessages,
                    $"{groupMessage.Sender.NickName}的色图",
                    (isSearchTag ? new[] { $"关键字: {sourceTag}" } : [])
                    .Concat([$"CD变化: {(addSecond >= 0 ? $"+{addSecond}" : addSecond.ToString())}s"])
                    .ToArray(),
                    $"查看所有{bonusTimes + (r18Bonus ? 1 : 0)}张色图", "[色图]",
                    RunAction)
                .ConfigureAwait(false);

            void RunAction(ClientReceiveData clientReceiveData)
            {
                if (clientReceiveData?.IsSuccessed == true)
                    return;

                // MEMO : 消息被风控, 发送文字消息
                sendMessages[^1] = new GroupForwardMessage($"{setuInfo.SetuType}", BotId, "[该消息已被风控拦截!]");
                GlobalBotClient.SendGroupForwardMessageAsync(groupId, sendMessages);
            }

            async Task<(SetuInfo, string)> GetSetu(
                Func<Task<SetuInfo>> getSetuInfoFunc,
                bool checkImageOnly)
            {
                var setuInfo = await getSetuInfoFunc().ConfigureAwait(false);
                if (setuInfo.Result == SetuResult.Successed
                    && File.Exists(Path.Combine(PATH_CACHE_IMAGE, setuInfo.FullCacheFileName)))
                {
                    if (IsDebug)
                    {
                        await GlobalBotClient.SendGroupMessageAsync(groupId, $"[DEBUG]已存在缓存{setuInfo.FullCacheFileName}!")
                            .ConfigureAwait(false);
                    }

                    // MEMO : 缓存中存在该图, 跳过下载
                    return (setuInfo, setuInfo.FullCacheFileName);
                }

                if (IsDebug)
                    await DebugSendSetuInfo(setuInfo).ConfigureAwait(false);

                const int maxRetryTimes = 5;
                var retryTimes = 1;
                while (true)
                {
                    if (setuInfo.Result == SetuResult.Successed)
                    {
                        if (File.Exists(Path.Combine(PATH_CACHE_IMAGE, setuInfo.FullCacheFileName)))
                        {
                            if (IsDebug)
                            {
                                await GlobalBotClient.SendGroupMessageAsync(groupId, $"[DEBUG]已存在缓存{setuInfo.FullCacheFileName}!")
                                    .ConfigureAwait(false);
                            }

                            // MEMO : 缓存中存在该图, 跳过下载
                            return (setuInfo, setuInfo.FullCacheFileName);
                        }

                        var (getSuccessed, fileName) = await HttpExtensions.HttpDownloadAsync(
                            setuInfo.ImageUrl, PATH_CACHE_IMAGE, checkImageOnly, customName: setuInfo.CacheFileName).ConfigureAwait(false);
                        if (getSuccessed)
                            return (setuInfo, fileName);
                    }
                    // MEMO : 0.14.8.8 任意错误都移出搜索队列
                    //else if (setuInfo.Result == SetuResult.NoSearchResult)
                    else
                    {
                        var setuType = setuInfo.SetuType.ToString();
                        if (!checkImageOnly)
                        {
                            if (randomSetuKeyword.Count == 1)
                                return (setuInfo, string.Empty);

                            randomSetuKeyword = randomSetuKeyword.Where(each => !each.Value.Method.Name.Contains(setuType)).ToList();
                        }
                        else
                        {
                            if (randomSetuR18Keyword.Count == 1)
                                return (setuInfo, string.Empty);

                            randomSetuR18Keyword = randomSetuR18Keyword!.Where(each => !each.Value.Method.Name.Contains(setuType)).ToList();
                        }
                    }

                    retryTimes++;
                    if (retryTimes > maxRetryTimes)
                        break;

                    setuInfo = await getSetuInfoFunc().ConfigureAwait(false);
                    if (IsDebug)
                        await DebugSendSetuInfo(setuInfo).ConfigureAwait(false);
                }

                await GlobalBotClient.SendGroupMessageAsync(groupId, "超过重试次数上限,放弃下载!").ConfigureAwait(false);
                setuInfo.Result = SetuResult.ApiError;
                return (setuInfo, string.Empty);

                async Task DebugSendSetuInfo(SetuInfo stInfo)
                {
                    if (stInfo.Result == SetuResult.Successed)
                    {
                        await GlobalBotClient.SendGroupMessageAsync(groupId, "[DEBUG]SetuInfo" +
                                $"{ENTER}SetuType: {stInfo.SetuType}" +
                                $"{ENTER}SetuResult: {stInfo.Result}" +
                                $"{ENTER}SmallUrl: {stInfo.ImageUrl}" +
                                $"{ENTER}SourceUrl: {stInfo.SourceUrl}")
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        await GlobalBotClient.SendGroupMessageAsync(groupId, "[DEBUG]SetuInfo" +
                                $"{ENTER}SetuType: {stInfo.SetuType}" +
                                $"{ENTER}SetuResult: {stInfo.Result}")
                            .ConfigureAwait(false);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            YameiLogExtensions.WriteLog(LogType.Warning, $"[RandomSetuAsync]{ex.Message}");
        }

        return;

        void AddCD()
        {
            setuDoushiInfo.SetuCD = (setuCd > dateNow ? setuCd : dateNow)
                .AddSeconds(addSecond).ToTimeStamp();
            UpdateSetuDoushiInfo(setuDoushiInfo);
        }

        void SetSetuValues(SendSetuConfig sendSetuConfig)
        {
            addLevel = sendSetuConfig.SetuAddLevel;
            addCDReason = sendSetuConfig.AddCDReason;
            addSecond = sendSetuConfig.AddSecond;
            canSendSetu = sendSetuConfig.CanSend;
            r18Bonus = sendSetuConfig.R18;
        }

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
                // MEMO : NekosiaCat没有R18接口, 用Lolicon的
                SetuType.NekosiaCat => new RandomWeight<Func<string, Task<SetuInfo>>>(_setuWeight[setuType],
                    isR18 ? SetuExtensions.GetSetu_Lolicon_R18Async : SetuExtensions.GetSetu_NekosiaCat_Async),
                SetuType.Mossia => new RandomWeight<Func<string, Task<SetuInfo>>>(_setuWeight[setuType],
                    isR18 ? SetuExtensions.GetSetu_Mossia_R18Async : SetuExtensions.GetSetu_MossiaAsync),
                _ => throw new ArgumentOutOfRangeException(nameof(setuType), setuType, null),
            };
    }

    private static void UpdateSetuDoushiInfo(SetuDoushiInfo setuDoushiInfo)
    {
        var targetId = setuDoushiInfo.TargetId;
        SetuDoushiInfoCache.AddOrUpdate(targetId,
            _ => setuDoushiInfo,
            (_, _) => setuDoushiInfo);
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