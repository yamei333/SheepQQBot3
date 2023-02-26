using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.Setu;
using SheepQQBot3.SDK.Client;
using Yamei.Common;
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
        /// 下一次可发色图的时间
        /// </summary>
        private static DateTime canSendDate = DateTime.MinValue;

        /// <summary>
        /// 色图的基础CD, 不能发得太频繁
        /// </summary>
        private const int sendDelay = 600;

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

        /// <summary>
        /// 随机色图
        /// </summary>
        /// <param name="groupMessage"><see cref="GroupMessage"/></param>
        public static async Task<bool> RandomSetu(GroupMessage groupMessage)
        {
            var groupId = groupMessage.GroupId;
            var targetId = groupMessage.Sender.User_Id;
            var message = groupMessage.Message;
            var upperMessage = message.ToUpper();
            if (_setuKeyWords == null)
            {
                _setuKeyWords = new List<string>();
                var startText = new[]
                {
                    "涩","色","瑟"
                };
                var endText = new[]
                {
                    "图","囤","圖","図","屯"
                };

                startText.ForEach(eachStart => endText.ForEach(eachEnd => _setuKeyWords.Add(eachStart + eachEnd)));
            }

            // MEMO : 命令为#st#
            // MEMO : 或者字数在4字以内, 并包含色图关键字
            if (upperMessage.StartsWith(COMMAND_CUSTOM_GROUP_SETU_LIBRARY)
                || upperMessage.GetByteCount() <= 12
                && _setuKeyWords.Any(each => upperMessage.Contains(each)))
            {
                var r18Bonus = false;
                if (targetId == PublicVar.ADMIN_ID)
                {
                    // MEMO : ADMIN无限制要色图
                }
                else
                {
                    var dateNow = DateTime.Now;
                    if (dateNow > canSendDate)
                    {
                        var randNum = Rand.Next(0, 100000);
                        switch (randNum)
                        {
                            case < 100000 and >= 30000:
                                canSendDate = dateNow.AddSeconds(sendDelay).AddSeconds(Rand.Next(-180, 300));
                                break;
                            case < 30000 and >= 15000:
                                await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}的CD被增加了");
                                canSendDate = dateNow.AddSeconds(Rand.Next(5, 15));
                                return true;
                            case < 15000 and >= 300:
                                await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}的CD被增加了");
                                canSendDate = dateNow.AddSeconds(Rand.Next(10, 30));
                                return true;
                            case < 300 and >= 0:
                                canSendDate = dateNow.AddSeconds(sendDelay).AddSeconds(Rand.Next(-180, 300));
                                r18Bonus = true;
                                break;
                        }
                    }
                    else
                    {
                        var randNum = Rand.Next(0, 100000);
                        switch (randNum)
                        {
                            case < 100000 and >= 40000:
                                await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}的CD被增加了");
                                canSendDate = canSendDate.AddSeconds(Rand.Next(10, 120));
                                return true;
                            case < 40000 and >= 14000:
                                await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}的CD被加倍了!");
                                canSendDate = canSendDate.AddSeconds(Rand.Next(10 * 2, 120 * 2));
                                return true;
                            case < 14000 and >= 8900:
                                await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}的CD被超级加倍了!!");
                                canSendDate = canSendDate.AddSeconds(Rand.Next(10 * 4, 120 * 4));
                                return true;
                            case < 8900 and >= 4000:
                                await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}的CD被黄金加倍了!!!");
                                canSendDate = canSendDate.AddSeconds(Rand.Next(10 * 8, 120 * 8));
                                return true;
                            case < 4900 and >= 2000:
                                await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}的CD被白金加倍了!!!!");
                                canSendDate = canSendDate.AddSeconds(Rand.Next(10 * 16, 120 * 16));
                                return true;
                            case < 2000 and >= 1000:
                                await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, 但运气好, {_setuKeyWords.Random()}的CD被缩短了!");
                                canSendDate = canSendDate.AddSeconds(-Rand.Next(180, 600));
                                return true;
                            case < 1000 and >= 0:
                                await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, 但运气好, {_setuKeyWords.Random()}的CD被超级缩短了!");
                                canSendDate = canSendDate.AddSeconds(-Rand.Next(300, 900));
                                return true;
                        }
                    }
                }

                try
                {
                    Func<Task<SetuInfo>>[] randomSetu = {
                        SetuExtensions.GetSetu_Lolicon,
                        SetuExtensions.GetSetu_Yuban
                    };

                    await Api.SendGroupMessage(groupId,
                        $"[CQ:at,qq={targetId}] {_setuKeyWords.Random()}正在{_setuGetting.Random()}...");
                    var setuInfo = await randomSetu.Random().Invoke();
                    var fileName = string.Empty;
                    var getSuccessed = false;
                    const int maxRetryTimes = 5;
                    var retryTimes = 0;
                    while (!getSuccessed)
                    {
                        (getSuccessed, fileName) = await HttpExtensions.HttpDownloadAsync(setuInfo.ImageUrl);
                        if (getSuccessed)
                            continue;

                        retryTimes++;
                        if (retryTimes <= maxRetryTimes)
                        {
                            await Api.SendGroupMessage(groupId,
                                $"啊, 该{_setuKeyWords.Random()}被作者删了!{ENTER}正在第{retryTimes}次重新{_setuGetting.Random()}...");
                            setuInfo = await randomSetu.Random().Invoke();
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
                        return false;
                    }

                    var isFileExists = false;
                    while (!isFileExists)
                    {
                        isFileExists = File.Exists($"Cache/{fileName}");
                        CommonUtil.Sleep(100);
                    }

                    CommonExtensions.DeleteExpiredCache();
                    await Api.SendGroupMessage(groupId,
                        $"[CQ:image,file={CommonExtensions.GetCachePath(fileName)}]"
                        + $"{ENTER}{setuInfo.SourceText}"
                        + $"{ENTER}{_setuSource.Random()}:{setuInfo.SourceUrl}"
                        + $"{ENTER}[CQ:at,qq={targetId}] {_setuYouwant.Random()}{_setuKeyWords.Random()}{_setuGetted.Random()}");

                    if (r18Bonus)
                    {
                        var setuInfoR18 = SetuExtensions.GetSetu_Lolicon_R18().Result;
                        await Api.SendGroupMessage(groupId,
                            $"[这是一张额外的金色传说色图, 不可预览]"
                            + $"{ENTER}{setuInfoR18.SourceText}"
                            + $"{ENTER}{_setuSource.Random()}:{setuInfoR18.SourceUrl}"
                            + $"{ENTER}[CQ:at,qq={targetId}] {_setuYouwant.Random()}{_setuKeyWords.Random()}{_setuGetted.Random()}");
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}