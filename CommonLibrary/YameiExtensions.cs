using System;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Yamei.Common;

public static class YameiExtensions
{
    /// <summary>
    /// 转换为带符号的字符串
    /// </summary>
    public static string ToSignString(this int number)
        => number >= 0 ? $"+{number}" : $"{number}";

    /// <summary>
    /// 转换为带符号的字符串(long版)
    /// </summary>
    public static string ToSignString(this long number)
        => number >= 0 ? $"+{number}" : $"{number}";

    public static readonly DateTime StartTime = new DateTime(1970, 1, 1).Add(TimeZoneInfo.Local.BaseUtcOffset);

    public static DateTime ToDateTime(this long timeStamp)
        => StartTime.AddSeconds(timeStamp);

    public static DateTime ToDateTime(this int timeStamp)
        => StartTime.AddSeconds(timeStamp);

    public static long ToTimeStamp(this DateTime dateTime)
        => (long)(dateTime - StartTime).TotalSeconds;

    public static long AddSeconds(this long timeStamp, long addValue)
        => timeStamp + addValue;

    public static long AddMinutes(this long timeStamp, long addValue)
        => timeStamp + addValue * 60;

    public static long AddHours(this long timeStamp, long addValue)
        => timeStamp + addValue * 3600;

    public static long AddDays(this long timeStamp, long addValue)
        => timeStamp + addValue * 86400;

    public static long ToLong(this bool boolValue) => boolValue ? 1 : 0;

    public static int ToInt(this bool boolValue) => boolValue ? 1 : 0;

    public static bool ToBool(this long value) => value != 0;

    /// <summary>
    /// 获取给定日期是本月的第几天和倒数第几天
    /// </summary>
    public static (int DayOfMonth, int LastDayOfMonth) GetDayOfMonthAndLastDayOfMonth(this DateTime date)
    {
        var dayOfMonth = date.Day;
        var lastDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month) - dayOfMonth + 1;
        return (dayOfMonth, lastDayOfMonth);
    }

    /// <summary>
    /// 播放声音
    /// </summary>
    /// <param name="filePath">音频文件路径</param>
    /// <param name="delay">延迟</param>
    public static void PlaySe3(string filePath, int delay = 800)
        => PlaySe(filePath, 3, delay);

    /// <summary>
    /// 播放声音
    /// </summary>
    /// <param name="filePath">音频文件路径</param>
    /// <param name="playTimes">播放次数</param>
    /// <param name="delay">延迟</param>
    public static void PlaySe(string filePath, int playTimes = 1, int delay = 500)
    {
        if (playTimes <= 0)
            throw new ArgumentOutOfRangeException(nameof(playTimes));

        //if (!File.Exists(filePath))
        //    filePath = System.Environment.CurrentDirectory

        using var waveOut = new WaveOutEvent();
        using var wavReader = new WaveFileReader(filePath);

        waveOut.Init(wavReader);
        playTimes.Times(i =>
        {
            waveOut.Play();
            if (i < playTimes - 1)
                SpinWait.SpinUntil(() => false, delay);
        });
    }

    /// <summary>
    /// 尝试N次取值
    /// </summary>
    /// <param name="maxTimes">最大尝试次数</param>
    /// <param name="getFunc">取值函数</param>
    /// <returns>是否取值成功</returns>
    public static async Task<bool> TryTimesAsync(this int maxTimes, Func<Task<bool>> getFunc)
    {
        var times = 1;
        var getResultSuccess = false;
        while (times <= maxTimes && !getResultSuccess)
        {
            getResultSuccess = await getFunc();
            times++;
        }

        return getResultSuccess;
    }
}