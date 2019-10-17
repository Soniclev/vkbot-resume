using System;
using System.Collections.Generic;
using System.Threading;

namespace VkBot.Core.Timer
{
    public interface ITimer
    {
        void AddOneTime(Action<CancellationToken> action, TimeSpan timeSpan);
        void AddNumberTimes(Action<CancellationToken> action, TimeSpan timeSpan, JobExecutionType executionType, long times = 0);
        void AddUntil(Action<CancellationToken> action, TimeSpan timeSpan, JobExecutionType executionType, Func<bool> condition);
        bool Remove(Action<CancellationToken> action);
        bool Remove(IEnumerable<Action<CancellationToken>> actions);
        bool RemoveAll();
    }
}
