using Masuit.Tools;
using SheepQQBot3.Enums;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace System;

public static class CommonExtensions
{
    private const string KH_LEFT = @"&#91;";
    private const string KH_RIGHT = @"&#93;";

    public static bool CheckPercent(this Random rand, int percent)
        => rand.Next(100) + 1 <= percent;

    extension(DateTime input)
    {
        public string ToConditionString(Dictionary<string, bool> holidayInfo)
            => input.ToString($"yyyy-MM-dd-{DayOfWeek2Int(input.DayOfWeek)}-{(input.IsHoliday(holidayInfo) ? 1 : 0)} HH:mm:ss");

        public string ToYYYYMDHHMMSS() => input.ToString("yyyy-M-d HH:mm:ss");
        public string ToYYYYMD() => input.ToString("yyyy-M-d");
        public string ToYYYYMMDD() => input.ToString("yyyy-MM-dd");
        public string ToYYYYMM() => input.ToString("yyyy-MM");

        /// <summary>
        /// 判定是否节假日
        /// </summary>
        public bool IsHoliday(Dictionary<string, bool> holidayInfo)
        {
            switch (input.DayOfWeek)
            {
                case DayOfWeek.Monday:
                case DayOfWeek.Tuesday:
                case DayOfWeek.Wednesday:
                case DayOfWeek.Thursday:
                case DayOfWeek.Friday:
                    return holidayInfo != null
                        && holidayInfo.TryGetValue(input.ToYYYYMMDD(), out var isHoliday1)
                        && isHoliday1;
                case DayOfWeek.Saturday:
                case DayOfWeek.Sunday:
                    return holidayInfo == null
                        || !holidayInfo.TryGetValue(input.ToYYYYMMDD(), out var isHoliday2)
                        || isHoliday2;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// <see cref="DayOfWeek"/>转换为数字
    /// </summary>
    public static int DayOfWeek2Int(DayOfWeek dayOfWeek)
        => dayOfWeek == DayOfWeek.Sunday ? 7 : (int)dayOfWeek;

    /// <summary>
    /// 替换普通内容为CQ代码
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <param name="senderId">发送者Id</param>
    /// <returns>替换结果</returns>
    public static (string Result, bool IsNoAt, bool IsNoReply, bool IsLoop, bool isBark) ToCqCode(
        this string input, string senderId = "")
    {
        var result = input;
        var isNoAt = Replace($@"\{KH_LEFT}-na{KH_RIGHT}", string.Empty);
        var isNoReply = Replace($@"\{KH_LEFT}-nr{KH_RIGHT}", string.Empty);
        var isLoop = Replace($@"\{KH_LEFT}-loop{KH_RIGHT}", string.Empty);
        var isBark = Replace($@"\{KH_LEFT}-bark{KH_RIGHT}", string.Empty);
        Replace($@"\{KH_LEFT}at-self{KH_RIGHT}", $"[CQ:at,qq={senderId}]");
        Replace($@"\{KH_LEFT}at-(?<str>[0-9]+?){KH_RIGHT}", "[CQ:at,qq=${str}]");
        Replace($@"\{KH_LEFT}image-(?<str>.+?){KH_RIGHT}", "[CQ:image,file=${str}]");
        Replace($@"\{KH_LEFT}play-(?<str>.+?){KH_RIGHT}", "[CQ:ym_play,file=${str}]");
        Replace($@"{KH_LEFT}play3-(?<str>.+?){KH_RIGHT}", "[CQ:ym_play3,file=${str}]");
        return (result, isNoAt, isNoReply, isLoop, isBark);

        bool Replace(string pattern, string replacement)
        {
            var newResult = Regex.Replace(result, pattern, replacement);
            var isMatch = newResult != result;
            result = newResult;
            return isMatch;
        }
    }

    /// <summary>
    /// 替换CQ代码为普通内容
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <returns>替换结果</returns>
    public static string ToNormalText(this string input)
    {
        var result = input;
        Replace(@"\[CQ:at,qq=(?<str>[0-9]+?)]", "[at-${str}]");
        Replace(@"\[CQ:image,file=(?<str>.+?)\]", "[image-${str}]");
        return result;

        void Replace(string pattern, string replacement)
            => result = Regex.Replace(result, pattern, replacement);
    }

    public static void Sleep(int timeout, Func<bool> condition = null)
        => SpinWait.SpinUntil(condition ?? (() => false), timeout);

    public static void SleepSeconds(int timeout, Func<bool> condition = null)
        => SpinWait.SpinUntil(condition ?? (() => false), timeout * 1000);

    public static void SleepMinutes(int timeout, Func<bool> condition = null)
        => SpinWait.SpinUntil(condition ?? (() => false), timeout * 60000);

    public static void SleepHours(int timeout, Func<bool> condition = null)
        => SpinWait.SpinUntil(condition ?? (() => false), timeout * 3600000);

    public static bool IsMatch(this string regString, string targetString)
        => new Regex(regString, RegexOptions.Multiline).IsMatch(targetString);

    //public static string ProcessYMCode(this string input)
    //{
    //    var result = input;
    //    Replace($"\\{KH_LEFT}play-(?<str>[0-9]+?){KH_RIGHT}", "[CQ:ym_play,qq=${str}]");

    //    var playRegex = new Regex($"\\{KH_LEFT}play-(?<str>.+?)\\{KH_RIGHT}");
    //    var resZap = playRegex.Matches(input);
    //    //var isPlay = Replace($"\\{KH_LEFT}-na{KH_RIGHT}", string.Empty);
    //    //"\[play-(?<str>.+?)\]"

    //    return result;

    //    bool Replace(string pattern, string replacement)
    //    {
    //        var newResult = Regex.Replace(result, pattern, replacement);
    //        var isMatch = newResult != result;
    //        result = newResult;
    //        return isMatch;
    //    }
    //}

    public static string GetPath(string directoryName, string fileName, GetPathType pathType)
    {
        var appPath = Environment.CurrentDirectory;
        switch (pathType)
        {
            case GetPathType.Normal:
                return Path.Combine(appPath, directoryName, fileName);
            case GetPathType.CQCodePath:
                return $"{new Uri(appPath).AbsoluteUri}{(directoryName.IsNullOrEmpty() ? string.Empty : $"/{directoryName}")}/{fileName}";
            default:
                throw new ArgumentOutOfRangeException(nameof(pathType), pathType, null);
        }
    }

    public static void CreatePath(string pathName)
    {
        if (!Directory.Exists(pathName))
            Directory.CreateDirectory(pathName);
    }

    /// <summary>
    /// 删除过期缓存文件
    /// </summary>
    /// <param name="expiredDays">过期日期</param>
    public static void DeleteExpiredCache(int expiredDays = 7)
    {
        const string folderPath = "Cache";
        var dateNow = DateTime.Now;
        Directory.GetFiles(folderPath)
            .ForEach(file =>
            {
                var fileInfo = new FileInfo(file);
                if ((dateNow - fileInfo.CreationTime).TotalDays >= expiredDays)
                    fileInfo.Delete();
            });
    }
}