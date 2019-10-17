using System;
using System.Collections.Generic;
using System.Text;
using VkNet;

namespace VkBot.Core
{
    public interface IGroupApiProvider
    {
        VkApi GetGroupApi();
        VkApi GetAdminApi();
        long GetGroupId();
    }
}
