using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using MessagePack;
using Yamei.Common;

namespace System;

public static class CommonExtensions
{
    public static readonly JsonSerializerOptions JsonOption = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private const string KH_LEFT = @"&#91;";
    private const string KH_RIGHT = @"&#93;";

    //[Obsolete("为了适配所有对象深拷贝, 使用BinaryFormatter")]
    //public static T Clone<T>(T realObject)
    //{
    //    var objectStream = new MemoryStream();
    //    //利用 System.Runtime.Serialization序列化与反序列化完成引用对象的复制
    //    var formatter = new BinaryFormatter();
    //    formatter.Serialize(objectStream, realObject);
    //    objectStream.Seek(0, SeekOrigin.Begin);
    //    return (T)formatter.Deserialize(objectStream);
    //}

    /// <summary>
    /// 基于MessagePackObject的深拷贝
    /// </summary>
    /// <param name="obj">对象</param>
    /// <typeparam name="T">对象类型</typeparam>
    /// <returns>深拷贝后的对象</returns>
    public static T Clone<T>(T obj)
    {
        var raw = MessagePackSerializer.Serialize(obj);
        return MessagePackSerializer.Deserialize<T>(raw);
    }

    public static string ToConditionString(this DateTime input, Dictionary<string, bool> holidayInfo)
        => input.ToString($"yyyy-MM-dd-{DayOfWeek2Int(input.DayOfWeek)}-{(input.IsHoliday(holidayInfo) ? 1 : 0)} HH:mm:ss");

    public static string ToDayHHMM(this DateTime input) => input.ToString($"d{(input.DayOfWeek == DayOfWeek.Saturday ? "(六)" : string.Empty)}, HH:mm");

    public static string ToYYYYMMDDHHMMSS(this DateTime input) => input.ToString("yyyy-M-dd HH:mm:ss");

    public static string ToYYYYMMDDHHMM(this DateTime input) => input.ToString("yyyy-M-dd HH:mm");

    public static string ToYYYYMDD(this DateTime input) => input.ToString("yyyy-M-dd");

    public static string ToYYYYMMDD(this DateTime input) => input.ToString("yyyy-MM-dd");

    /// <summary>
    /// <see cref="DayOfWeek"/>转换为数字
    /// </summary>
    public static int DayOfWeek2Int(DayOfWeek dayOfWeek)
        => dayOfWeek == DayOfWeek.Sunday ? 7 : (int)dayOfWeek;

    /// <summary>
    /// 判定是否节假日
    /// </summary>
    public static bool IsHoliday(this DateTime dateTime, Dictionary<string, bool> holidayInfo)
    {
        switch (dateTime.DayOfWeek)
        {
            case DayOfWeek.Monday:
            case DayOfWeek.Tuesday:
            case DayOfWeek.Wednesday:
            case DayOfWeek.Thursday:
            case DayOfWeek.Friday:
                return holidayInfo != null
                       && holidayInfo.TryGetValue(dateTime.ToYYYYMMDD(), out var isHoliday1)
                       && isHoliday1;
            case DayOfWeek.Saturday:
            case DayOfWeek.Sunday:
                return holidayInfo == null
                       || !holidayInfo.TryGetValue(dateTime.ToYYYYMMDD(), out var isHoliday2)
                       || isHoliday2;
            default:
                return false;
        }
    }

    /// <summary>
    /// 替换普通内容为CQ代码
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <param name="senderId">发送者Id</param>
    /// <returns>替换结果</returns>
    public static (string Result, bool IsNoAt, bool IsNoReply) ToCqCode(this string input, long senderId = 0)
    {
        var result = input;
        var isNoAt = Replace($"\\{KH_LEFT}-na{KH_RIGHT}", string.Empty);
        var isNoReply = Replace($"\\{KH_LEFT}-nr{KH_RIGHT}", string.Empty);
        Replace($"\\{KH_LEFT}at-self{KH_RIGHT}", $"[CQ:at,qq={senderId}]");
        Replace($"\\{KH_LEFT}at-(?<str>[0-9]+?){KH_RIGHT}", "[CQ:at,qq=${str}]");
        Replace($"\\{KH_LEFT}image-(?<str>.+?){KH_RIGHT}", "[CQ:image,File=${str}]");
        Replace($"\\{KH_LEFT}play-(?<str>.+?){KH_RIGHT}", "[CQ:ym_play,File=${str}]");
        Replace($"\\{KH_LEFT}play3-(?<str>.+?){KH_RIGHT}", "[CQ:ym_play3,File=${str}]");
        return (result, isNoAt, isNoReply);

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
        Replace($"\\[CQ:at,qq=(?<str>[0-9]+?)]", "[at-${str}]");
        Replace($"\\[CQ:image,File=(?<str>.++?)]", "[image-${str}]");
        return result;

        void Replace(string pattern, string replacement)
            => result = Regex.Replace(result, pattern, replacement);
    }

    public static void Sleep(int timeout, Func<bool> condition = null)
        => SpinWait.SpinUntil(condition ?? (() => false), timeout);

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

    public static string GetPath(string directoryName, string fileName)
    {
        var appPath = Environment.CurrentDirectory;
        return $"file:///{appPath.Replace(@"\", "/")}/{directoryName}/{fileName}";
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