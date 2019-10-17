using VkBot.Core.Cache;
using VkNet.Model;

namespace VkBot.Users
{
    public interface ICacheableUser : ICacheble
    {
        User GetUser(long userId, bool forceUpdate = false);
        System.Drawing.Image GetUserAvatar(long userId, bool forceUpdate = false);
    }
}
