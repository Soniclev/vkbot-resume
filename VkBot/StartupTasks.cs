using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Core.Timer;
using VkBot.Group.Bot.Jobs.Horoscope;
using VkBot.Group.Cover;
using VkBot.Users;

namespace VkBot
{
    public static class StartupTasks
    {
        public static void InitializeGroupCoverUpdater(IServiceProvider serviceProvider)
        {
            var groupCoverUpdater = serviceProvider.GetService<IGroupCoverUpdater>();
            groupCoverUpdater.Start();
        }

        public static void InitializeHoroscope(IServiceProvider serviceProvider)
        {
            var horoscopeManager = serviceProvider.GetService<IHoroscopeAutoPoster>();
            horoscopeManager.Enable();
        }

        public static void InitializeCacheAutoclear(IServiceProvider serviceProvider)
        {
            var cacheableUser = serviceProvider.GetService<ICacheableUser>();
            var timer = serviceProvider.GetService<ITimer>();
            var autoClearTime = new TimeSpan(1, 0, 0);
            timer.AddNumberTimes((c)=>cacheableUser.ClearOld(autoClearTime), autoClearTime, JobExecutionType.AllowSeveralInstances);
        }
    }
}
