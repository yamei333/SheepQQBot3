using Masuit.Tools;
using OpenWeatherMap.Standard.Models;
using SheepQQBot3.Model.AI;
using System;
using System.Linq;
using System.Threading.Tasks;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.Extensions
{
    public static class WeatherExtensions
    {
        public const double LON = 121.4581;
        public const double LAT = 31.2222;
        //public const string TEMPERATURE_UNITS = "Celsius";

        public static async Task<AIWeatherContext> AIGetWeatherDataAsync(string cityName = "shanghai")
        {
            var weatherData = await OpenWeatherMapService.GetWeatherDataByCityNameAsync(cityName).ConfigureAwait(false);

            var dateNow = DateTime.Now;
            var forecastData = await OpenWeatherMapService.GetForecastDataByCityNameAsync(cityName).ConfigureAwait(false);
            var forecastWeatherDatas = forecastData.WeatherData;
            var nextWeatherData = forecastWeatherDatas.First(each => each.AcquisitionDateTime > dateNow);
            var prevWeatherData = forecastWeatherDatas.Last(each => each.AcquisitionDateTime < dateNow);

            return CreateWeatherContext(weatherData, prevWeatherData, nextWeatherData);
        }

        //private static AIWeatherData ToAIWeatherData(this WeatherData weatherData, bool setForecastDate)
        //{
        //    var weatherDayInfo = weatherData.WeatherDayInfo;
        //    return new AIWeatherData
        //    {
        //        ForecastDate = setForecastDate ? weatherData.AcquisitionDateTime.ToYYYYMDHHMMSS() : string.Empty,
        //        Weather = string.Join(',', weatherData.Weathers.Select(each => each.Description)),
        //        TempAvg = $"{weatherDayInfo.Temperature:0.0} {TEMPERATURE_UNITS}",
        //        FeelLike = $"{weatherDayInfo.FeelsLike:0.0} {TEMPERATURE_UNITS}",
        //        Precipitation = $"{weatherData.Precipitation:0.0} mm/h",
        //        WindSpeed = $"{weatherData.Wind.Speed} meter/sec",
        //        Cloudiness = $"{weatherData.Clouds.All}%",
        //    };
        //}

        public static AIWeatherContext CreateWeatherContext(WeatherData current, WeatherData prev, WeatherData next)
        {
            var context = new AIWeatherContext();

            var temp = current.WeatherDayInfo.Temperature;
            var feel = current.WeatherDayInfo.FeelsLike;

            var currentWeather = GetWeatherString(current);
            var prevWeather = GetWeatherString(prev);
            var nextWeather = GetWeatherString(next);
            var tempStr = Math.Abs(temp - feel) > 3
                ? $"{temp:0.0}°C (Feels like {feel:0.0}°C)"
                : $"{temp:0.0}°C";
            context.CurrentCondition = $"{currentWeather}, {tempStr}";

            // 如果风特别大，追加一句描述，因为猫怕风
            if (current.Wind.Speed > 10)
                context.CurrentCondition += ", Strong Wind!";

            // --- 处理预报 (Forecast) ---
            // 只有当未来天气和现在不同时，才值得说
            // 如果没变化，就不要传 forecast 字段了，省钱
            context.ForecastSummary = nextWeather != currentWeather
                ? $"Will turn {nextWeather} later."
                : null;

            // --- 处理突发事件 (Event) - 最重要的一步 ---
            // 这就是你一直想实现的 "刚下大雨了" 的逻辑
            context.RecentChangeEvent = string.Empty;
            // 雨停了
            if (prevWeather.Contains("Rain") && !currentWeather.Contains("Rain"))
                context.RecentChangeEvent += "The rain has just stopped. ";
            // 突然下雨
            if (!prevWeather.Contains("Rain") && currentWeather.Contains("Rain"))
                context.RecentChangeEvent += "It suddenly started raining! ";
            // 骤降温 (温差超过 5 度)
            if (prev.WeatherDayInfo.Temperature - temp > 5)
                context.RecentChangeEvent += "Temperature dropped significantly recently. ";

            if (context.RecentChangeEvent.IsNullOrEmpty())
                context.RecentChangeEvent = null;

            return context;
        }

        private static string GetWeatherString(WeatherData weather) => string.Join(',', weather.Weathers.Select(each => each.Description));
    }
}