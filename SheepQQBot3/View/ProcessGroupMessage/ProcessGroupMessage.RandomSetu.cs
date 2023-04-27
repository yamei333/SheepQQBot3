using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary;
using Masuit.Tools.Systems;
using SheepQQBot3.DbModel;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.Setu;
using SheepQQBot3.SDK.Client;
using Yamei.Common;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View;

public static partial class ProcessGroupMessage
{
    private static readonly object _syncKeyword = new();

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

    private static readonly Dictionary<string, string> _tagDictionary = new()
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
    public static async Task<bool> RandomSetuAsync(BotConfig botConfig, GroupMessage groupMessage)
    {
        var groupId = groupMessage.GroupId;
        var targetId = groupMessage.Sender.UserId;
        var messageId = groupMessage.MessageId;
        var message = groupMessage.Message;
        var dateNow = DateTime.Now;

        var setuDoushiInfo = await BotDb.SetuDoushiInfos.FindAsync(targetId).ConfigureAwait(false);
        if (setuDoushiInfo == null)
        {
            setuDoushiInfo = new SetuDoushiInfo(targetId);
            await BotDb.AddAsync(setuDoushiInfo).ConfigureAwait(false);
        }

        var setuDoushiLv = setuDoushiInfo.SetuDoushiLv;
        var setuCd = setuDoushiInfo.SetuCD.ToDateTime();
        if (message.StartsWith(COMMAND_CUSTOM_GROUP_SETUCD_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
        {
            if (message.Equals(COMMAND_CUSTOM_GROUP_SETUCD_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
            {
                // MEMO : 显Lv
                var sendMessage = $"当前色图斗士Lv{setuDoushiInfo.CalcSetuDoushiLv(dateNow)}, " +
                                  $"{BotExtensions.GetSetuSuccessPercent(setuDoushiInfo, dateNow)}";
                await Api.SendGroupMessageAsync(groupId, sendMessage).ConfigureAwait(false);
            }
            else if (targetId == PublicVar.AdminId)
            {
                if (long.TryParse(message[COMMAND_CUSTOM_GROUP_SETUCD_LIBRARY.Length..], out var searchTargetId))
                {
                    if (searchTargetId < 100)
                    {
                        var vtuberSetuDoushiInfo = new SetuDoushiInfo
                        {
                            TargetId = 0,
                            SetuCD = dateNow.ToTimeStamp(),
                            SetuDoushiLv = searchTargetId
                        };
                        var sendMessage = $"虚拟色图斗士Lv{searchTargetId}, " +
                                          $"{BotExtensions.GetSetuSuccessPercent(vtuberSetuDoushiInfo, dateNow)}";
                        await Api.SendGroupMessageAsync(groupId, sendMessage).ConfigureAwait(false);
                    }
                    else
                    {
                        var searchSetuDoushiInfo = await BotDb.SetuDoushiInfos
                            .FindAsync(searchTargetId).ConfigureAwait(false)
                            ?? new SetuDoushiInfo(searchTargetId);
                        // MEMO : 显CD
                        var sendMessage = $"目标色图斗士Lv{searchSetuDoushiInfo.CalcSetuDoushiLv(dateNow)} " +
                                          $"CD[{GetCD(searchSetuDoushiInfo)}], " +
                                          $"{BotExtensions.GetSetuSuccessPercent(searchSetuDoushiInfo, dateNow)}";
                        await Api.SendGroupMessageAsync(groupId, sendMessage).ConfigureAwait(false);
                    }
                }
                else
                {
                    await Api.SendGroupMessageAsync(groupId, "输入不正确").ConfigureAwait(false);
                }
            }

            return true;
        }

        if (targetId == PublicVar.AdminId)
        {
            if (message.StartsWith(COMMAND_CUSTOM_GROUP_SETUQKCD_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
            {
                if (long.TryParse(message[COMMAND_CUSTOM_GROUP_SETUQKCD_LIBRARY.Length..], out var searchTargetId))
                {
                    // MEMO : 清空CD
                    var targetDoushiInfo = await BotDb.SetuDoushiInfos.FindAsync(searchTargetId).ConfigureAwait(false);
                    if (targetDoushiInfo != null)
                    {
                        targetDoushiInfo.SetuCD = 0;
                        BotDb.Update(targetDoushiInfo);
                    }
                    else
                    {
                        await BotDb.AddAsync(new SetuDoushiInfo(searchTargetId)).ConfigureAwait(false);
                    }

                    await Api.SendGroupMessageAsync(groupId, "CD已清空!").ConfigureAwait(false);
                }
                else
                {
                    await Api.SendGroupMessageAsync(groupId, "输入不正确").ConfigureAwait(false);
                }

                return true;
            }

            if (message.StartsWith(COMMAND_CUSTOM_GROUP_SETUQKLV_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
            {
                if (long.TryParse(message[COMMAND_CUSTOM_GROUP_SETUQKLV_LIBRARY.Length..], out var searchTargetId))
                {
                    // MEMO : 清空Lv
                    var targetDoushiInfo = await BotDb.SetuDoushiInfos.FindAsync(searchTargetId).ConfigureAwait(false);
                    if (targetDoushiInfo != null)
                    {
                        targetDoushiInfo.SetuDoushiLv = 0;
                        BotDb.Update(targetDoushiInfo);
                    }
                    else
                    {
                        await BotDb.AddAsync(new SetuDoushiInfo(searchTargetId)).ConfigureAwait(false);
                    }

                    await Api.SendGroupMessageAsync(groupId, "Lv已清空!").ConfigureAwait(false);
                }
                else
                {
                    await Api.SendGroupMessageAsync(groupId, "输入不正确").ConfigureAwait(false);
                }

                return true;
            }

            if (message.StartsWith(COMMAND_CUSTOM_GROUP_SETUQKALL_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
            {
                if (long.TryParse(message[COMMAND_CUSTOM_GROUP_SETUQKALL_LIBRARY.Length..], out var searchTargetId))
                {
                    // MEMO : 清空CD
                    var targetDoushiInfo = await BotDb.SetuDoushiInfos.FindAsync(searchTargetId).ConfigureAwait(false);
                    if (targetDoushiInfo != null)
                    {
                        targetDoushiInfo.SetuDoushiLv = 0;
                        targetDoushiInfo.SetuCD = 0;
                        BotDb.Update(targetDoushiInfo);
                    }
                    else
                    {
                        await BotDb.AddAsync(new SetuDoushiInfo(searchTargetId)).ConfigureAwait(false);
                    }

                    await Api.SendGroupMessageAsync(groupId, "色图斗士状态已重置!").ConfigureAwait(false);
                }
                else
                {
                    await Api.SendGroupMessageAsync(groupId, "输入不正确").ConfigureAwait(false);
                }

                return true;
            }
        }

        if (message.Equals(COMMAND_CUSTOM_GROUP_SETURANK_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
        {
            var groupMembers = await Api.GetGroupMembersAsync(groupId).ConfigureAwait(false);
            if (groupMembers == null)
                return false;

            var sendMessage = "=====色图斗士Lv排行=====";
            var rankIndex = 1;
            BotDb.SetuDoushiInfos
                .AsEnumerable()
                .Select(info => new
                {
                    info.TargetId,
                    SetuDoushiLv = info.CalcSetuDoushiLv(dateNow)
                })
                .OrderByDescending(info => info.SetuDoushiLv)
                .Take(5)
                .ForEach(info =>
                {
                    sendMessage += $"\r\n{rankIndex++}. " +
                                   $"{GetSetuSenderName(info.TargetId)} [Lv{info.SetuDoushiLv}]";
                });
            await Api.SendGroupMessageAsync(groupId, sendMessage).ConfigureAwait(false);
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

        var isSetuDebug = false;
        var tag = string.Empty;
        var setuKeywordCheck = false;
        // MEMO : #st# (#st#支持关键字)
        if (message.StartsWith(COMMAND_CUSTOM_GROUP_SETU_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
        {
            setuKeywordCheck = true;
            tag = message[COMMAND_CUSTOM_GROUP_SETU_LIBRARY.Length..];
            goto StartSetu;
        }

        // MEMO : 字数在8字以内, 并包含色图关键字 (支持前置关键字)
        if (message.GetByteCount() <= 45 && message.EndsWithAny(_setuKeyWords))
        {
            setuKeywordCheck = true;
            tag = message[..^2];
            goto StartSetu;
        }

        // MEMO : #stdebug#
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

        var isSearchTag = !string.IsNullOrEmpty(tag);
        var targetSetuSendHistorys = BotDb.SetuSendHistorys
            .Where(each => each.TargetId == targetId)
            .ToList();
        var lastHistory = Enumerable.MaxBy(targetSetuSendHistorys, each => each.TimeStamp);
        var lastKeyword = lastHistory?.SearchKeyword ?? string.Empty;
        if (!string.IsNullOrEmpty(lastKeyword))
        {
            // MEMO : 最后2次色图都有关键字
            var last2Historys = targetSetuSendHistorys
                .OrderByDescending(history => history.TimeStamp)
                .Where(history => !history.IsR18Bonus.ToBool()
                    && history.IsRequestSuccessed.ToBool())
                .Take(2)
                .ToList();
            if (last2Historys.Count == 2 && last2Historys
                .All(history => history.SearchKeyword == lastKeyword
                    && !history.IsGetSuccessed.ToBool()))
            {
                setuDoushiInfo.SetuCD = dateNow.AddHours(120).ToTimeStamp();
                BotDb.Update(setuDoushiInfo);
                return true;
            }
        }

        var setuSendHistory = lastHistory?.TimeStamp.ToDateTime() ?? DateTime.MinValue;
        if (setuKeywordCheck)
        {
            if (!isSetuDebug && (dateNow - setuSendHistory).TotalSeconds <= 20 + setuDoushiLv * 10)
            {
                // MEMO : 20秒内不连续响应
                //await Api.SendGroupMessageAsync(groupId, $"{CQCode.Reply(targetId, messageId)}太快了, 休息一下吧").ConfigureAwait(false);
                return true;
            }

            //botConfig.SetuSendLastRecords[targetId] = dateNow;

            var r18Bonus = false;
            var addSecond = 0;
            var addLevel = SetuAddLevel.Normal;
            var canSendSetu = false;

            var oldSetuSenderLv = setuDoushiLv;
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
                    randActions = new List<RandomWeight<SendSetuConfig>>
                    {
                        new(3000 + (oldSetuSenderLv == 0 ? (int)(dateNow - setuCd).TotalMinutes : 0), new SendSetuConfig(
                            SendBaseDelay + (int) (60 * Math.Pow(setuDoushiLv, 2)) + Rand.Next(-60, 60),
                            SetuAddLevel.Normal, true)),
                        new(200 + (int)(150 * (setuDoushiLv < 10 ? Math.Pow(setuDoushiLv, 2) : Math.Pow(setuDoushiLv, 2.5))),
                            new SendSetuConfig(Rand.Next(1 + (int)(1 * Math.Pow(setuDoushiLv, 2)), 15 + (int)(5 * Math.Pow(setuDoushiLv, 2))), SetuAddLevel.Normal)),
                        new(200 + (int)(150 * (setuDoushiLv < 10 ? Math.Pow(setuDoushiLv, 2) : Math.Pow(setuDoushiLv, 2.5))),
                            new SendSetuConfig(Rand.Next(3 + (int)(2 * Math.Pow(setuDoushiLv, 2)), 30 + (int)(10 * Math.Pow(setuDoushiLv, 2))), SetuAddLevel.Normal)),
                        new(100 + (int)(75 * (setuDoushiLv < 10 ? Math.Pow(setuDoushiLv, 2) : Math.Pow(setuDoushiLv, 2.5))),
                            new SendSetuConfig(Rand.Next(5 + (int)(3 * Math.Pow(setuDoushiLv, 2)), 45 + (int)(15 * Math.Pow(setuDoushiLv, 2))), SetuAddLevel.Double)),
                        new(50 + (int)(40 * (setuDoushiLv < 10 ? Math.Pow(setuDoushiLv, 2) : Math.Pow(setuDoushiLv, 2.5))),
                            new SendSetuConfig(Rand.Next(7 + (int)(4 * Math.Pow(setuDoushiLv, 2)), 60 + (int)(20 * Math.Pow(setuDoushiLv, 2))), SetuAddLevel.SuperDouble)),
                        new(150 - (int)(setuDoushiLv * 135.0 / MaxSenderLv), new SendSetuConfig(0, SetuAddLevel.Free, true)),
                        new(30 - (int)(setuDoushiLv * 27.0 / MaxSenderLv), new SendSetuConfig(
                            SendBaseDelay + (int)(60 * Math.Pow(setuDoushiLv, 2)) + Rand.Next(-60, 60),
                            SetuAddLevel.Normal, true, true)),
                        new(10 - (int)(setuDoushiLv * 9.0 / MaxSenderLv), new SendSetuConfig(0, SetuAddLevel.Free, true, true)),
                    };
                }
                else
                {
                    if ((setuCd - dateNow).TotalSeconds >= 300 - (int)(setuDoushiLv * 150.0 / 15))
                    {
                        // MEMO : CD5分钟以上, 老实等着吧
                        //await Api.SendGroupMessageAsync(groupId, $"{CQCode.Reply(targetId, messageId)}CD还早呢, 先歇着吧").ConfigureAwait(false);
                        return true;
                    }

                    randActions = new List<RandomWeight<SendSetuConfig>>
                    {
                        new(10000, new SendSetuConfig((int)(Rand.Next(10, 60) * Math.Pow(1.1, setuDoushiLv)), SetuAddLevel.Normal)),
                        new(3500, new SendSetuConfig((int)(Rand.Next(10, 60) * 2 * Math.Pow(1.1, setuDoushiLv)), SetuAddLevel.Double)),
                        new(1500, new SendSetuConfig((int)(Rand.Next(10, 60) * 4 * Math.Pow(1.1, setuDoushiLv)), SetuAddLevel.SuperDouble)),
                        new(750, new SendSetuConfig((int)(Rand.Next(10, 60) * 8 * Math.Pow(1.1, setuDoushiLv)), SetuAddLevel.Golden)),
                        new(300, new SendSetuConfig((int)(Rand.Next(10, 60) * 16 * Math.Pow(1.1, setuDoushiLv)), SetuAddLevel.Platinum)),
                        new(150, new SendSetuConfig((int)(Rand.Next(10, 60) * 32 * Math.Pow(1.1, setuDoushiLv)), SetuAddLevel.Diamond)),
                        new(10, new SendSetuConfig((int)(Rand.Next(10, 60) * 256 * Math.Pow(1.1, setuDoushiLv)), SetuAddLevel.Death)),
                        new(1500, new SendSetuConfig(Rand.Next(5, 60) * -1, SetuAddLevel.Luck)),
                        new(600, new SendSetuConfig(Rand.Next(5, 60) * -4, SetuAddLevel.LuckSuper)),
                        new(200, new SendSetuConfig(Rand.Next(5, 60) * -16, SetuAddLevel.LuckGolden)),
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
                if (tag!.Contains('&') || tag!.Contains("%26"))
                    SetSetuValues(new SendSetuConfig(3600, SetuAddLevel.Death));

                // MEMO : 色图Lv增加
                if (canSendSetu)
                {
                    if (isSearchTag && !message.StartsWith(COMMAND_CUSTOM_GROUP_SETU_LIBRARY, StringComparison.CurrentCultureIgnoreCase))
                    {
                        changeLvTag = 1;
                        setuDoushiLv++;
                    }

                    if ((dateNow - setuCd).TotalSeconds <= 180)
                    {
                        changeLvFast = 1;
                        setuDoushiLv++;
                    }
                }

                if (setuDoushiLv > MaxSenderLv)
                    setuDoushiLv = MaxSenderLv;

                setuDoushiInfo.SetuDoushiLv = setuDoushiLv;
                BotDb.Update(setuDoushiInfo);
            }

            if (PublicVar.IsDebug)
            {
                await Api.SendGroupMessageAsync(groupId, "[DEBUG]" +
                                                         $"{ENTER}目标对象: {targetId}" +
                                                         $"{ENTER}色图Lv: {setuDoushiLv}" +
                                                         $"{ENTER}是否发送: {canSendSetu}" +
                                                         $"{ENTER}增加时间: {addSecond}s" +
                                                         $"{ENTER}色图CD: {GetCD(setuDoushiInfo)}")
                    .ConfigureAwait(false);
            }

            var addSetuSenderLv = setuDoushiLv - oldSetuSenderLv;
            var isFree = false;
            if (!canSendSetu)
            {
                AddCD();
                var isShowDate = Rand.Next(0, 100) <= 3;
                string sendMessage;
                if (addSecond > 0)
                {
                    // MEMO : CD增加
                    sendMessage = $"{CQCode.At(targetId)}" +
                                  $"{_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}" +
                                  $"的CD{_setuCDWasAdded.Random().Replace("$ADD_LEVEL$", addLevel.ToAddLevelString())}" +
                                  GetSetuLvInfo() +
                                  (isShowDate ? $" [CD {GetCD(setuDoushiInfo)}]" : string.Empty);
                }
                else
                {
                    // MEMO : 幸运(CD减少)
                    sendMessage = $"{CQCode.At(targetId)}"
                                  + $"运气好, {_setuCDWasReduced.Random().Replace("$ADD_LEVEL$", addLevel.ToAddLevelString())}"
                                  + $" ({addSecond}s)"
                                  + GetSetuLvInfo()
                                  + (isShowDate ? $" [CD {GetCD(setuDoushiInfo)}]" : string.Empty);
                }

                await BotDb.AddAsync(new SetuSendHistory(targetId, dateNow, sourceTag, false, false, false, false))
                    .ConfigureAwait(false);
                await Api.SendGroupMessageAsync(groupId, sendMessage)
                    .ConfigureAwait(false);
                return true;
            }
            else
            {
                AddCD();
                var isShowDate = Rand.Next(0, 100) <= 3;
                if (targetId != PublicVar.AdminId && addSecond == 0)
                {
                    // MEMO : 白嫖
                    isFree = true;
                    var sendMessage = $"{CQCode.At(targetId)}"
                                      + $"什么!? 你成功白嫖了一张{sourceTag}{_setuKeyWords.Random()}!"
                                      + (isShowDate ? $" [CD {GetCD(setuDoushiInfo)}]" : string.Empty);
                    await Api.SendGroupMessageAsync(groupId, sendMessage).ConfigureAwait(false);
                    goto SendSetu;
                }
            }

            var revertCd = DateTime.MinValue;
SendSetu:
            try
            {
                var randomSetuKeyword = new List<RandomWeight<Func<string, Task<SetuInfo>>>>
                {
                    new(10, SetuExtensions.GetSetu_LoliconAsync),
                    new(8, SetuExtensions.GetSetu_YubanAsync),
                    new(3, SetuExtensions.GetSetu_JitsuAsync),
                };
                var randomSetu = new List<RandomWeight<Func<string, Task<SetuInfo>>>>
                {
                    new(10, SetuExtensions.GetSetu_LoliconAsync),
                    new(8, SetuExtensions.GetSetu_YubanAsync),
                    new(6, SetuExtensions.GetSetu_NyanCatdaAsync),
                    new(3, SetuExtensions.GetSetu_JitsuAsync),
                };
                Func<string, Task<SetuInfo>>[] randomSetuDefault =
                {
                    SetuExtensions.GetSetu_LoliconAsync,
                    SetuExtensions.GetSetu_YubanAsync,
                    SetuExtensions.GetSetu_NyanCatdaAsync,
                    SetuExtensions.GetSetu_JitsuAsync,
                };

                await Api.SendGroupMessageAsync(groupId,
                        $"{CQCode.Reply(targetId, messageId)}{_setuKeyWords.Random()}正在{_setuGetting.Random()}...")
                    .ConfigureAwait(false);

                var (setuInfo, fileName) = await GetSetu(() => isSearchTag
                    ? randomSetuKeyword.TryGetRandomWeight(out var funcResult)
                        ? funcResult.Value(tag)
                        : randomSetuDefault.Random()(tag)
                    : randomSetu.TryGetRandomWeight(out var funcResult2)
                        ? funcResult2.Value(tag)
                        : randomSetuDefault.Random()(tag),
                    false).ConfigureAwait(false);
                if (setuInfo == null)
                    return false;

                switch (setuInfo.Result)
                {
                    case SetuResult.Successed:
                        break;
                    case SetuResult.NoSearchResult:
                        await Api.SendGroupMessageAsync(groupId,
                            $"{CQCode.At(targetId)}{_setuKexiStart.Random()}" +
                            $"色图库中没找到色图~,{_setuKexiEnd.Random()} {GetSetuLvInfo()}")
                            .ConfigureAwait(false);
                        //setuDoushiInfo.SetuCD = revertCd.ToTimeStamp();
                        //BotDb.Update(setuDoushiInfo);
                        await BotDb.AddAsync(new SetuSendHistory(targetId, dateNow, sourceTag, true, false, isFree, r18Bonus))
                            .ConfigureAwait(false);
                        return true;
                    case SetuResult.ApiError:
                    case SetuResult.Timeout:
                    case SetuResult.OtherError:
                        await Api.SendGroupMessageAsync(groupId,
                            $"{CQCode.At(targetId)}{_setuKexiStart.Random()}" +
                            $"{setuInfo.Result.GetDisplay()}[{setuInfo.SetuType}],色图取得失败!{_setuKexiEnd.Random()} {GetSetuLvInfo()}")
                            .ConfigureAwait(false);
                        setuDoushiInfo.SetuCD = revertCd.ToTimeStamp();
                        BotDb.Update(setuDoushiInfo);
                        await BotDb.AddAsync(new SetuSendHistory(targetId, dateNow, sourceTag, true, false, isFree, false))
                            .ConfigureAwait(false);
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException(setuInfo.Result.ToString());
                }

                var sendMessages = new List<GroupForwardMessage>
                {
                    new(messageId),
                    new(BOT_NAME, BotId, $"{GetSetuLvInfo()}"),
                    new($"{setuInfo.SetuType}", BotId, CQCode.Image(CommonExtensions.GetPath(CACHE_DIRECTORY_NAME, fileName))),
                    new($"{setuInfo.SetuType}", BotId, $"{setuInfo.SourceText}" +
                                                       $"{ENTER}{_setuSource.Random()}:{setuInfo.SourceUrl}"),
                };

                if (r18Bonus)
                {
                    var randomSetuR18Keyword = new List<RandomWeight<Func<string, Task<SetuInfo>>>>
                    {
                        new(10, SetuExtensions.GetSetu_Lolicon_R18Async),
                        new(8, SetuExtensions.GetSetu_Yuban_R18Async),
                        new(3, SetuExtensions.GetSetu_Jitsu_R18Async),
                    };
                    var randomSetuR18 = new List<RandomWeight<Func<string, Task<SetuInfo>>>>
                    {
                        new(10, SetuExtensions.GetSetu_Lolicon_R18Async),
                        new(8, SetuExtensions.GetSetu_Yuban_R18Async),
                        new(6, SetuExtensions.GetSetu_NyanCatda_R18Async),
                        new(3, SetuExtensions.GetSetu_Jitsu_R18Async),
                    };

                    var (setuInfoR18, _) = await GetSetu(() => isSearchTag
                        ? randomSetuR18Keyword.TryGetRandomWeight(out var funcResult)
                            ? funcResult.Value(tag)
                            : randomSetuDefault.Random()(tag)
                        : randomSetuR18.TryGetRandomWeight(out var funcResult2)
                            ? funcResult2.Value(tag)
                            : randomSetuDefault.Random()(tag),
                        true).ConfigureAwait(false);
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
                        case SetuResult.NoSearchResult:
                            sendMessages.Add(new GroupForwardMessage($"{setuInfo.SetuType}", BotId,
                                $"{_setuKexiStart.Random()}" +
                                $"色图库中没找到金色传说色图~,{_setuKexiEnd.Random()} {GetSetuLvInfo()}"));
                            break;
                        case SetuResult.ApiError:
                        case SetuResult.Timeout:
                        case SetuResult.OtherError:
                            await Api.SendGroupMessageAsync(groupId,
                                $"{CQCode.At(targetId)}{_setuKexiStart.Random()}" +
                                $"{setuInfo.Result.GetDisplay()}[{setuInfo.SetuType}],金色传说色图取得失败!{_setuKexiEnd.Random()} {GetSetuLvInfo()}")
                                .ConfigureAwait(false);
                            await BotDb.AddAsync(new SetuSendHistory(targetId, dateNow, sourceTag, true, false, isFree, true))
                                .ConfigureAwait(false);
                            break;
                    }
                }

                await BotDb.AddAsync(new SetuSendHistory(targetId, dateNow, sourceTag, true, true, isFree, r18Bonus))
                    .ConfigureAwait(false);
                await Api.SendGroupForwardMessageAsync(groupId, sendMessages, 15, RunAction)
                    .ConfigureAwait(false);

                async void RunAction(ClientReceiveData clientReceiveData)
                {
                    if (clientReceiveData.IsSuccessed)
                        return;

                    // MEMO : 图片被风控, 发送文字消息
                    sendMessages[2] = new GroupForwardMessage($"{setuInfo.SetuType}", BotId, "[该图片已被风控拦截!]");
                    await Api.SendGroupForwardMessageAsync(groupId, sendMessages).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                return false;
            }

            void SetSetuValues(SendSetuConfig sendSetuConfig)
            {
                addLevel = sendSetuConfig.SetuAddLevel;
                addSecond = sendSetuConfig.AddSecond > 0
                    ? (int)(sendSetuConfig.AddSecond * (setuDoushiLv + 3.0) / 3)
                    : sendSetuConfig.AddSecond;
                canSendSetu = sendSetuConfig.CanSend;
                r18Bonus = sendSetuConfig.R18;
            }

            void AddCD()
            {
                revertCd = dateNow.AddSeconds(20);
                setuDoushiInfo.SetuCD = (setuCd > dateNow ? setuCd : dateNow)
                    .AddSeconds(addSecond).ToTimeStamp();
                BotDb.Update(setuDoushiInfo);
            }

            string GetSetuLvInfo()
            {
                var addString = string.Empty;
                if (changeLvTime < 0)
                    addString += $",冷却{changeLvTime.ToSignString()}";
                if (changeLvTag > 0)
                    addString += $",搜索{changeLvTag.ToSignString()}";
                if (changeLvFast > 0)
                    addString += $",频率快{changeLvFast.ToSignString()}";
                if (!string.IsNullOrEmpty(addString))
                    addString = addString[1..];

                var addLvString = (string.IsNullOrEmpty(addString)
                    ? string.Empty
                    : $"本次{addSetuSenderLv.ToSignString()}({addString})");
                return $"[斗士Lv{oldSetuSenderLv}] {addLvString}";
            }

            async Task<(SetuInfo, string)> GetSetu(Func<Task<SetuInfo>> getSetuInfoFunc, bool checkImageOnly)
            {
                var setuInfo = await getSetuInfoFunc().ConfigureAwait(false);
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
                            setuInfo.ImageUrl, CACHE_DIRECTORY_NAME, true, checkImageOnly).ConfigureAwait(false);
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
                    setuInfo = await getSetuInfoFunc().ConfigureAwait(false);
                    DebugSendSetuInfo(setuInfo);
                    CommonUtil.Sleep(500);
                }

                if (retryTimes > maxRetryTimes)
                {
                    await Api.SendGroupMessageAsync(groupId, "超过重试次数上限,放弃下载!").ConfigureAwait(false);
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

                async void DebugSendSetuInfo(SetuInfo stInfo)
                {
                    if (!PublicVar.IsDebug)
                        return;

                    if (stInfo.Result == SetuResult.Successed)
                    {
                        await Api.SendGroupMessageAsync(groupId, "[DEBUG]SetuInfo" +
                                                                 $"{ENTER}SetuType: {stInfo.SetuType}" +
                                                                 $"{ENTER}SetuResult: {stInfo.Result}" +
                                                                 $"{ENTER}SmallUrl: {stInfo.ImageUrl}" +
                                                                 $"{ENTER}SourceUrl: {stInfo.SourceUrl}")
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        await Api.SendGroupMessageAsync(groupId, "[DEBUG]SetuInfo" +
                                                                 $"{ENTER}SetuType: {stInfo.SetuType}" +
                                                                 $"{ENTER}SetuResult: {stInfo.Result}")
                            .ConfigureAwait(false);
                    }
                }
            }
        }
        else
        {
            if (message.ContainsAny(_setuBuman) && (dateNow - setuSendHistory).TotalSeconds <= 30)
            {
                var addSecond = Rand.Next(60 + (int)setuDoushiLv * 10, 600 + (int)setuDoushiLv * 600);
                var randActions = new List<RandomWeight<int>>
                {
                    new(1000, 0),
                    new(100, 1),
                    new(30, 2),
                    new(5, 3)
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
                setuDoushiInfo.SetuDoushiLv = setuDoushiLv + addSetuSenderLv;
                setuDoushiInfo.SetuCD = (setuCd > dateNow ? setuCd : dateNow)
                    .AddSeconds(addSecond).ToTimeStamp();
                var sendMessage = $"{CQCode.At(targetId)}" +
                                  $"{_setuKeyWords.Random()}的CD神秘地{_setuCDWasAdded.Random().Replace("$ADD_LEVEL$", SetuAddLevel.SuperDouble.ToAddLevelString())}" +
                                  $"[斗士Lv{setuDoushiLv}] {addLvString}";
                BotDb.Update(setuDoushiInfo);
                await Api.SendGroupMessageAsync(groupId, sendMessage).ConfigureAwait(false);
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
            _ => throw new ArgumentOutOfRangeException(nameof(setuAddLevel), setuAddLevel, null)
        };
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
}