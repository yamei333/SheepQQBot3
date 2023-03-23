using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.Setu;
using SheepQQBot3.SDK.Client;
using Yamei.Common;
using static CommonLibrary.RandomWeightUtil;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    public static partial class ProcessGroupMessage
    {
        /// <summary>
        /// 色图命令的开头
        /// </summary>
        private const string COMMAND_CUSTOM_GROUP_SETU_LIBRARY = "#ST#";

        /// <summary>
        /// 色图CD命令
        /// </summary>
        private const string COMMAND_CUSTOM_GROUP_SETUCD_LIBRARY = "#STCD#";

        /// <summary>
        /// 色图的基础CD, 不能发得太频繁
        /// </summary>
        private const int sendBaseDelay = 300;

        private static List<string> _setuKeyWords;

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
            { "rjj", "甘雨" },
            { "rjj2", "甘雨" }
        };

        /// <summary>
        /// 随机色图
        /// </summary>
        /// <param name="config">配置</param>
        /// <param name="groupMessage"><see cref="GroupMessage"/></param>
        public static async Task<bool> RandomSetu(SetConfig config, GroupMessage groupMessage)
        {
            var groupId = groupMessage.GroupId;
            var targetId = groupMessage.Sender.User_Id;
            var message = groupMessage.Message;
            var upperMessage = message.ToUpper();
            var dateNow = DateTime.Now;
            if (targetId == PublicVar.AdminId && upperMessage == COMMAND_CUSTOM_GROUP_SETUCD_LIBRARY)
            {
                // MEMO : 显CD
                var sendMessage = $"当前色图CD为 [{GetCD()}]";
                await Api.SendGroupMessage(groupId, sendMessage);
                return true;
            }

            if (_setuKeyWords == null)
            {
                _setuKeyWords = new List<string>();
                var startText = new[]
                {
                    "涩","色","瑟","铯"
                };
                var endText = new[]
                {
                    "图","囤","圖","図","屯"
                };

                startText.ForEach(eachStart => endText.ForEach(eachEnd => _setuKeyWords.Add(eachStart + eachEnd)));
            }

            // MEMO : 命令为#st#
            // MEMO : 或者字数在10字以内, 并包含色图关键字
            var tag = string.Empty;
            var setuKeywordCheck = false;
            if (upperMessage.StartsWith(COMMAND_CUSTOM_GROUP_SETU_LIBRARY))
            {
                setuKeywordCheck = true;
                tag = message[4..];
                goto StartSetu;
            }

            if (upperMessage.GetByteCount() <= 24 && _setuKeyWords.Any(upperMessage.EndsWith))
            {
                setuKeywordCheck = true;
                tag = message[..^2];
                goto StartSetu;
            }

StartSetu:
            if (setuKeywordCheck)
            {
                var r18Bonus = false;
                var addSecond = 0;
                var addLevel = SetuAddLevel.Normal;
                var canSendSetu = false;
                if (!PublicVar.IsDebug && targetId == PublicVar.AdminId)
                {
                    // MEMO : ADMIN无限制要色图
                    canSendSetu = true;
                }
                else
                {
                    var hasHistory = config.SetuSendHistorys.TryGetValue(groupId, out var nextCanSendDate);
                    List<RandomWeight<SendSetuConfig>> randActions;
                    if (!hasHistory || dateNow > nextCanSendDate)
                    {
                        if ((dateNow - nextCanSendDate).TotalMinutes >= 30)
                        {
                            randActions = new List<RandomWeight<SendSetuConfig>>
                            {
                                new(1000, new SendSetuConfig(sendBaseDelay + Rand.Next(-180, 180), SetuAddLevel.Normal, true)),
                                new(50, new SendSetuConfig(0, SetuAddLevel.Free, true)),
                                new(10, new SendSetuConfig(sendBaseDelay + Rand.Next(-180, 180), SetuAddLevel.Normal, true, true)),
                                new(3, new SendSetuConfig(0, SetuAddLevel.Free, true, true)),
                            };
                        }
                        else
                        {
                            randActions = new List<RandomWeight<SendSetuConfig>>
                            {
                                new(1000, new SendSetuConfig(sendBaseDelay + Rand.Next(-180, 180), SetuAddLevel.Normal, true)),
                                new(200, new SendSetuConfig(Rand.Next(3, 15), SetuAddLevel.Normal)),
                                new(200, new SendSetuConfig(Rand.Next(5, 30), SetuAddLevel.Normal)),
                                new(100, new SendSetuConfig(Rand.Next(8, 45), SetuAddLevel.Double)),
                                new(50, new SendSetuConfig(Rand.Next(10, 60), SetuAddLevel.SuperDouble)),
                                new(50, new SendSetuConfig(0, SetuAddLevel.Free, true)),
                                new(10, new SendSetuConfig(sendBaseDelay + Rand.Next(-180, 180), SetuAddLevel.Normal, true, true)),
                                new(3, new SendSetuConfig(0, SetuAddLevel.Free, true, true)),
                            };
                        }
                    }
                    else
                    {
                        randActions = new List<RandomWeight<SendSetuConfig>>
                        {
                            new(10000, new SendSetuConfig(Rand.Next(10, 60), SetuAddLevel.Normal)),
                            new(3500, new SendSetuConfig(Rand.Next(10, 60) * 2, SetuAddLevel.Double)),
                            new(1500, new SendSetuConfig(Rand.Next(10, 60) * 4, SetuAddLevel.SuperDouble)),
                            new(750, new SendSetuConfig(Rand.Next(10, 60) * 8, SetuAddLevel.Golden)),
                            new(300, new SendSetuConfig(Rand.Next(10, 60) * 16, SetuAddLevel.Platinum)),
                            new(150, new SendSetuConfig(Rand.Next(10, 60) * 32, SetuAddLevel.Diamond)),
                            new(1500, new SendSetuConfig(Rand.Next(5, 60) * -1, SetuAddLevel.Luck)),
                            new(750, new SendSetuConfig(Rand.Next(5, 60) * -4, SetuAddLevel.LuckSuper)),
                            new(300, new SendSetuConfig(Rand.Next(5, 60) * -16, SetuAddLevel.LuckGolden)),
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
                }

                if (PublicVar.IsDebug)
                {
                    await Api.SendGroupMessage(groupId, "[DEBUG]" +
                                                        $"{ENTER}是否可发: {canSendSetu}" +
                                                        $"{ENTER}增加时间: {addSecond}s" +
                                                        $"{ENTER}色图CD: {GetCD()}");
                }

                if (!canSendSetu)
                {
                    AddCD();
                    var isShowDate = Rand.Next(0, 100) <= 3;
                    var sendMessage = string.Empty;
                    if (addSecond > 0)
                    {
                        // MEMO : CD增加
                        sendMessage = $"{CQCode.At(targetId)} " +
                            $"{_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}" +
                            $"的CD{_setuCDWasAdded.Random().Replace("$ADD_LEVEL$", addLevel.ToAddLevelString())}" +
                            //+ $" (+{addSecond}s)"
                            (isShowDate ? $" [CD {GetCD()}]" : string.Empty);
                    }
                    //else if (addSecond == 0)
                    //{
                    //    // MEMO : 白嫖
                    //    sendMessage = $"{CQCode.at(targetId)} "
                    //                  + $"什么!? 你成功白嫖了一张{_setuKeyWords.Random()}!"
                    //                  + (isShowDate ? $" [CD {config.SetuSendHistorys[groupId]:HH:mm:ss}]" : string.Empty);
                    //    await Api.SendGroupMessage(groupId, sendMessage);
                    //    goto SendSetu;
                    //}
                    else
                    {
                        // MEMO : 幸运(CD减少)
                        sendMessage = $"{CQCode.At(targetId)} "
                                      + $"运气好, {_setuCDWasReduced.Random().Replace("$ADD_LEVEL$", addLevel.ToAddLevelString())}"
                                      + $" ({addSecond}s)"
                                      + (isShowDate ? $" [CD {GetCD()}]" : string.Empty);
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
                        var sendMessage = $"{CQCode.At(targetId)} "
                            + $"什么!? 你成功白嫖了一张{_setuKeyWords.Random()}!"
                            + (isShowDate ? $" [CD {GetCD()}]" : string.Empty);
                        await Api.SendGroupMessage(groupId, sendMessage);
                        goto SendSetu;
                    }
                }

                var revertCD = DateTime.MinValue;
SendSetu:
                try
                {
                    Func<string, Task<SetuInfo>>[] randomSetuKeyword = {
                        SetuExtensions.GetSetu_Lolicon,
                        SetuExtensions.GetSetu_Yuban,
                        SetuExtensions.GetSetu_Jitsu,
                    };
                    Func<string, Task<SetuInfo>>[] randomSetu = {
                        SetuExtensions.GetSetu_Lolicon,
                        SetuExtensions.GetSetu_Yuban,
                        SetuExtensions.GetSetu_NyanCatda,
                        SetuExtensions.GetSetu_Jitsu,
                    };

                    await Api.SendGroupMessage(groupId,
                        $"{_setuKeyWords.Random()}正在{_setuGetting.Random()}...");

                    var (setuInfo, fileName) = await GetSetu(() => !string.IsNullOrEmpty(tag)
                        ? randomSetuKeyword.Random().Invoke(tag)
                        : randomSetu.Random().Invoke(tag),
                        false);
                    if (setuInfo == null)
                        return false;

                    if (!setuInfo.IsSuccess)
                    {
                        await Api.SendGroupMessage(groupId, $"{CQCode.At(targetId)}{_setuKexiStart.Random()}色图库中没找到色图~,{_setuKexiEnd.Random()}.");
                        config.SetuSendHistorys[groupId] = revertCD;
                        return true;
                    }

                    await Api.SendGroupMessage(groupId,
                        "[预览图等后续]" +
                        $"{ENTER}{setuInfo.SourceText}" +
                        $"{ENTER}{_setuSource.Random()}:{setuInfo.SourceUrl}" +
                        $"{ENTER}API提供:{setuInfo.SetuType}" +
                        $"{ENTER}{CQCode.At(targetId)}{_setuYouwant.Random()}{_setuKeyWords.Random()}{_setuGetted.Random()}");

                    await Api.SendGroupMessage(groupId,
                        CQCode.Image(CommonExtensions.GetCachePath(fileName)));

                    if (r18Bonus)
                    {
                        Func<string, Task<SetuInfo>>[] randomSetuR18Keyword = {
                            SetuExtensions.GetSetu_Lolicon_R18,
                            SetuExtensions.GetSetu_Yuban_R18,
                            SetuExtensions.GetSetu_Jitsu_R18,
                        };
                        Func<string, Task<SetuInfo>>[] randomSetuR18 = {
                            SetuExtensions.GetSetu_Lolicon_R18,
                            SetuExtensions.GetSetu_Yuban_R18,
                            SetuExtensions.GetSetu_NyanCatda_R18,
                            SetuExtensions.GetSetu_Jitsu_R18,
                        };

                        var (setuInfoR18, _) = await GetSetu(() => !string.IsNullOrEmpty(tag)
                            ? randomSetuR18Keyword.Random().Invoke(tag)
                            : randomSetuR18.Random().Invoke(tag),
                            true);
                        if (setuInfoR18 == null)
                            return false;

                        if (!setuInfoR18.IsSuccess)
                        {
                            await Api.SendGroupMessage(groupId, $"{CQCode.At(targetId)}{_setuKexiStart.Random()}色图库中没找到金色传说色图~,{_setuKexiEnd.Random()}.");
                            return true;
                        }

                        await Api.SendGroupMessage(groupId,
                            $"[这是一张额外的金色传说色图, 不可预览]" +
                            $"{ENTER}{setuInfoR18.SourceText}" +
                            $"{ENTER}{_setuSource.Random()}:{setuInfoR18.SourceUrl}" +
                            $"{ENTER}API提供:{setuInfoR18.SetuType}" +
                            $"{ENTER}{CQCode.At(targetId)}{_setuYouwant.Random()}{_setuKeyWords.Random()}{_setuGetted.Random()}");
                    }
                }
                catch (Exception)
                {
                    return false;
                }

                void SetSetuValues(SendSetuConfig sendSetuConfig)
                {
                    addLevel = sendSetuConfig.SetuAddLevel;
                    addSecond = sendSetuConfig.AddSecond;
                    canSendSetu = sendSetuConfig.CanSend;
                    r18Bonus = sendSetuConfig.R18;
                }

                void AddCD()
                {
                    revertCD = dateNow.AddSeconds(Rand.Next(3, 15));
                    config.SetuSendHistorys[groupId] = config.SetuSendHistorys.ContainsKey(groupId)
                        ? (config.SetuSendHistorys[groupId] > dateNow
                            ? config.SetuSendHistorys[groupId]
                            : dateNow).AddSeconds(addSecond)
                        : dateNow.AddSeconds(addSecond);
                }

                async Task<(SetuInfo, string)> GetSetu(Func<Task<SetuInfo>> getSetuInfoFunc, bool checkImageOnly)
                {
                    var setuInfo = await getSetuInfoFunc.Invoke();
                    if (PublicVar.IsDebug)
                    {
                        await Api.SendGroupMessage(groupId, "[DEBUG]SetuInfo" +
                            $"{ENTER}SearchTag: {tag}" +
                            $"{ENTER}SetuType: {setuInfo.SetuType}" +
                            $"{ENTER}SmallUrl: {setuInfo.ImageUrl}" +
                            $"{ENTER}SourceUrl: {setuInfo.SourceUrl}" +
                            $"{ENTER}IsSuccess: {setuInfo.IsSuccess}");
                    }

                    var fileName = string.Empty;
                    var getSuccessed = false;
                    const int maxRetryTimes = 5;
                    var retryTimes = 0;
                    while (!getSuccessed)
                    {
                        if (!setuInfo.IsSuccess)
                        {
                            getSuccessed = true;
                            continue;
                        }

                        if (string.IsNullOrEmpty(setuInfo.ImageUrl))
                        {
                            retryTimes++;
                            setuInfo = await getSetuInfoFunc.Invoke();
                            CommonUtil.Sleep(500);
                            continue;
                        }

                        (getSuccessed, fileName) = await HttpExtensions.HttpDownloadAsync(setuInfo.ImageUrl, checkImageOnly);
                        if (getSuccessed)
                            continue;

                        retryTimes++;
                        if (retryTimes <= maxRetryTimes)
                        {
                            //await Api.SendGroupMessage(groupId,
                            //    $"啊, 该{_setuKeyWords.Random()}被作者删了!{ENTER}正在第{retryTimes}次重新{_setuGetting.Random()}...");
                            setuInfo = await getSetuInfoFunc.Invoke();
                            if (PublicVar.IsDebug)
                            {
                                await Api.SendGroupMessage(groupId, "[DEBUG]SetuInfo" +
                                                                    $"{ENTER}SetuType: {setuInfo.SetuType}" +
                                                                    $"{ENTER}SmallUrl: {setuInfo.ImageUrl}" +
                                                                    $"{ENTER}SourceUrl: {setuInfo.SourceUrl}");
                            }
                        }
                        else
                        {
                            getSuccessed = true;
                        }

                        CommonUtil.Sleep(500);
                    }

                    if (retryTimes > maxRetryTimes)
                    {
                        await Api.SendGroupMessage(groupId, "超过重试次数上限,放弃下载!");
                        return (setuInfo, string.Empty);
                    }

                    if (!checkImageOnly && setuInfo.IsSuccess)
                    {
                        var isFileExists = false;
                        while (!isFileExists)
                        {
                            isFileExists = File.Exists($"Cache/{fileName}");
                            CommonUtil.Sleep(100);
                        }
                        CommonExtensions.DeleteExpiredCache();
                    }

                    return (setuInfo, fileName);
                }
            }
            else
            {
                return false;
            }

            return true;

            string GetCD()
            {
                return $"{(config.SetuSendHistorys.ContainsKey(groupId)
                    ? (dateNow >= config.SetuSendHistorys[groupId]
                        ? "可使用"
                        : config.SetuSendHistorys[groupId].ToString("HH:mm:ss"))
                    : "无记录")}";
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