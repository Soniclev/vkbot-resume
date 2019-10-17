using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VkBot.Core;
using VkBot.Group.Bot.Updates;
using VkBot.Group.Cover;
using VkNet.Exception;
using VkNet.Model.RequestParams;

namespace VkBot
{
    internal class Program
    {
        private const int LongPollWaitTime = 25;

#pragma warning disable RCS1163 // Unused parameter.
        private static void Main(string[] args)
#pragma warning restore RCS1163 // Unused parameter.
        {
            var serviceProvider = Startup.CreateServiceProvider();

            LogProgramStartup(serviceProvider);

            var logger = serviceProvider.GetService<ILogger<Program>>();

            try
            {
                StartupTasks.InitializeGroupCoverUpdater(serviceProvider);
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Ошибка запуска автообновления обложки!");
            }
            try
            {
                StartupTasks.InitializeCacheAutoclear(serviceProvider);
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Ошибка запуска периодической очистки кэша!");
            }


            while (true)
            {
                try
                {
                    DoMainLoop(serviceProvider);
                }
                catch (LongPollKeyExpiredException)
                {
                    logger.LogDebug("Ключ LongPoll просрочен!");
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    logger.LogWarning(e, "Ошибка в LongPoll!");
                }
            }
        }

        private static void LogProgramStartup(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger<Program>>();
#if DEBUG_CONFIGURATION
logger.LogInformation("Запуск программы. Конфигурация: DEBUG");
#endif
#if DEV_GROUP
logger.LogInformation("Запуск программы. Конфигурация: DEV_GROUP");
#endif
#if RELEASE
logger.LogInformation("Запуск программы. Конфигурация: RELEASE");
#endif
        }

        private static void DoMainLoop(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger<Program>>();
            var longPollProccessor = serviceProvider.GetService<ILongPollProccessor>();
            var api = serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
            var groupId = serviceProvider.GetService<IGroupApiProvider>().GetGroupId();
            var lpServer = api.Groups.GetLongPollServer((ulong)-groupId);
            while (true)
            {
                logger.LogTrace("Запрос к LongPoll с удержанием {Wait} секунд.", LongPollWaitTime);
                var history = api.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams
                {
                    Key = lpServer.Key,
                    Server = lpServer.Server,
                    Ts = lpServer.Ts,
                    Wait = LongPollWaitTime
                });
                logger.LogTrace("Получено с LongPoll {Count} новых событий.", history.Updates.Count());
                longPollProccessor.Proccess(history.Updates);
                lpServer.Ts = history.Ts;
            }
        }
    }
}
