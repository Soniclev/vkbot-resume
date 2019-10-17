using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Information.Weather;

namespace VkBot.Group.Cover.GroupCoverDrawable
{
    class CurrentWeather : IGroupCoverDrawable
    {
        private WeatherInformation _weather;
        private readonly IServiceProvider _serviceProvider;

        public CurrentWeather(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Draw(IGroupCoverRenderer groupCover)
        {
            groupCover.DrawWeatherIcon(_weather.Image);
            _weather.Image.Dispose();
            groupCover.DrawTemperature(_weather.Temperature);
            groupCover.DrawWindSpeed(_weather.WindSpeed);
            groupCover.DrawWindArrow(_weather.WindDegree);
            groupCover.DrawSunImage();
            groupCover.DrawSunStatus(_weather.SunSet, _weather.SunRise);
            groupCover.DrawCloudnessImage();
            groupCover.DrawCloudnessStatus(_weather.Cloudiness);
        }

        public Task Precache()
        {
            return Task.Run(() => _weather = _serviceProvider.GetService<IWeatherProvider>().GetWeather());
        }

        public async Task DrawAsync(IGroupCoverRenderer groupCover)
        {
            await Task.Run(() => Draw(groupCover)).ConfigureAwait(false);
        }
    }
}
