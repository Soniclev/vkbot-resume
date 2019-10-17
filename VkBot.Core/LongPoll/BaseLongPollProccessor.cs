using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Model.GroupUpdate;

namespace VkBot.Core.LongPoll
{
    public abstract class BaseLongPollProccessor : ILongPollProccessor
    {
        protected static readonly Dictionary<string, Type> EventsRouteTable = new Dictionary<string, Type>();
        protected readonly IServiceProvider ServiceProvider;

        protected BaseLongPollProccessor(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public abstract void Proccess(IEnumerable<GroupUpdate> updates);

        void ILongPollProccessor.RegisterEvent<TInterface, TRealization>()
        {
            var realization = (INewEvent)ServiceProvider.GetService(typeof(TInterface));
            var eventTypeCode = realization.GetEventTypeCode();
            if (!EventsRouteTable.ContainsKey(eventTypeCode))
            {
                EventsRouteTable.Add(eventTypeCode, typeof(TInterface));
            }
        }
    }
}
