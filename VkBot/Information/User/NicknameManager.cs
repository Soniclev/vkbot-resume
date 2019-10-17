using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Information.User.Models;

namespace VkBot.Information.User
{
    internal class NicknameManager : INicknameManager
    {
        private readonly IServiceProvider _serviceProvider;

        public NicknameManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool Exists(long userId)
        {
            var db = _serviceProvider.GetService<UserContext>();
            return db.Find<Nickname>(userId) != null;
        }

        public string GetNickname(long userId)
        {
            var db = _serviceProvider.GetService<UserContext>();
            var nickname = db.Find<Nickname>(userId);
            return nickname.Nick;
        }

        public bool IsValid(string nick)
        {
            return new Nickname() { Nick = nick}.IsValid();
        }

        public void SetNickname(long userId, string nickname)
        {
            var db = _serviceProvider.GetService<UserContext>();
            if (Exists(userId))
            {
                var nick = db.Find<Nickname>(userId);
                nick.Nick = nickname;
            }
            else
            {
                db.Add(new Nickname
                {
                    UserId = userId,
                    Nick = nickname
                });
            }
            db.SaveChanges();
        }
    }
}
