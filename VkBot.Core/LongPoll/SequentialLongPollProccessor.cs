using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VkNet.Model.GroupUpdate;

namespace VkBot.Core.LongPoll
{
    public class SequentialLongPollProccessor : BaseLongPollProccessor
    {
        private readonly ILogger<SequentialLongPollProccessor> _logger;
        public SequentialLongPollProccessor(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<SequentialLongPollProccessor>>();
        }

        public override void Proccess(IEnumerable<GroupUpdate> updates)
        {
            foreach (var historyUpdate in updates)
            {
                var key = historyUpdate.Type.ToString();
                if (EventsRouteTable.ContainsKey(key))
                {
                    var eventListeners =
                        (IEnumerable<INewEvent>)ServiceProvider.GetServices(EventsRouteTable[key]);
                    if (eventListeners.Any())
                    {
                        _logger.LogTrace("Найден {Count} обработчиков для события {Event}", eventListeners.Count(), key);
                    }
                    else
                    {
                        _logger.LogWarning("Найден {Count} обработчиков для события {Event}", eventListeners.Count(), key);
                    }
                    foreach (var eventListener in eventListeners)
                    {
                        try
                        {
                            eventListener.OnEvent(historyUpdate);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Ошибка при обработке события {} обработчиком {}", key, eventListener.GetType().ToString());
                        }
                    }
                }
            }
        }
    }
}
