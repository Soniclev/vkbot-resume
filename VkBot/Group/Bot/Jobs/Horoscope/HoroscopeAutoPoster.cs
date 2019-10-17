using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Core;
using VkBot.Core.Timer;
using VkBot.Information.Horoscope;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace VkBot.Group.Bot.Jobs.Horoscope
{
    class HoroscopeAutoPoster : IHoroscopeAutoPoster
    {
        private readonly IServiceProvider _serviceProvider;
        private TimeSpan _autoPostTime = new TimeSpan(7, 0, 0);
        private TimeSpan _autopostInterval = TimeSpan.FromDays(1);

        public HoroscopeAutoPoster(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Disable()
        {
            var timer = _serviceProvider.GetService<ITimer>();
            timer.Remove(PostNow);
        }

        public void Enable()
        {
            var timer = _serviceProvider.GetService<ITimer>();
            timer.Remove(PostNow);

            TimeSpan firstPostRemainedTime;
            if (DateTime.Now.TimeOfDay < _autoPostTime)
            {
                firstPostRemainedTime = _autoPostTime - DateTime.Now.TimeOfDay;
            }
            else
            {
                firstPostRemainedTime = _autoPostTime - DateTime.Now.TimeOfDay;
                firstPostRemainedTime = firstPostRemainedTime.Add(_autopostInterval);
            }

            timer.AddOneTime(c =>
            {
                timer.AddNumberTimes(PostNow, _autopostInterval, JobExecutionType.OnlyOneInstance);
            }, firstPostRemainedTime);
        }

        private void PostNow(CancellationToken cancellationToken)
        {
            PostNow();
        }

        private Dictionary<ZodiacSign, List<User>> GroupUserByHoroscopes(List<User> users)
        {
            var result = new Dictionary<ZodiacSign, List<User>>();
            var horoscope = _serviceProvider.GetService<IHoroscopeProvider>();
            foreach (var user in users)
            {
                if (user.BirthDate == null)
                    continue;

                if (DateTime.TryParseExact(user.BirthDate, new[] {"d.M.yyyy", "d.M"}, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var birthday))
                {
                    var zodiacSign = horoscope.GetZodiacSignByBirthDay(birthday);
                    if (!result.ContainsKey(zodiacSign))
                        result[zodiacSign] = new List<User>();
                    result[zodiacSign].Add(user);
                }
            }

            return result;
        }

        public void PostNow()
        {
            var api = _serviceProvider.GetService<IGroupApiProvider>().GetAdminApi();
            var groupId = _serviceProvider.GetService<IGroupApiProvider>().GetGroupId();
            var horoscope = _serviceProvider.GetService<IHoroscopeProvider>();
            var members = api.Groups.GetMembers(new GroupsGetMembersParams()
            {
                Fields = UsersFields.BirthDate,
                GroupId = (-groupId).ToString()
            }).ToList();

            var users = GroupUserByHoroscopes(members);

            var postText = new StringBuilder();

            foreach (var zodiacUsers in users)
            {
                foreach (var zodiacUser in zodiacUsers.Value)
                {
                    postText.Append($"@id{zodiacUser.Id}({zodiacUser.FirstName} {zodiacUser.LastName}), ");
                }

                postText.Remove(postText.Length - 2, 2);
                postText.AppendLine();

                postText.AppendLine("Бизнес-гороскоп:");
                postText.AppendLine(horoscope.GetBusinessHoroscope(zodiacUsers.Key));
                postText.AppendLine("Любовный гороскоп:");
                postText.AppendLine(horoscope.GetLoveHoroscope(zodiacUsers.Key));
                postText.AppendLine("Эротический гороскоп:");
                postText.AppendLine(horoscope.GetEroticHoroscope(zodiacUsers.Key));

                postText.AppendLine();
            }

            api.Wall.Post(new WallPostParams()
            {
                FromGroup = true,
                OwnerId = groupId,
                Message = postText.ToString()
            });

            Console.WriteLine("Horoscope has been posted");
        }
    }
}
