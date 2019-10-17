using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Core;
using VkBot.Core.Cache;
using VkBot.Core.Rest;
using VkBot.Utils.VkNetExtensions;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace VkBot.Users
{
    public class CacheableUser : ICacheableUser
    {
        private readonly Dictionary<long, CacheableEntry<User>> _usersCache = new Dictionary<long, CacheableEntry<User>>();
        private readonly Dictionary<long, CacheableEntry<System.Drawing.Image>> _userAvatarsCache = new Dictionary<long, CacheableEntry<System.Drawing.Image>>();
        private readonly IServiceProvider _serviceProvider;

        public CacheableUser(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Clear()
        {
            _usersCache.Clear();
            _userAvatarsCache.Clear();
        }

        public void ClearOld(TimeSpan timeSpan)
        {
            foreach (var item in _usersCache)
            {
                if (item.Value.IsOld(timeSpan))
                    _usersCache.Remove(item.Key);
            }

            foreach (var item in _userAvatarsCache)
            {
                if (item.Value.IsOld(timeSpan))
                    _usersCache.Remove(item.Key);
            }
        }

        public User GetUser(long userId, bool forceUpdate = false)
        {
            if (forceUpdate || !_usersCache.ContainsKey(userId))
            {
                var user = _serviceProvider.GetService<IGroupApiProvider>().GetAdminApi().Users.Get(new[] { userId }, ProfileFields.All);
                _usersCache[userId] = new CacheableEntry<User>(user[0]);
            }

            return _usersCache[userId].Entry;
        }

        public System.Drawing.Image GetUserAvatar(long userId, bool forceUpdate = false)
        {
            if (forceUpdate || !_userAvatarsCache.ContainsKey(userId))
            {
                var user = GetUser(userId, forceUpdate);
                var url = user.PhotoPreviews.GetMaxPhotoUrl();
                var stream = _serviceProvider.GetService<IRestClient>().GetStream(url);
                _userAvatarsCache[userId] = new CacheableEntry<System.Drawing.Image>(System.Drawing.Image.FromStream(stream));
            }

            return (System.Drawing.Image)_userAvatarsCache[userId].Entry.Clone();
        }
    }
}
