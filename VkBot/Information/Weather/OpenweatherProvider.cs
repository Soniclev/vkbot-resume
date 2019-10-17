using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkBot.Core.Rest;

namespace VkBot.Information.Weather
{
    public class OpenweatherProvider : IWeatherProvider
    {
        private const long CityId = 620127;
        private string _apiKey = null;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Image> _imagesCache = new Dictionary<string, Image>();

        public OpenweatherProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private string GetApiKey()
        {
            if (_apiKey == null)
            {
                _apiKey = File.ReadAllText(@"Data/Tokens/OpenweatherToken.txt");
            }
            return _apiKey;
        }

        private string CreateRequestUrl()
        {
            return $"https://api.openweathermap.org/data/2.5/weather?id={CityId}&appid={GetApiKey()}&lang=ru&units=metric";
        }

        public WeatherInformation GetWeather()
        {
            var client = _serviceProvider.GetService<IRestClient>();
            var response = client.GetString(CreateRequestUrl());
            var deserialized = JsonConvert.DeserializeObject<JObject>(response);

            var url = $"http://openweathermap.org/img/w/{deserialized["weather"][0]["icon"].Value<string>()}.png";
            if (!_imagesCache.ContainsKey(url))
            {
                using (var imageStream = client.GetStream(url))
                {
                    _imagesCache[url] = Image.FromStream(imageStream);
                }
            }

            var weatherInformation = new WeatherInformation
            {
                Description = deserialized["weather"][0]["description"].Value<string>(),
                Cloudiness = deserialized["clouds"]["all"].Value<double>(),
                Image = (Image)_imagesCache[url].Clone(),
                MaxTemperature = deserialized["main"]["temp_max"].Value<double>(),
                MinTemperature = deserialized["main"]["temp_min"].Value<double>(),
                SunRise = UnixTimeStampToDateTime(deserialized["sys"]["sunrise"].Value<long>()),
                SunSet = UnixTimeStampToDateTime(deserialized["sys"]["sunset"].Value<long>()),
                Temperature = deserialized["main"]["temp"].Value<double>(),
                WindDegree = deserialized["wind"]["deg"]?.Value<double>() ?? 0,
                WindSpeed = deserialized["wind"]["speed"].Value<double>()
            };
            return weatherInformation;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
}
}
