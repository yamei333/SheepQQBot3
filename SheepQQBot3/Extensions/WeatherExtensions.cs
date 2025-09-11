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
        public const string TEMPERATURE_UNITS = "Celsius";

        public static async Task<(AIWeatherInfo, AIWeatherInfo, AIWeatherInfo)> AIGetWeatherInfoAsync(string cityName = "shanghai")
        {
            var weatherData = await OpenWeatherMapService.GetWeatherDataByCityNameAsync(cityName).ConfigureAwait(false);

            var dateNow = DateTime.Now;
            var forecastData = await OpenWeatherMapService.GetForecastDataByCityNameAsync(cityName).ConfigureAwait(false);
            var forecastWeatherDatas = forecastData.WeatherData;
            var nextWeatherData = forecastWeatherDatas.First(each => each.AcquisitionDateTime > dateNow);
            var prevWeatherData = forecastWeatherDatas.Last(each => each.AcquisitionDateTime < dateNow);

            return (weatherData.ToAIWeatherInfo(false), prevWeatherData.ToAIWeatherInfo(true), nextWeatherData.ToAIWeatherInfo(true));
        }

        private static AIWeatherInfo ToAIWeatherInfo(this WeatherData weatherData, bool setForecastDate)
        {
            var weatherDayInfo = weatherData.WeatherDayInfo;
            return new AIWeatherInfo
            {
                ForecastDate = setForecastDate ? weatherData.AcquisitionDateTime.ToYYYYMDHHMMSS() : string.Empty,
                CurrentWeather = string.Join(',', weatherData.Weathers.Select(each => each.Description)),
                TempAvg = $"{weatherDayInfo.Temperature:0.0} {TEMPERATURE_UNITS}",
                TempFeelLike = $"{weatherDayInfo.FeelsLike:0.0} {TEMPERATURE_UNITS}",
                Precipitation = $"{weatherData.Precipitation:0.0} mm/h",
                WindSpeed = $"{weatherData.Wind.Speed} meter/sec",
                Cloudiness = $"{weatherData.Clouds.All}%",
            };
        }
    }
}