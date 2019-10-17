using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Information.User.Models;

namespace VkBot.Information.User
{
    internal class GeoPositionManager : IGeoPositionManager
    {
        private readonly IServiceProvider _serviceProvider;

        public GeoPositionManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool Exists(long userId)
        {
            var db = _serviceProvider.GetService<UserContext>();
            return db.Find<GeoPosition>(userId) != null;
        }

        public GeoPosition Get(long userId)
        {
            var db = _serviceProvider.GetService<UserContext>();
            return db.Find<GeoPosition>(userId);
        }

        public List<GeoPosition> GetAll()
        {
            var db = _serviceProvider.GetService<UserContext>();
            return db.GeoPositions.ToList();
        }

        public bool Remove(long userId)
        {
            var db = _serviceProvider.GetService<UserContext>();
            if (Exists(userId))
            {
                db.Remove(Get(userId));
                db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Set(GeoPosition geoPosition)
        {
            var db = _serviceProvider.GetService<UserContext>();
            geoPosition.UpdateTime = DateTime.Now;
            if (!Exists(geoPosition.UserId))
            {
                db.Add(geoPosition);
            }
            else
            {
                var geo = db.GeoPositions.Find(geoPosition.UserId);
                geo.Commentary = geoPosition.Commentary;
                geo.Place = geoPosition.Place;
                geo.UpdateTime = geoPosition.UpdateTime;
            }

            db.SaveChanges();
        }
    }
}
