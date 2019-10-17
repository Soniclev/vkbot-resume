using System;
using System.Collections.Generic;
using System.Text;

namespace VkBot.Information.Horoscope
{
    internal interface IHoroscopeProvider
    {
        ZodiacSign GetZodiacSignByBirthDay(DateTime birthday);
        string GetEroticHoroscope(ZodiacSign zodiacSign);
        string GetEroticHoroscope(DateTime birthday);
        string GetLoveHoroscope(ZodiacSign zodiacSign);
        string GetLoveHoroscope(DateTime birthday);
        string GetBusinessHoroscope(ZodiacSign zodiacSign);
        string GetBusinessHoroscope(DateTime birthday);
    }
}
