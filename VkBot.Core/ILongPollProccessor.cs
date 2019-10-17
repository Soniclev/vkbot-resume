using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Core.LongPoll;
using VkNet.Model.GroupUpdate;

namespace VkBot.Core
{
    public interface ILongPollProccessor
    {
        void RegisterEvent<TInterface, TRealization>()
            where TInterface : class, INewEvent where TRealization : class, TInterface;
        void Proccess(IEnumerable<GroupUpdate> updates);
    }
}
