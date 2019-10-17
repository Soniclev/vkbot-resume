using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace VkBot.Core
{
    public class GroupApiProvider : IGroupApiProvider, IDisposable
    {
        private VkApi _groupApi;
        private VkApi _adminApi;
        private long _groupId;

        public GroupApiProvider()
        {
            GetGroupId();
            GetAdminApi();
            GetGroupApi();
        }

        public void Dispose()
        {
            _groupApi?.Dispose();
            _adminApi?.Dispose();
        }

        public VkApi GetAdminApi()
        {
            if (_adminApi == null)
            {
                _adminApi = new VkApi();
                var adminAccessToken = File.ReadAllText("Data/Tokens/AdminAccessToken.txt");
                _adminApi.Authorize(new ApiAuthParams
                {
                    AccessToken = adminAccessToken,
                    Settings = GetAdminSettings()
                });
            }
            return _adminApi;
        }

        private static Settings GetAdminSettings()
        {
            return Settings.Wall
                   | Settings.Friends
                   | Settings.Groups
                   | Settings.Photos
                   | Settings.Statistic
                   | Settings.Status
                   | Settings.Video;
        }

        public VkApi GetGroupApi()
        {
            if (_groupApi == null)
            {
                _groupApi = new VkApi();
#if DEV_GROUP
                var groupAccessToken = File.ReadAllText("Data/Tokens/DEV.GroupAccessToken.txt");
#else
                var groupAccessToken = File.ReadAllText("Data/Tokens/GroupAccessToken.txt");
#endif
                _groupApi.Authorize(new ApiAuthParams() { AccessToken = groupAccessToken });
                _groupApi.RequestsPerSecond = 20;
            }
            return _groupApi;
        }

        public long GetGroupId()
        {
            if (_groupId == 0)
            {
#if DEV_GROUP
                var groupIdString = File.ReadAllText("Data/Tokens/DEV.GroupId.txt");
#else
                var groupIdString = File.ReadAllText("Data/Tokens/GroupId.txt");
#endif
                _groupId = long.Parse(groupIdString);
            }
            return _groupId;
        }
    }
}
