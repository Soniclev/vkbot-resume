using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Core;
using VkBot.Core.Cache;
using VkBot.Group.Analyze.TopLikers;
using VkBot.Information.User;
using VkBot.Users;

namespace VkBot.Group.Cover.GroupCoverDrawable
{
    internal class TopLikers : IGroupCoverDrawable
    {
        private readonly List<TopLiker> _list = new List<TopLiker>();
        private int _lastDays = 7;
        private readonly IServiceProvider _serviceProvider;

        public TopLikers(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Draw(IGroupCoverRenderer groupCover)
        {
            groupCover.DrawTopLikersHeader($"ТОП-лайкеры за {_lastDays} дней:");
            for (var i = 0; i < _list.Count; i++)
            {
                var item = _list[i];
                groupCover.DrawTopLiker(item.FullNames, item.LikesAmount, i + 1);
                groupCover.DrawTopLikerAvatar(item.Avatar, i + 1);
            }
        }

        public Task Precache()
        {
            return Task.Run(() =>
            {
                var adminApi = _serviceProvider.GetService<IGroupApiProvider>().GetAdminApi();
                var groupId = _serviceProvider.GetService<IGroupApiProvider>().GetGroupId();
                var cacheableUser = _serviceProvider.GetService<ICacheableUser>();
                var fastTopLikers = _serviceProvider.GetService<ITopLikers>();
                var nickManager = _serviceProvider.GetService<INicknameManager>();
                var random = _serviceProvider.GetService<IRandomDataProvider>();
                var topLikers = fastTopLikers.GetTopLikers(adminApi, groupId, _lastDays);
                for (var i = 0; i < 3; i++)
                {
                    if (!topLikers.Any())
                        break;
                    var likesAmount = topLikers.First().Value;

                    var stringBuilder = new StringBuilder();
                    var localLikers = topLikers.Where(x => x.Value == likesAmount).Select(x => x.Key);
                    for (var j = 0; j < localLikers.Count(); j++)
                    {
                        var liker = localLikers.ElementAt(j);
                        if (nickManager.Exists(liker))
                        {
                            stringBuilder.Append(nickManager.GetNickname(liker));
                        }
                        else
                        {
                            var user = cacheableUser.GetUser(liker, false);
                            stringBuilder.Append($"{user.FirstName} {user.LastName}");
                        }

                        if (j != localLikers.Count() - 1)
                            stringBuilder.Append(", ");
                    }

                    var topLiker = new TopLiker()
                    {
                        LikesAmount = likesAmount,
                        FullNames = stringBuilder.ToString(),
                        Avatar = cacheableUser.GetUserAvatar(random.SelectElement(localLikers))
                    };

                    _list.Add(topLiker);
                    topLikers = topLikers.Where(x => x.Value != likesAmount);
                }
            });
        }

        public async Task DrawAsync(IGroupCoverRenderer groupCover)
        {
            await Task.Run(() => Draw(groupCover)).ConfigureAwait(false);
        }

        private class TopLiker : IDisposable
        {
            public string FullNames { get; set; }
            public int LikesAmount { get; set; }
            public Image Avatar { get; set; }

            public void Dispose()
            {
                Avatar.Dispose();
            }
        }
    }
}
