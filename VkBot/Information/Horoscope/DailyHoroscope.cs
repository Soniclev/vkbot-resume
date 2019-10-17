using System;
using System.Collections.Generic;
using System.Text;

namespace VkBot.Information.Horoscope
{
    class DailyHoroscope
    {
        public string Erotic { get; set; }
        public string Love { get; set; }
        public string Business { get; set; }
        public ZodiacSign ZodiacSign { get; set; }

        public DailyHoroscope()
        {
        }

        public DailyHoroscope(ZodiacSign zodiacSign, string love = null, string erotic = null, string business = null)
        {
            ZodiacSign = zodiacSign;
            Love = love;
            Erotic = erotic;
            Business = business;
        }
    }
}
