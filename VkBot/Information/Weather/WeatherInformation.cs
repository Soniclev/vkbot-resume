using System;
using System.Drawing;

namespace VkBot.Information.Weather
{
    public class WeatherInformation
    {
        public double Temperature { get; set; }
        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set; }

        public string Description { get; set; }
        public Image Image { get; set; }

        public double WindSpeed { get; set; }
        public double WindDegree { get; set; }

        public double Cloudiness { get; set; }

        public DateTime SunRise { get; set; }
        public DateTime SunSet { get; set; }
    }
}
