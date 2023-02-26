using NAudio.Wave;

namespace Yamei.Common
{
    public static class YameiExtensions
    {
        public static readonly DateTime StartTime = new DateTime(1970, 1, 1).Add(TimeZoneInfo.Local.BaseUtcOffset);

        public static DateTime ToDateTime(this long timeStamp)
            => StartTime.AddSeconds(timeStamp);

        public static DateTime ToDateTime(this int timeStamp)
            => StartTime.AddSeconds(timeStamp);

        /// <summary>
        /// 获取给定日期是本月的第几天和倒数第几天
        /// </summary>
        public static (int DayOfMonth, int LastDayOfMonth) GetDayOfMonthAndLastDayOfMonth(DateTime date)
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
    }
}