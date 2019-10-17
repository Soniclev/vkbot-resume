using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace VkBot.Tests
{
    public static class NullNlogExtension
    {
        public static IServiceCollection AddNullNLog(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddNLog();
            });
        }
    }
}
