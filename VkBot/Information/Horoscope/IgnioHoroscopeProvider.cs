using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Core.Rest;

namespace VkBot.Information.Horoscope
{
    internal class IgnioHoroscopeProvider : IHoroscopeProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _updateInterval = new TimeSpan(4, 0, 0);
        private DateTime _lastUpDateTime = DateTime.MinValue;
        private readonly List<DailyHoroscope> _dailyHoroscopes = new List<DailyHoroscope>();

        public IgnioHoroscopeProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public string GetEroticHoroscope(ZodiacSign zodiacSign)
        {
            var horoscope = GetDailyHoroscope(zodiacSign);
            return horoscope.Erotic;
        }

        private DailyHoroscope GetDailyHoroscope(ZodiacSign zodiacSign)
        {
            if (IsUpdatedNeeded())
                UpdateHoroscope();
            var horoscope = _dailyHoroscopes.Find(x => x.ZodiacSign == zodiacSign);
            return horoscope;
        }

        private void UpdateHoroscope()
        {
            void AddHoroscope(ZodiacSign zodiacSign, XmlDocument eroticDocument, XmlDocument loveDocument, XmlDocument businessDocument)
            {
                var name = zodiacSign.ToString().ToLower();
                var erotic = eroticDocument.GetElementsByTagName(name).Item(0).SelectSingleNode("child::today").ChildNodes.Item(0).Value.Trim();
                var love = loveDocument.GetElementsByTagName(name).Item(0).SelectSingleNode("child::today").ChildNodes.Item(0).Value.Trim();
                var business = businessDocument.GetElementsByTagName(name).Item(0).SelectSingleNode("child::today").ChildNodes.Item(0).Value.Trim();
                _dailyHoroscopes.Add(new DailyHoroscope(zodiacSign, love, erotic, business));
            }

            const string eroticHoroscopeUrl = @"http://ignio.com/r/export/utf/xml/daily/ero.xml";
            const string loveHoroscopeUrl = @"http://ignio.com/r/export/utf/xml/daily/lov.xml";
            const string businessHoroscopeUrl = @"http://ignio.com/r/export/utf/xml/daily/bus.xml";
            var restClient = _serviceProvider.GetService<IRestClient>();
            var eroticHoroscope = restClient.GetString(eroticHoroscopeUrl);
            var loveHoroscope = restClient.GetString(loveHoroscopeUrl);
            var businessHoroscope = restClient.GetString(businessHoroscopeUrl);

            var eroticXmlDocument = new XmlDocument();
            eroticXmlDocument.LoadXml(eroticHoroscope);
            var loveXmlDocument = new XmlDocument();
            loveXmlDocument.LoadXml(loveHoroscope);
            var businessXmlDocument = new XmlDocument();
            businessXmlDocument.LoadXml(businessHoroscope);

            _dailyHoroscopes.Clear();
            AddHoroscope(ZodiacSign.Aries, eroticXmlDocument, loveXmlDocument, businessXmlDocument);
            AddHoroscope(ZodiacSign.Taurus, eroticXmlDocument, loveXmlDocument, businessXmlDocument);
            AddHoroscope(ZodiacSign.Gemini, eroticXmlDocument, loveXmlDocument, businessXmlDocument);
            AddHoroscope(ZodiacSign.Cancer, eroticXmlDocument, loveXmlDocument, businessXmlDocument);
            AddHoroscope(ZodiacSign.Leo, eroticXmlDocument, loveXmlDocument, businessXmlDocument);
            AddHoroscope(ZodiacSign.Virgo, eroticXmlDocument, loveXmlDocument, businessXmlDocument);
            AddHoroscope(ZodiacSign.Libra, eroticXmlDocument, loveXmlDocument, businessXmlDocument);
            AddHoroscope(ZodiacSign.Scorpio, eroticXmlDocument, loveXmlDocument, businessXmlDocument);
            AddHoroscope(ZodiacSign.Sagittarius, eroticXmlDocument, loveXmlDocument, businessXmlDocument);
            AddHoroscope(ZodiacSign.Capricorn, eroticXmlDocument, loveXmlDocument, businessXmlDocument);
            AddHoroscope(ZodiacSign.Aquarius, eroticXmlDocument, loveXmlDocument, businessXmlDocument);
            AddHoroscope(ZodiacSign.Pisces, eroticXmlDocument, loveXmlDocument, businessXmlDocument);

            _lastUpDateTime = DateTime.Now;
        }

        private bool IsUpdatedNeeded()
        {
            return _lastUpDateTime + _updateInterval < DateTime.Now;
        }

        public string GetEroticHoroscope(DateTime birthday)
        {
            return GetEroticHoroscope(GetZodiacSignByBirthDay(birthday));
        }

        public string GetLoveHoroscope(ZodiacSign zodiacSign)
        {
            var horoscope = GetDailyHoroscope(zodiacSign);
            return horoscope.Love;
        }

        public string GetLoveHoroscope(DateTime birthday)
        {
            return GetLoveHoroscope(GetZodiacSignByBirthDay(birthday));
        }

        private bool IsHoroscopeDateTimeInRange(DateTime birthday, int month1, int day1, int month2, int day2)
        {
            var t1 = new DateTime(birthday.Year, month1, day1);
            var t2 = new DateTime(birthday.Year, month2, day2);
            return t1.Ticks <= birthday.Ticks && birthday.Ticks <= t2.Ticks;
        }

        public ZodiacSign GetZodiacSignByBirthDay(DateTime birthday)
        {
            if (IsHoroscopeDateTimeInRange(birthday, 3, 21, 4, 20))
                return ZodiacSign.Aries;
            if (IsHoroscopeDateTimeInRange(birthday, 4, 21, 5, 21))
                return ZodiacSign.Taurus;
            if (IsHoroscopeDateTimeInRange(birthday, 5, 22, 6, 21))
                return ZodiacSign.Gemini;
            if (IsHoroscopeDateTimeInRange(birthday, 6, 22, 7, 22))
                return ZodiacSign.Cancer;
            if (IsHoroscopeDateTimeInRange(birthday, 7, 23, 8, 21))
                return ZodiacSign.Leo;
            if (IsHoroscopeDateTimeInRange(birthday, 8, 22, 9, 23))
                return ZodiacSign.Virgo;
            if (IsHoroscopeDateTimeInRange(birthday, 9, 24, 10, 23))
                return ZodiacSign.Libra;
            if (IsHoroscopeDateTimeInRange(birthday, 10, 24, 11, 22))
                return ZodiacSign.Scorpio;
            if (IsHoroscopeDateTimeInRange(birthday, 1, 21, 2, 19))
                return ZodiacSign.Aquarius;
            if (IsHoroscopeDateTimeInRange(birthday, 2, 20, 3, 20))
                return ZodiacSign.Pisces;
            return ZodiacSign.Capricorn;
        }

        public string GetBusinessHoroscope(ZodiacSign zodiacSign)
        {
            var horoscope = GetDailyHoroscope(zodiacSign);
            return horoscope.Business;
        }

        public string GetBusinessHoroscope(DateTime birthday)
        {
            return GetBusinessHoroscope(GetZodiacSignByBirthDay(birthday));
        }
    }
}
