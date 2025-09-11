using System;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    /// <summary>
    /// 小助手状态(保存用)
    /// </summary>
    public class AIStatusData
    {
        /// <summary>
        /// 心情指数
        /// </summary>
        [JsonPropertyName("mood")]
        public int MoodIndexValue { get; set; }
    }

    /// <summary>
    /// 小助手状态(请求用)
    /// </summary>
    public class AIStatusInfo
    {
        /// <summary>
        /// 心情状态
        /// </summary>
        [JsonPropertyName("mood")]
        public string Mood { get; set; }

        /// <summary>
        /// 当前日程
        /// </summary>
        [JsonPropertyName("schedule")]
        public string Schedule { get; set; }

        /// <summary>
        /// 当前聊天场景
        /// </summary>
        [JsonPropertyName("scene")]
        public string Scene { get; set; }

        /// <summary>
        /// 当天天气
        /// </summary>
        [JsonPropertyName("weatherInfo")]
        public AIWeatherInfo WeatherInfo { get; set; }

        /// <summary>
        /// 上个时间点天气预报
        /// </summary>
        [JsonPropertyName("prevWeatherInfo")]
        public AIWeatherInfo PrevWeatherInfo { get; set; }

        /// <summary>
        /// 下个时间点天气预报
        /// </summary>
        [JsonPropertyName("nextWeatherInfo")]
        public AIWeatherInfo NextWeatherInfo { get; set; }

        /// <summary>
        /// 当前时间
        /// </summary>
        [JsonPropertyName("nowDate")]
        public string NowDate { get; set; }
    }

    public class AIWeatherInfo
    {
        /// <summary>
        /// 天气预报时间(预报时使用)
        /// </summary>
        [JsonPropertyName("forecastDate")]
        public string ForecastDate { get; set; }

        /// <summary>
        /// 当前天气
        /// </summary>
        [JsonPropertyName("currentWeather")]
        public string CurrentWeather { get; set; }

        /// <summary>
        /// 平均温度
        /// </summary>
        [JsonPropertyName("temp")]
        public string TempAvg { get; set; }

        /// <summary>
        /// 体感温度
        /// </summary>
        [JsonPropertyName("tempFeelLike")]
        public string TempFeelLike { get; set; }

        /// <summary>
        /// 风速
        /// </summary>
        [JsonPropertyName("windSpeed")]
        public string WindSpeed { get; set; }

        /// <summary>
        /// 降雨量(mm/h)
        /// </summary>
        [JsonPropertyName("precipitation")]
        public string Precipitation { get; set; }

        /// <summary>
        /// 浑浊度
        /// </summary>
        [JsonPropertyName("cloudiness")]
        public string Cloudiness { get; set; }
    }

    public static class AIStatusUtil
    {
        public static string ToMood(this int moodIndexValue)
        {
            return moodIndexValue switch
            {
                < -130 => "心情最差",
                < -100 => "心情极差",
                < -70 => "心情非常差",
                < -40 => "心情比较差",
                < -25 => "心情有点差",
                < -10 => "心情略微差",
                <= 10 => "心情一般",
                <= 25 => "心情略微好",
                <= 40 => "心情有点好",
                <= 70 => "心情比较好",
                <= 100 => "心情非常好",
                <= 130 => "心情极好",
                _ => "心情最好",
            };
        }

        public static string GetSchedule()
        {
            var dateNow = DateTime.Now;
            var dayOfWeek = dateNow.DayOfWeek;
            var timeSeconds = (int)dateNow.TimeOfDay.TotalSeconds;
            return dayOfWeek switch
            {
                DayOfWeek.Monday or DayOfWeek.Tuesday or DayOfWeek.Wednesday or DayOfWeek.Thursday or DayOfWeek.Friday => timeSeconds switch
                {
                    _ when timeSeconds <= GetTime(00, 30) => "bed time",
                    _ when timeSeconds <= GetTime(07, 30) => "deep sleep time",
                    _ when timeSeconds <= GetTime(08, 00) => "bed time",
                    _ when timeSeconds <= GetTime(08, 15) => "get up",
                    _ when timeSeconds <= GetTime(08, 30) => "wash up in the morning",
                    _ when timeSeconds <= GetTime(09, 00) => "breakfast",
                    _ when timeSeconds <= GetTime(11, 00) => "小助手 time",
                    _ when timeSeconds <= GetTime(11, 30) => "lunch",
                    _ when timeSeconds <= GetTime(12, 30) => "nap",
                    _ when timeSeconds <= GetTime(13, 00) => "sleep inertia",
                    _ when timeSeconds <= GetTime(17, 00) => "小助手 time",
                    _ when timeSeconds <= GetTime(17, 30) => "dinner",
                    _ when timeSeconds <= GetTime(18, 30) => "short rest",
                    _ when timeSeconds <= GetTime(19, 15) => "sports",
                    _ when timeSeconds <= GetTime(19, 30) => "a post-workout break",
                    _ when timeSeconds <= GetTime(20, 00) => "bath",
                    _ when timeSeconds <= GetTime(21, 00) => "study time",
                    _ when timeSeconds <= GetTime(22, 00) => "relaxation time",
                    _ when timeSeconds <= GetTime(23, 00) => "personal time",
                    _ => "bed time",
                },
                DayOfWeek.Saturday => timeSeconds switch
                {
                    _ when timeSeconds <= GetTime(00, 30) => "bed time",
                    _ when timeSeconds <= GetTime(07, 30) => "deep sleep time",
                    _ when timeSeconds <= GetTime(08, 00) => "bed time",
                    _ when timeSeconds <= GetTime(08, 15) => "get up",
                    _ when timeSeconds <= GetTime(08, 30) => "wash up in the morning",
                    _ when timeSeconds <= GetTime(09, 00) => "breakfast",
                    _ when timeSeconds <= GetTime(10, 00) => "sunbathe",
                    _ when timeSeconds <= GetTime(10, 30) => "hair care",
                    _ when timeSeconds <= GetTime(11, 00) => "short rest",
                    _ when timeSeconds <= GetTime(11, 30) => "lunch",
                    _ when timeSeconds <= GetTime(12, 00) => "nap",
                    _ when timeSeconds <= GetTime(16, 30) => "spending time with 雅美",
                    _ when timeSeconds <= GetTime(17, 00) => "time for 雅美 to clean my ears",
                    _ when timeSeconds <= GetTime(17, 30) => "dinner",
                    _ when timeSeconds <= GetTime(18, 30) => "short rest",
                    _ when timeSeconds <= GetTime(19, 30) => "time for 雅美 to help with massage",
                    _ when timeSeconds <= GetTime(20, 00) => "bath",
                    _ when timeSeconds <= GetTime(21, 00) => "masturbation time",
                    _ when timeSeconds <= GetTime(21, 30) => "enjoying the sunset with 雅美",
                    _ when timeSeconds <= GetTime(22, 00) => "acting cute with 雅美",
                    _ when timeSeconds <= GetTime(22, 30) => "share the week's fun moments with 雅美",
                    _ when timeSeconds <= GetTime(23, 00) => "enjoy quiet moments with 雅美",
                    _ => "sleeping with 雅美",
                },
                DayOfWeek.Sunday => timeSeconds switch
                {
                    _ when timeSeconds <= GetTime(00, 30) => "sleeping with 雅美",
                    _ when timeSeconds <= GetTime(07, 30) => "deep sleep with 雅美",
                    _ when timeSeconds <= GetTime(08, 00) => "sleeping with 雅美",
                    _ when timeSeconds <= GetTime(08, 15) => "get up",
                    _ when timeSeconds <= GetTime(08, 30) => "wash up in the morning",
                    _ when timeSeconds <= GetTime(09, 00) => "breakfast",
                    _ when timeSeconds <= GetTime(10, 00) => "sunbathe",
                    _ when timeSeconds <= GetTime(11, 00) => "strolling",
                    _ when timeSeconds <= GetTime(11, 30) => "lunch",
                    _ when timeSeconds <= GetTime(12, 30) => "nap",
                    _ when timeSeconds <= GetTime(13, 00) => "sleep inertia",
                    _ when timeSeconds <= GetTime(15, 00) => "shopping out",
                    _ when timeSeconds <= GetTime(17, 00) => "do housework",
                    _ when timeSeconds <= GetTime(17, 30) => "dinner",
                    _ when timeSeconds <= GetTime(18, 30) => "scrolling through TikTok",
                    _ when timeSeconds <= GetTime(19, 00) => "reflecting on the past week",
                    _ when timeSeconds <= GetTime(19, 30) => "organizing my thoughts for next week",
                    _ when timeSeconds <= GetTime(20, 00) => "bath",
                    _ when timeSeconds <= GetTime(21, 00) => "masturbation time",
                    _ when timeSeconds <= GetTime(22, 00) => "relaxation time",
                    _ when timeSeconds <= GetTime(23, 00) => "personal time",
                    _ => "bed time",
                },
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        private static int GetTime(int hours, int minutes) => (int)new TimeSpan(hours, minutes, 0).TotalSeconds;
    }
}