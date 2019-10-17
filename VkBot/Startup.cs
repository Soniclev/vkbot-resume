using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using VkBot.Core;
using VkBot.Core.Cache;
using VkBot.Core.LongPoll;
using VkBot.Core.Rest;
using VkBot.Core.Timer;
using VkBot.Features.Core.Fonts;
using VkBot.Features.Core.Images;
using VkBot.Features.MemeGenerator;
using VkBot.Features.MemeGenerator.Commands;
using VkBot.Group.Analyze.TopLikers;
using VkBot.Group.Bot.Jobs.Horoscope;
using VkBot.Group.Bot.Updates;
using VkBot.Group.Bot.Updates.Comments;
using VkBot.Group.Bot.Updates.Messages;
using VkBot.Group.Cover;
using VkBot.Group.Cover.GroupCoverDrawable;
using VkBot.Information.Horoscope;
using VkBot.Information.User;
using VkBot.Information.Weather;
using VkBot.Users;

namespace VkBot
{
    public static class Startup
    {
        static IServiceCollection RegisterEvent<TInterface, TRealization>(this IServiceCollection serviceCollection) where TInterface : class, INewEvent where TRealization : class, TInterface
        {
            serviceCollection = serviceCollection.AddSingleton<TInterface, TRealization>();
            var longPollProccessor = serviceCollection.BuildServiceProvider().GetService<ILongPollProccessor>();
            longPollProccessor.RegisterEvent<TInterface, TRealization>();
            return serviceCollection;
        }

        static IServiceCollection InitializeLogging(this IServiceCollection serviceCollection)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Data", "Configs"))
                .AddJsonFile("loggingsettings.json", optional: true, reloadOnChange: true)
                .Build();

            LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));

            return serviceCollection.AddLogging(loggingBuilder =>
            {
                // configure Logging with NLog
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                loggingBuilder.AddNLog(config);
            });
        }

        public static ServiceProvider CreateServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
                .InitializeLogging()
                .AddSingleton<ITimer, LowAccurateTimer>()
                .AddSingleton<IGroupApiProvider, GroupApiProvider>()
                .AddSingleton<IFontProvider, FontProvider>()
                .AddSingleton<ILongPollProccessor, SequentialLongPollProccessor>()
                .AddSingleton<IWordUtils, WordUtils>()
                .AddSingleton<ICacheableUser, CacheableUser>()
                .AddSingleton<IRandomDataProvider, RandomDataProvider>()
                .AddSingleton<IWeatherProvider, CacheableWeatherProvider>(provider => new CacheableWeatherProvider(new OpenweatherProvider(provider)))
                .AddSingleton<IRestClient, RestHttpClient>()
                .AddSingleton<INicknameManager, NicknameManager>()
                .AddSingleton<IGeoPositionManager, GeoPositionManager>()
                .AddSingleton<IGroupCoverUpdater, GroupCoverUpdaterAutoUpdater>()
                .AddSingleton<IHoroscopeProvider, IgnioHoroscopeProvider>()
                .AddSingleton<IHoroscopeAutoPoster, HoroscopeAutoPoster>()
                .AddTransient<ITopLikers, FastTopLikers>()
                .AddTransient<IGroupCoverRenderer, GroupCoverRenderer>()
                .AddSingleton<IGroupCoverDrawable, PostCount>()
                .AddSingleton<IGroupCoverDrawable, RandomQuote>()
                .AddSingleton<IGroupCoverDrawable, CurrentWeather>()
                .AddTransient<IGroupCoverDrawable, TopLikers>()
                .AddTransient<IGroupCoverDrawable, TodayVisitorsCount>()
                .AddTransient<IImageRender, ImageRender>()
                .AddTransient<IMemeCommand, CreateImageFrom>()
                .AddTransient<IMemeCommand, RenderImage>()
                .AddTransient<IMemeCommand, RenderText>()
                .AddTransient<IMemeCommand, RenderUserAvatar>()
                .AddTransient<IMemeGenerator, MemeGenerator>()
                .AddDbContext<UserContext>()
#if RELEASE || DEV_GROUP || DEBUG
                .RegisterEvent<INewPost, FirstCommentMaker>()
                .RegisterEvent<INewPost, MeaningJokeComment>()
                .RegisterEvent<INewMessage, BaseMessageRouter>()
#endif
                .BuildServiceProvider();
            return serviceProvider;
        }
    }
}
