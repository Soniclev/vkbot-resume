using System;
using System.Collections.Generic;
using System.Text;
using VkBot.Information.User.Models;

namespace VkBot.Information.User
{
    interface IGeoPositionManager
    {
        bool Exists(long userId);
        GeoPosition Get(long userId);
        bool Remove(long userId);
        List<GeoPosition> GetAll();
        void Set(GeoPosition geoPosition);
    }
}
