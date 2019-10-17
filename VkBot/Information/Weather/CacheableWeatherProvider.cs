using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace VkBot.Information.Weather
{
    internal class CacheableWeatherProvider : IWeatherProvider
    {
        private readonly IWeatherProvider _baseWeatherProvider;
        private WeatherInformation _weatherInformation;
        private readonly ILogger<CacheableWeatherProvider> _logger;

        public CacheableWeatherProvider(IWeatherProvider baseWeatherProvider)
        {
            _baseWeatherProvider = baseWeatherProvider;
        }

        public WeatherInformation GetWeather()
        {
            try
            {
                var weatherInformation = _baseWeatherProvider.GetWeather();
                _weatherInformation = weatherInformation;
                return weatherInformation;
            }
            catch (Exception)
            {
                if (_weatherInformation != null)
                    return _weatherInformation;
                else
                {
                    throw;
                }
            }
        }
    }
}
