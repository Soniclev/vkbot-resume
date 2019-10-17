using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace VkBot.Core.Timer
{
    class InfinityTimerJob : TimerJob
    {
        public InfinityTimerJob(Action<CancellationToken> action, TimeSpan span, JobExecutionType executionType) : base(action, span, executionType)
        {
        }

        public override bool IsActual()
        {
            return true;
        }

        public override bool IsWillBeActualAfterExecute()
        {
            return true;
        }
    }
}
