using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Extension;
using SheepQQBot3.Model.Setu;
using Yamei.Common;
using static SheepQQBot3.View.PublicVar;

namespace SheepQQBot3.View
{
    public static partial class ProcessGroupMessage
    {
        /// <summary>
        /// 群提醒方法命令的开头
        /// </summary>
        private const string COMMAND_CUSTOM_GROUP_SETU_LIBRARY = "#ST#";

        /// <summary>
        /// 下一次可发色图的时间
        /// </summary>
        private static DateTime canSendDate = DateTime.MinValue;

        /// <summary>
        /// 色图的CD, 不能发得太频繁
        /// </summary>
        private const int sendDelay = 600;

        private static List<string> _setuKeyWords;

        private static readonly string[] _setuYouwant = {
            string.Empty, "你要的", "你点的", "请求的", "申请的"
        };

        private static readonly string[] _setuGet = {
            "来了", "已经送出", "到了", "来咯", "lei了", "已发送"
        };

        private static readonly string[] _setuSource = {
            "原图", "大图", "查看大图", "原图查看", "源链接", "图源"
        };

        private static readonly string[] _setuNo = {
            "别", "憋", "鳖"
        };

        private static readonly string[] _setuSendLe = {
            "发了", "要了", "整了", "冲了"
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
                    "图","囤","圖"
                };

                startText.ForEach(eachStart => endText.ForEach(eachEnd => _setuKeyWords.Add(eachStart + eachEnd)));
            }

            // MEMO : 命令为#st#
            // MEMO : 或者字数在4字以内, 并包含色图关键字
            if (upperMessage == COMMAND_CUSTOM_GROUP_SETU_LIBRARY
                || upperMessage.GetByteCount() <= 12 && _setuKeyWords.Any(each => upperMessage.Contains(each)))
            {
                var dateNow = DateTime.Now;
                var r18bonus = false;
                if (dateNow > canSendDate)
                {
                    var randNum = Rand.Next(0, 100000);
                    switch (randNum)
                    {
                        case var n when n >= 30000 && n < 100000:
                            canSendDate = dateNow.AddSeconds(sendDelay).AddSeconds(Rand.Next(-180, 300));
                            break;
                        case var n when n >= 15000 && n < 30000:
                            await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}的CD被增加了");
                            canSendDate = dateNow.AddSeconds(Rand.Next(5, 15));
                            return true;
                        case var n when n >= 300 && n < 15000:
                            await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}的CD被增加了");
                            canSendDate = dateNow.AddSeconds(Rand.Next(10, 30));
                            return true;
                        case var n when n >= 0 && n < 300:
                            canSendDate = dateNow.AddSeconds(sendDelay).AddSeconds(Rand.Next(-180, 300));
                            r18bonus = true;
                            break;
                    }
                }
                else
                {
                    var randNum = Rand.Next(0, 100000);
                    switch (randNum)
                    {
                        case var n when n >= 40000 && n < 100000:
                            await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}的CD被增加了");
                            canSendDate = canSendDate.AddSeconds(Rand.Next(10, 120));
                            return true;
                        case var n when n >= 14000 && n < 40000:
                            await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}的CD被加倍了!");
                            canSendDate = canSendDate.AddSeconds(Rand.Next(10 * 2, 120 * 2));
                            return true;
                        case var n when n >= 4900 && n < 14000:
                            await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}的CD被超级加倍了!!");
                            canSendDate = canSendDate.AddSeconds(Rand.Next(10 * 4, 120 * 4));
                            return true;
                        case var n when n >= 4900 && n < 14000:
                            await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}的CD被黄金加倍了!!!");
                            canSendDate = canSendDate.AddSeconds(Rand.Next(10 * 8, 120 * 8));
                            return true;
                        case var n when n >= 2000 && n < 4900:
                            await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, {_setuKeyWords.Random()}的CD被白金加倍了!!!!");
                            canSendDate = canSendDate.AddSeconds(Rand.Next(10 * 16, 120 * 16));
                            return true;
                        case var n when n >= 1000 && n < 2000:
                            await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, 但运气好, {_setuKeyWords.Random()}的CD被缩短了!");
                            canSendDate = canSendDate.AddSeconds(-Rand.Next(180, 600));
                            return true;
                        case var n when n >= 0 && n < 1000:
                            await Api.SendGroupMessage(groupId, $"[CQ:at,qq={targetId}] {_setuNo.Random()}{_setuSendLe.Random()}, 但运气好, {_setuKeyWords.Random()}的CD被超级缩短了!");
                            canSendDate = canSendDate.AddSeconds(-Rand.Next(300, 900));
                            return true;
                    }
                }

                try
                {
                    Func<Task<SetuInfo>>[] randomSetu = {
                        SetuExtensions.GetSetu_Lolicon,
                        SetuExtensions.GetSetu_Yuban
                    };

                    var setuInfo = await randomSetu.Random().Invoke();
                    var fileName = await HttpExtensions.HttpDownloadAsync(setuInfo.ImageUrl, "png");
                    CommonExtensions.DeleteExpiredCache();
                    await Api.SendGroupMessage(groupId, $"[CQ:image,file={CommonExtensions.GetCachePath(fileName)}]" +
                                                        $"\n{setuInfo.SourceText}" +
                                                        $"\n{_setuSource.Random()}:{setuInfo.SourceUrl}" +
                                                        $"\n[CQ:at,qq={targetId}] {_setuYouwant.Random()}{_setuKeyWords.Random()}{_setuGet.Random()}");

                    if (r18bonus)
                    {
                        var setuInfoR18 = SetuExtensions.GetSetu_Lolicon_R18().Result;
                        await Api.SendGroupMessage(groupId, $"[这是一张额外的金色传说色图, 不可预览]" +
                                                            $"\n{setuInfoR18.SourceText}" +
                                                            $"\n{_setuSource.Random()}:{setuInfoR18.SourceUrl}" +
                                                            $"\n[CQ:at,qq={targetId}] {_setuYouwant.Random()}{_setuKeyWords.Random()}{_setuGet.Random()}");
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