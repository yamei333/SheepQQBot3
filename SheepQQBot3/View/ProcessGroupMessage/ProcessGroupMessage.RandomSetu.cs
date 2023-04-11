using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.Setu;
using SheepQQBot3.SDK.Client;
using Yamei.Common;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    public static partial class ProcessGroupMessage
    {
        private static readonly object _syncKeyword = new object();

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
        private const string COMMAND_CUSTOM_GROUP_SETUQKCD_LIBRARY = "#STQKCD#";

        /// <summary>
        /// 色图清空LV命令
        /// </summary>
        private const string COMMAND_CUSTOM_GROUP_SETUQKLV_LIBRARY = "#STQKLV#";

        /// <summary>
        /// 色图清空所有命令
        /// </summary>
        private const string COMMAND_CUSTOM_GROUP_SETUQKALL_LIBRARY = "#STQKALL#";

        /// <summary>
        /// 色图斗士排行命令
        /// </summary>
        private const string COMMAND_CUSTOM_GROUP_SETURANK_LIBRARY = "#STRANK#";

        /// <summary>
        /// 缓存文件夹名称
        /// </summary>
        private const string CACHE_DIRECTORY_NAME = "Cache";

        /// <summary>
        /// 色图的基础CD, 不能发得太频繁
        /// </summary>
        private const int SendBaseDelay = 180;

        /// <summary>
        /// 最大色图斗士Lv
        /// </summary>
        private const int MaxSenderLv = 15;

        private static HashSet<string> _setuKeyWords;

        private static readonly string[] _setuBuman =
        {
            "不够", "这也", "一般", "不色", "就这", "太小"
        };

        private static readonly string[] _setuYouwant = {
            string.Empty, "你要的", "你点的", "请求的", "申请的", "需求的"
        };

        private static readonly string[] _setuGetted = {
            "来了", "已经送出", "到了", "来咯", "lei了", "已发送", "给你了"
        };

        private static readonly string[] _setuSource = {
            "原图", "大图", "查看大图", "原图查看", "源链接", "图源"
        };

        private static readonly string[] _setuNo = {
            "别", "憋", "鳖", "No"
        };

        private static readonly string[] _setuSendLe = {
            "发了", "要了", "整了", "冲了", "弄了"
        };

        private static readonly string[] _setuGetting = {
            "下载中", "传送中", "获取中", "取得中", "载入中"
        };

        private static readonly string[] _setuCDWasAdded =
        {
            "被$ADD_LEVEL$了!", "被$ADD_LEVEL$, 时间延长了!", "被$ADD_LEVEL$, 大加特加了!",
        };

        private static readonly string[] _setuCDWasReduced =
        {
            "色图的CD发生了$ADD_LEVEL$变化, 被减少了!",
        };

        private static readonly string[] _setuKexiStart = {
            "太可惜了!", "Taxi了!", "悲剧啊!", "尬住了!", "寄了!", "鸡了!", "JI了!"
        };

        private static readonly string[] _setuKexiEnd = {
            "我的我的, 哈哈", "都怪ruojiji2", "今日不宜色图", "吔?你的XP有点怪", "一定是关键字太怪了", "图库懂的都没你多"
        };

        private static readonly Dictionary<string, string> _tagDictionary = new Dictionary<string, string>
        {
            { "导师", "甘雨" },
            { "RJJ", "甘雨" },
            { "RJJ2", "甘雨" },
            { "RUOJIJI", "甘雨" },
            { "RUOJIJI2", "甘雨" },
            { "车万", "东方" },
            { "铜", "萝莉" },
        };

        /// <summary>
        /// 随机色图
        /// </summary>
        /// <param name="botConfig">配置</param>
        /// <param name="groupMessage"><see cref="GroupMessage"/></param>
        public static async Task<bool> RandomSetu(BotConfig botConfig, GroupMessage groupMessage)
        {
            var groupId = groupMessage.GroupId;
            var targetId = groupMessage.Sender.UserId;
            var messageId = groupMessage.MessageId;
            var message = groupMessage.Message;
            var dateNow = DateTime.Now;

            if (message.StartsWith(COMMAND_CUSTOM_GROUP_SETUCD_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
            {
                if (message.Equals(COMMAND_CUSTOM_GROUP_SETUCD_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
                {
                    var doushiLv = botConfig.SetuSenderLv.ContainsKey(targetId)
                        ? botConfig.SetuSenderLv[targetId]
                        : 0;
                    // MEMO : 显Lv
                    var sendMessage = $"当前色图斗士Lv{doushiLv}, " +
                                      $"{BotExtensions.GetSetuSuccessPercent(doushiLv)}";
                    await Api.SendGroupMessage(groupId, sendMessage);
                }
                else if (targetId == PublicVar.AdminId)
                {
                    if (long.TryParse(message[COMMAND_CUSTOM_GROUP_SETUCD_LIBRARY.Length..], out var searchTargetId))
                    {
                        var doushiLv = botConfig.SetuSenderLv.ContainsKey(searchTargetId)
                            ? botConfig.SetuSenderLv[searchTargetId]
                            : 0;
                        // MEMO : 显CD
                        var sendMessage = $"目标色图斗士Lv{doushiLv} CD[{GetCD(searchTargetId)}], " +
                                          $"{BotExtensions.GetSetuSuccessPercent(doushiLv)}";
                        await Api.SendGroupMessage(groupId, sendMessage);
                    }
                    else
                    {
                        await Api.SendGroupMessage(groupId, "输入不正确");
                    }
                }

                return true;
            }

            if (targetId == PublicVar.AdminId && message.StartsWith(COMMAND_CUSTOM_GROUP_SETUQKCD_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
            {
                if (long.TryParse(message[COMMAND_CUSTOM_GROUP_SETUQKCD_LIBRARY.Length..], out var searchTargetId))
                {
                    // MEMO : 清空CD
                    botConfig.CanSetuSendCDs.Remove(searchTargetId);
                    var sendMessage = $"CD已清空!";
                    await Api.SendGroupMessage(groupId, sendMessage);
                }
                else
                {
                    await Api.SendGroupMessage(groupId, "输入不正确");
                }

                return true;
            }

            if (targetId == PublicVar.AdminId && message.StartsWith(COMMAND_CUSTOM_GROUP_SETUQKLV_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
            {
                if (long.TryParse(message[COMMAND_CUSTOM_GROUP_SETUQKLV_LIBRARY.Length..], out var searchTargetId))
                {
                    // MEMO : 清空Lv
                    botConfig.SetuSenderLv.Remove(searchTargetId);
                    var sendMessage = $"Lv已清空!";
                    await Api.SendGroupMessage(groupId, sendMessage);
                }
                else
                {
                    await Api.SendGroupMessage(groupId, "输入不正确");
                }

                return true;
            }

            if (targetId == PublicVar.AdminId && message.StartsWith(COMMAND_CUSTOM_GROUP_SETUQKALL_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
            {
                if (long.TryParse(message[COMMAND_CUSTOM_GROUP_SETUQKALL_LIBRARY.Length..], out var searchTargetId))
                {
                    // MEMO : 清空CD
                    botConfig.CanSetuSendCDs.Remove(searchTargetId);
                    botConfig.SetuSenderLv.Remove(searchTargetId);
                    var sendMessage = $"色图斗士状态已重置!";
                    await Api.SendGroupMessage(groupId, sendMessage);
                }
                else
                {
                    await Api.SendGroupMessage(groupId, "输入不正确");
                }

                return true;
            }

            if (message.Equals(COMMAND_CUSTOM_GROUP_SETURANK_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
            {
                if (Api.TryGetGroupMembers(groupId, out var groupMembers))
                {
                    var sendMessage = $"====色图斗士排行====";
                    var rankIndex = 1;
                    botConfig.SetuSenderLv
                        .OrderByDescending(each => each.Value)
                        .Take(5)
                        .ForEach(each =>
                        {
                            var userId = each.Key;
                            string doushiMessage;
                            if (groupMembers.TryGetValue(userId, out var groupMember))
                            {
                                doushiMessage = string.IsNullOrEmpty(groupMember.Card)
                                    ? $"{groupMember.NickName}({userId}) [Lv{each.Value}]"
                                    : $"{groupMember.Card}({userId}) [Lv{each.Value}]";
                            }
                            else
                            {
                                doushiMessage = $"无名色图斗士({userId}) [Lv{each.Value}]";
                            }

                            sendMessage += $"{ENTER}{rankIndex++}. {doushiMessage}";
                        });
                    await Api.SendGroupMessage(groupId, sendMessage);
                }

                return true;
            }

            lock (_syncKeyword)
            {
                if (_setuKeyWords == null)
                {
                    _setuKeyWords = new HashSet<string>();
                    var startText = new[]
                    {
                        "涩", "色", "瑟", "铯"
                    };
                    var endText = new[]
                    {
                        "图", "囤", "圖", "図", "屯"
                    };

                    startText.ForEach(eachStart => endText.ForEach(eachEnd => _setuKeyWords.Add(eachStart + eachEnd)));
                }
            }

            // MEMO : 命令为#st# (#st#支持关键字)
            var isSetuDebug = false;
            var tag = string.Empty;
            var setuKeywordCheck = false;
            if (message.StartsWith(COMMAND_CUSTOM_GROUP_SETU_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
            {
                setuKeywordCheck = true;
                tag = message[COMMAND_CUSTOM_GROUP_SETU_LIBRARY.Length..];
                goto StartSetu;
            }

            // MEMO : 字数在8字以内, 并包含色图关键字 (支持前置关键字)
            if (message.GetByteCount() <= 24 && message.EndsWithAny(_setuKeyWords))
            {
                setuKeywordCheck = true;
                tag = message[..^2];
                goto StartSetu;
            }

            if (message.Equals(COMMAND_CUSTOM_GROUP_SETUDEBUG_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
            {
                isSetuDebug = true;
                setuKeywordCheck = true;
                goto StartSetu;
            }

StartSetu:
            var sourceTag = tag;
            // MEMO : tag映射
            if (_tagDictionary.TryGetValue(tag.ToUpper(), out var changeTag))
                tag = changeTag;

            if (!botConfig.SetuSendLastRecords.TryGetValue(targetId, out var setuSendHistory))
                setuSendHistory = DateTime.MinValue;
            if (!botConfig.SetuSenderLv.TryGetValue(targetId, out var setuSenderLv))
                setuSenderLv = 0;

            if (setuKeywordCheck)
            {
                if (!isSetuDebug && (dateNow - setuSendHistory).TotalSeconds <= 20 + setuSenderLv * 10)
                {
                    // MEMO : 20秒内不连续响应
                    await Api.SendGroupMessage(groupId, $"{CQCode.Reply(targetId, messageId)}太快了, 休息一下吧");
                    return true;
                }

                botConfig.SetuSendLastRecords[targetId] = dateNow;

                var r18Bonus = false;
                var addSecond = 0;
                var addLevel = SetuAddLevel.Normal;
                var canSendSetu = false;

                var oldSetuSenderLv = setuSenderLv;
                var changeLvTime = 0;
                var changeLvTag = 0;
                var changeLvFast = 0;
                if ((!PublicVar.IsDebug || isSetuDebug) && targetId == PublicVar.AdminId)
                {
                    // MEMO : ADMIN无限制要色图
                    canSendSetu = true;
                }
                else
                {
                    // MEMO : 可发色图时间获得
                    if (!botConfig.CanSetuSendCDs.TryGetValue(targetId, out var nextCanSendDate))
                        nextCanSendDate = DateTime.MinValue;

                    // MEMO : 色图Lv减少
                    if (setuSenderLv > 0)
                    {
                        var totalMinutes = (dateNow - nextCanSendDate).TotalMinutes;
                        if (totalMinutes >= 90 && nextCanSendDate != DateTime.MinValue)
                        {
                            var changeLvTimePoint = (int)(totalMinutes / 90);
                            while (setuSenderLv > 0 && changeLvTimePoint >= setuSenderLv)
                            {
                                changeLvTimePoint -= setuSenderLv;
                                changeLvTime--;
                                setuSenderLv--;
                            }
                        }
                    }

                    List<RandomWeight<SendSetuConfig>> randActions;
                    if (dateNow > nextCanSendDate)
                    {
                        randActions = new List<RandomWeight<SendSetuConfig>>
                        {
                            new(3000, new SendSetuConfig(
                                SendBaseDelay + (int) (60 * Math.Pow(setuSenderLv, 2)) + Rand.Next(-60, 60),
                                SetuAddLevel.Normal, true)),
                            new(200 + (int) (150 * Math.Pow(setuSenderLv, 2)),
                                new SendSetuConfig(Rand.Next(3, 15), SetuAddLevel.Normal)),
                            new(200 + (int) (150 * Math.Pow(setuSenderLv, 2)),
                                new SendSetuConfig(Rand.Next(5, 30), SetuAddLevel.Normal)),
                            new(100 + (int) (75 * Math.Pow(setuSenderLv, 2)),
                                new SendSetuConfig(Rand.Next(8, 45), SetuAddLevel.Double)),
                            new(50 + (int) (40 * Math.Pow(setuSenderLv, 2)),
                                new SendSetuConfig(Rand.Next(10, 60), SetuAddLevel.SuperDouble)),
                            new(150 - (int)(setuSenderLv * 135.0 / MaxSenderLv), new SendSetuConfig(0, SetuAddLevel.Free, true)),
                            new(30 - (int)(setuSenderLv * 27.0 / MaxSenderLv), new SendSetuConfig(
                                SendBaseDelay + (int) (60 * Math.Pow(setuSenderLv, 2)) + Rand.Next(-60, 60),
                                SetuAddLevel.Normal, true, true)),
                            new(10 - (int)(setuSenderLv * 9.0 / MaxSenderLv), new SendSetuConfig(0, SetuAddLevel.Free, true, true)),
                        };
                    }
                    else
                    {
                        if ((nextCanSendDate - dateNow).TotalSeconds >= 300 - (int)(setuSenderLv * 150.0 / 15))
                        {
                            // MEMO : CD5分钟以上, 老实等着吧
                            await Api.SendGroupMessage(groupId, $"{CQCode.Reply(targetId, messageId)}CD还早呢, 先歇着吧");
                            return true;
                        }

                        randActions = new List<RandomWeight<SendSetuConfig>>
                        {
                            new(10000, new SendSetuConfig(Rand.Next(10, 60), SetuAddLevel.Normal)),
                            new(3500, new SendSetuConfig(Rand.Next(10, 60) * 2, SetuAddLevel.Double)),
                            new(1500, new SendSetuConfig(Rand.Next(10, 60) * 4, SetuAddLevel.SuperDouble)),
                            new(750, new SendSetuConfig(Rand.Next(10, 60) * 8, SetuAddLevel.Golden)),
                            new(300, new SendSetuConfig(Rand.Next(10, 60) * 16, SetuAddLevel.Platinum)),
                            new(150, new SendSetuConfig(Rand.Next(10, 60) * 32, SetuAddLevel.Diamond)),
                            new(10, new SendSetuConfig(Rand.Next(10, 60) * 256, SetuAddLevel.Death)),
                            new(1500, new SendSetuConfig(Rand.Next(5, 60) * -1, SetuAddLevel.Luck)),
                            new(600, new SendSetuConfig(Rand.Next(5, 60) * -4, SetuAddLevel.LuckSuper)),
                            new(200, new SendSetuConfig(Rand.Next(5, 60) * -16, SetuAddLevel.LuckGolden)),
                            //new(200, new SendSetuConfig(0, SetuAddLevel.Free)),
                        };
                    }

                    if (randActions.TryGetRandomWeight(out var resultAction))
                    {
                        SetSetuValues(resultAction.Value);
                    }
                    else
                    {
                        // MEMO : 应该不会发生
                    }

                    // MEMO : 参数注入意图
                    if (tag!.Contains("&") || tag!.Contains("%26"))
                        SetSetuValues(new SendSetuConfig(3600, SetuAddLevel.Death));

                    // MEMO : 色图Lv增加
                    if (canSendSetu)
                    {
                        if (!message.StartsWith(COMMAND_CUSTOM_GROUP_SETU_LIBRARY, StringComparison.CurrentCultureIgnoreCase)
                            && !string.IsNullOrEmpty(tag))
                        {
                            changeLvTag = 1;
                            setuSenderLv++;
                        }

                        if ((dateNow - nextCanSendDate).TotalSeconds <= 300)
                        {
                            changeLvFast = 1;
                            setuSenderLv++;
                        }
                    }

                    if (setuSenderLv < 0)
                        setuSenderLv = 0;

                    if (setuSenderLv > MaxSenderLv)
                        setuSenderLv = MaxSenderLv;

                    botConfig.SetuSenderLv[targetId] = setuSenderLv;
                }

                if (PublicVar.IsDebug)
                {
                    await Api.SendGroupMessage(groupId, "[DEBUG]" +
                                                        $"{ENTER}目标对象: {targetId}" +
                                                        $"{ENTER}色图Lv: {setuSenderLv}" +
                                                        $"{ENTER}是否发送: {canSendSetu}" +
                                                        $"{ENTER}增加时间: {addSecond}s" +
                                                        $"{ENTER}色图CD: {GetCD(targetId)}");
                }

                var addSetuSenderLv = setuSenderLv - oldSetuSenderLv;
                if (!canSendSetu)
                {
                    AddCD();
                    var isShowDate = Rand.Next(0, 100) <= 3;
                    var sendMessage = string.Empty;
                    if (addSecond > 0)
                    {
                        // MEMO : CD增加
                        sendMessage = $"{CQCode.At(targetId)}" +
                                      $"{_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}" +
                                      $"的CD{_setuCDWasAdded.Random().Replace("$ADD_LEVEL$", addLevel.ToAddLevelString())}" +
                                      GetSetuLvInfo() +
                                      (isShowDate ? $" [CD {GetCD(targetId)}]" : string.Empty);
                    }
                    //else if (addSecond == 0)
                    //{
                    //    // MEMO : 白嫖
                    //    sendMessage = $"{CQCode.at(targetId)}"
                    //                  + $"什么!? 你成功白嫖了一张{_setuKeyWords.Random()}!"
                    //                  + (isShowDate ? $" [CD {botConfig.CanSetuSendCDs[groupId]:HH:mm:ss}]" : string.Empty);
                    //    await Api.SendGroupMessage(groupId, sendMessage);
                    //    goto SendSetu;
                    //}
                    else
                    {
                        // MEMO : 幸运(CD减少)
                        sendMessage = $"{CQCode.At(targetId)}"
                                      + $"运气好, {_setuCDWasReduced.Random().Replace("$ADD_LEVEL$", addLevel.ToAddLevelString())}"
                                      + $" ({addSecond}s)"
                                      + GetSetuLvInfo()
                                      + (isShowDate ? $" [CD {GetCD(targetId)}]" : string.Empty);
                    }

                    await Api.SendGroupMessage(groupId, sendMessage);
                    return true;
                }
                else
                {
                    AddCD();
                    var isShowDate = Rand.Next(0, 100) <= 3;
                    if (targetId != PublicVar.AdminId && addSecond == 0)
                    {
                        // MEMO : 白嫖
                        var sendMessage = $"{CQCode.At(targetId)}"
                                          + $"什么!? 你成功白嫖了一张{sourceTag}{_setuKeyWords.Random()}!"
                                          + (isShowDate ? $" [CD {GetCD(targetId)}]" : string.Empty);
                        await Api.SendGroupMessage(groupId, sendMessage);
                        goto SendSetu;
                    }
                }

                var revertCD = DateTime.MinValue;
SendSetu:
                try
                {
                    var randomSetuKeyword = new List<RandomWeight<Func<string, Task<SetuInfo>>>>
                    {
                        new(10, SetuExtensions.GetSetu_Lolicon),
                        new(8, SetuExtensions.GetSetu_Yuban),
                        new(3, SetuExtensions.GetSetu_Jitsu),
                    };
                    var randomSetu = new List<RandomWeight<Func<string, Task<SetuInfo>>>>
                    {
                        new(10, SetuExtensions.GetSetu_Lolicon),
                        new(8, SetuExtensions.GetSetu_Yuban),
                        new(6, SetuExtensions.GetSetu_NyanCatda),
                        new(3, SetuExtensions.GetSetu_Jitsu),
                    };

                    //Func<string, Task<SetuInfo>>[] randomSetuKeyword = {
                    //    SetuExtensions.GetSetu_Lolicon,
                    //    SetuExtensions.GetSetu_Yuban,
                    //    SetuExtensions.GetSetu_Jitsu,
                    //};
                    Func<string, Task<SetuInfo>>[] randomSetuDefault =
                    {
                        SetuExtensions.GetSetu_Lolicon,
                        SetuExtensions.GetSetu_Yuban,
                        SetuExtensions.GetSetu_NyanCatda,
                        SetuExtensions.GetSetu_Jitsu,
                    };

                    await Api.SendGroupMessage(groupId,
                        $"{CQCode.Reply(targetId, messageId)}{_setuKeyWords.Random()}正在{_setuGetting.Random()}...");

                    var (setuInfo, fileName) = await GetSetu(() => !string.IsNullOrEmpty(tag)
                            ? randomSetuKeyword.TryGetRandomWeight(out var funcResult)
                                ? funcResult.Value.Invoke(tag)
                                : randomSetuDefault.Random().Invoke(tag)
                            : randomSetu.TryGetRandomWeight(out var funcResult2)
                                ? funcResult2.Value.Invoke(tag)
                                : randomSetuDefault.Random().Invoke(tag),
                        false);
                    if (setuInfo == null)
                        return false;

                    switch (setuInfo.Result)
                    {
                        case SetuResult.Successed:
                            break;
                        case SetuResult.ApiError:
                            await Api.SendGroupMessage(groupId,
                                $"{CQCode.At(targetId)}{_setuKexiStart.Random()}" +
                                $"Api炸了[{setuInfo.SetuType}],色图取得失败!{_setuKexiEnd.Random()} {GetSetuLvInfo()}");
                            botConfig.CanSetuSendCDs[targetId] = revertCD;
                            return true;
                        case SetuResult.NoSearchResult:
                            await Api.SendGroupMessage(groupId,
                                $"{CQCode.At(targetId)}{_setuKexiStart.Random()}" +
                                $"色图库中没找到色图~,{_setuKexiEnd.Random()} {GetSetuLvInfo()}");
                            botConfig.CanSetuSendCDs[targetId] = revertCD;
                            return true;
                        case SetuResult.OtherError:
                            await Api.SendGroupMessage(groupId,
                                $"{CQCode.At(targetId)}{_setuKexiStart.Random()}" +
                                $"Api未知错误[{setuInfo.SetuType}],色图取得失败!{_setuKexiEnd.Random()} {GetSetuLvInfo()}");
                            botConfig.CanSetuSendCDs[targetId] = revertCD;
                            return true;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    var sendMessages = new List<GroupForwardMessage>
                    {
                        new(messageId),
                        new(BOT_NAME, BotId, $"{GetSetuLvInfo()}"),
                        new($"{setuInfo.SetuType}", BotId, CQCode.Image(CommonExtensions.GetPath(CACHE_DIRECTORY_NAME, fileName))),
                        new($"{setuInfo.SetuType}", BotId, $"{setuInfo.SourceText}" +
                                                           $"{ENTER}{_setuSource.Random()}:{setuInfo.SourceUrl}"),
                    };
                    //await Api.SendGroupMessage(groupId,
                    //    $"{setuInfo.SourceText}" +
                    //    $"{ENTER}{_setuSource.Random()}:{setuInfo.SourceUrl}" +
                    //    $"{ENTER}API提供:{setuInfo.SetuType} {GetSetuLvInfo()}");

                    //await Api.SendGroupMessage(groupId,
                    //    CQCode.Image(CommonExtensions.GetPath(CACHE_DIRECTORY_NAME, fileName)) +
                    //    $"{ENTER}{CQCode.At(targetId)}{_setuYouwant.Random()}{sourceTag}{_setuKeyWords.Random()}{_setuGetted.Random()}");

                    if (r18Bonus)
                    {
                        var randomSetuR18Keyword = new List<RandomWeight<Func<string, Task<SetuInfo>>>>
                        {
                            new(10, SetuExtensions.GetSetu_Lolicon_R18),
                            new(8, SetuExtensions.GetSetu_Yuban_R18),
                            new(3, SetuExtensions.GetSetu_Jitsu_R18),
                        };
                        var randomSetuR18 = new List<RandomWeight<Func<string, Task<SetuInfo>>>>
                        {
                            new(10, SetuExtensions.GetSetu_Lolicon_R18),
                            new(8, SetuExtensions.GetSetu_Yuban_R18),
                            new(6, SetuExtensions.GetSetu_NyanCatda_R18),
                            new(3, SetuExtensions.GetSetu_Jitsu_R18),
                        };

                        var (setuInfoR18, _) = await GetSetu(() => !string.IsNullOrEmpty(tag)
                                ? randomSetuR18Keyword.TryGetRandomWeight(out var funcResult)
                                    ? funcResult.Value.Invoke(tag)
                                    : randomSetuDefault.Random().Invoke(tag)
                                : randomSetuR18.TryGetRandomWeight(out var funcResult2)
                                    ? funcResult2.Value.Invoke(tag)
                                    : randomSetuDefault.Random().Invoke(tag),
                            true);
                        if (setuInfoR18 == null)
                            return false;

                        switch (setuInfoR18.Result)
                        {
                            case SetuResult.Successed:
                                sendMessages.Add(new GroupForwardMessage($"{setuInfoR18.SetuType}", BotId,
                                    $"[这是一张额外的金色传说{sourceTag}色图, 不可预览]"));
                                sendMessages.Add(new GroupForwardMessage($"{setuInfoR18.SetuType}", BotId,
                                    $"{setuInfoR18.SourceText}" +
                                    $"{ENTER}{_setuSource.Random()}:{setuInfoR18.SourceUrl}"));
                                break;
                            case SetuResult.ApiError:
                                sendMessages.Add(new GroupForwardMessage($"{setuInfo.SetuType}", BotId,
                                    $"{_setuKexiStart.Random()}" +
                                    $"Api炸了, 金色传说色图取得失败!{_setuKexiEnd.Random()} {GetSetuLvInfo()}"));
                                //await Api.SendGroupMessage(groupId,
                                //    $"{CQCode.At(targetId)}{_setuKexiStart.Random()}" +
                                //    $"Api炸了[{setuInfo.SetuType}],金色传说色图取得失败!{_setuKexiEnd.Random()} {GetSetuLvInfo()}");
                                botConfig.CanSetuSendCDs[targetId] = revertCD;
                                break;
                            case SetuResult.NoSearchResult:
                                sendMessages.Add(new GroupForwardMessage($"{setuInfo.SetuType}", BotId,
                                    $"{_setuKexiStart.Random()}" +
                                    $"色图库中没找到金色传说色图~,{_setuKexiEnd.Random()} {GetSetuLvInfo()}"));
                                //await Api.SendGroupMessage(groupId,
                                //    $"{CQCode.At(targetId)}{_setuKexiStart.Random()}" +
                                //    $"色图库中没找到金色传说色图~,{_setuKexiEnd.Random()} {GetSetuLvInfo()}");
                                botConfig.CanSetuSendCDs[targetId] = revertCD;
                                break;
                            case SetuResult.OtherError:
                                sendMessages.Add(new GroupForwardMessage($"{setuInfo.SetuType}", BotId,
                                    $"{_setuKexiStart.Random()}" +
                                    $"Api未知错误, 金色传说色图取得失败!{_setuKexiEnd.Random()} {GetSetuLvInfo()}"));
                                //await Api.SendGroupMessage(groupId,
                                //    $"{CQCode.At(targetId)}{_setuKexiStart.Random()}" +
                                //    $"Api未知错误[{setuInfo.SetuType}],金色传说色图取得失败!{_setuKexiEnd.Random()} {GetSetuLvInfo()}");
                                botConfig.CanSetuSendCDs[targetId] = revertCD;
                                break;
                        }

                        //await Api.SendGroupMessage(groupId,
                        //    $"[这是一张额外的金色传说{sourceTag}色图, 不可预览]" +
                        //    $"{ENTER}{setuInfoR18.SourceText}" +
                        //    $"{ENTER}{_setuSource.Random()}:{setuInfoR18.SourceUrl}" +
                        //    $"{ENTER}API提供:{setuInfoR18.SetuType}");
                    }

                    //sendMessages.Add(new GroupForwardMessage(BOT_NAME, BotId,
                    //    $"{_setuYouwant.Random()}{sourceTag}{_setuKeyWords.Random()}{_setuGetted.Random()}"));
                    await Api.SendGroupForwardMessage(groupId, sendMessages);
                }
                catch (Exception)
                {
                    return false;
                }

                void SetSetuValues(SendSetuConfig sendSetuConfig)
                {
                    addLevel = sendSetuConfig.SetuAddLevel;
                    addSecond = sendSetuConfig.AddSecond > 0
                        ? (int)(sendSetuConfig.AddSecond * (setuSenderLv + 3.0) / 3)
                        : sendSetuConfig.AddSecond;
                    canSendSetu = sendSetuConfig.CanSend;
                    r18Bonus = sendSetuConfig.R18;
                }

                void AddCD()
                {
                    revertCD = dateNow.AddSeconds(Rand.Next(3, 15));
                    botConfig.CanSetuSendCDs[targetId] = botConfig.CanSetuSendCDs.ContainsKey(targetId)
                        ? (botConfig.CanSetuSendCDs[targetId] > dateNow
                            ? botConfig.CanSetuSendCDs[targetId]
                            : dateNow).AddSeconds(addSecond)
                        : dateNow.AddSeconds(addSecond);
                }

                string GetSetuLvInfo()
                {
                    var addString = string.Empty;
                    if (changeLvTime < 0)
                        addString += $",时间冷却{changeLvTime.ToSignString()}";
                    if (changeLvTag > 0)
                        addString += $",关键词搜索{changeLvTag.ToSignString()}";
                    if (changeLvFast > 0)
                        addString += $",频率过快{changeLvFast.ToSignString()}";
                    if (!string.IsNullOrEmpty(addString))
                        addString = addString[1..];

                    var addLvString = (string.IsNullOrEmpty(addString)
                        ? string.Empty
                        : $"本次{addSetuSenderLv.ToSignString()}({addString})");
                    return $"[斗士Lv{oldSetuSenderLv}] {addLvString}";
                }

                async Task<(SetuInfo, string)> GetSetu(Func<Task<SetuInfo>> getSetuInfoFunc, bool checkImageOnly)
                {
                    var setuInfo = await getSetuInfoFunc.Invoke();
                    DebugSendSetuInfo();

                    var fileName = string.Empty;
                    var getSuccessed = false;
                    const int maxRetryTimes = 4;
                    var retryTimes = 0;
                    while (!getSuccessed && retryTimes <= maxRetryTimes)
                    {
                        if (setuInfo.Result == SetuResult.Successed)
                        {
                            (getSuccessed, fileName) = await HttpExtensions.HttpDownloadAsync(
                                setuInfo.ImageUrl, CACHE_DIRECTORY_NAME, true, checkImageOnly);
                            if (getSuccessed)
                                continue;
                        }
                        else if (setuInfo.Result == SetuResult.NoSearchResult)
                        {
                            getSuccessed = true;
                            continue;
                        }

                        //await Api.SendGroupMessage(groupId,
                        //    $"啊, 该{_setuKeyWords.Random()}被作者删了!{ENTER}正在第{retryTimes}次重新{_setuGetting.Random()}...");
                        retryTimes++;
                        setuInfo = await getSetuInfoFunc.Invoke();
                        DebugSendSetuInfo();
                        CommonUtil.Sleep(500);
                    }

                    if (retryTimes > maxRetryTimes)
                    {
                        await Api.SendGroupMessage(groupId, "超过重试次数上限,放弃下载!");
                        return (setuInfo, string.Empty);
                    }

                    if (!checkImageOnly && setuInfo.Result == SetuResult.Successed)
                    {
                        var isFileExists = false;
                        while (!isFileExists)
                        {
                            isFileExists = File.Exists($"{CACHE_DIRECTORY_NAME}/{fileName}");
                            CommonUtil.Sleep(100);
                        }

                        CommonExtensions.DeleteExpiredCache();
                    }

                    return (setuInfo, fileName);

                    async void DebugSendSetuInfo()
                    {
                        if (!PublicVar.IsDebug)
                            return;

                        if (setuInfo.Result == SetuResult.Successed)
                        {
                            await Api.SendGroupMessage(groupId, "[DEBUG]SetuInfo" +
                                                                $"{ENTER}SetuType: {setuInfo.SetuType}" +
                                                                $"{ENTER}SetuResult: {setuInfo.Result}" +
                                                                $"{ENTER}SmallUrl: {setuInfo.ImageUrl}" +
                                                                $"{ENTER}SourceUrl: {setuInfo.SourceUrl}");
                        }
                        else
                        {
                            await Api.SendGroupMessage(groupId, "[DEBUG]SetuInfo" +
                                                                $"{ENTER}SetuType: {setuInfo.SetuType}" +
                                                                $"{ENTER}SetuResult: {setuInfo.Result}");
                        }
                    }
                }
            }
            else
            {
                if (message.ContainsAny(_setuBuman) && (dateNow - setuSendHistory).TotalSeconds <= 30)
                {
                    var addSecond = Rand.Next(60, 600);
                    var randActions = new List<RandomWeight<int>>
                    {
                        new(1000, 0),
                        new(300, 1),
                        new(100, 2),
                        new(30, 3)
                    };
                    var addSetuSenderLv = 0;
                    if (randActions.TryGetRandomWeight(out var item))
                        addSetuSenderLv = item.Value;

                    var addString = string.Empty;
                    if (addSetuSenderLv > 0)
                        addString += $"神秘{addSetuSenderLv.ToSignString()}";

                    var addLvString = (string.IsNullOrEmpty(addString)
                        ? string.Empty
                        : $"本次{addSetuSenderLv.ToSignString()}({addString})");
                    botConfig.SetuSenderLv[targetId] = setuSenderLv + addSetuSenderLv;
                    botConfig.CanSetuSendCDs[targetId] = botConfig.CanSetuSendCDs.ContainsKey(targetId)
                        ? (botConfig.CanSetuSendCDs[targetId] > dateNow
                            ? botConfig.CanSetuSendCDs[targetId]
                            : dateNow).AddSeconds(addSecond)
                        : dateNow.AddSeconds(addSecond);
                    var sendMessage = $"{CQCode.At(targetId)}" +
                                      $"{_setuKeyWords.Random()}的CD神秘地{_setuCDWasAdded.Random().Replace("$ADD_LEVEL$", SetuAddLevel.SuperDouble.ToAddLevelString())}" +
                                      $"[斗士Lv{setuSenderLv}] {addLvString}";
                    await Api.SendGroupMessage(groupId, sendMessage);
                    return true;
                }

                return true;
            }

            return true;

            string GetCD(long searchId)
            {
                if (!botConfig.CanSetuSendCDs.ContainsKey(searchId))
                    return "无记录";

                if (dateNow >= botConfig.CanSetuSendCDs[searchId])
                {
                    var doushiLv = botConfig.SetuSenderLv.ContainsKey(targetId)
                        ? botConfig.SetuSenderLv[targetId]
                        : 0;
                    if (!botConfig.CanSetuSendCDs.TryGetValue(targetId, out var nextCanSendDate))
                        nextCanSendDate = DateTime.MinValue;

                    var totalMinutes = (dateNow - nextCanSendDate).TotalMinutes;
                    var tempChangeLvTime = 0;
                    if (totalMinutes >= 90 && nextCanSendDate != DateTime.MinValue)
                    {
                        var changeLvTimePoint = (int)(totalMinutes / 90);
                        while (doushiLv > 0 && changeLvTimePoint >= doushiLv)
                        {
                            changeLvTimePoint -= doushiLv;
                            tempChangeLvTime--;
                            doushiLv--;
                        }
                    }

                    return tempChangeLvTime != 0
                        ? $"可Lv{tempChangeLvTime}"
                        : "可使用";
                }

                return botConfig.CanSetuSendCDs[searchId].ToString("HH:mm:ss");
            }
        }
    }

    /// <summary>
    /// 色图发送配置(加时间等)
    /// </summary>
    public class SendSetuConfig
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="addSecond"></param>
        /// <param name="setuAddLevel"></param>
        /// <param name="canSend"></param>
        /// <param name="r18"></param>
        public SendSetuConfig(int addSecond, SetuAddLevel setuAddLevel, bool canSend = false, bool r18 = false)
        {
            AddSecond = addSecond;
            SetuAddLevel = setuAddLevel;
            CanSend = canSend;
            R18 = r18;
        }

        public int AddSecond { get; set; }
        public SetuAddLevel SetuAddLevel { get; set; }
        public bool CanSend { get; set; }
        public bool R18 { get; set; }
    }

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
                _ => throw new ArgumentOutOfRangeException(nameof(setuAddLevel), setuAddLevel, null)
            };
    }

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
    }
}