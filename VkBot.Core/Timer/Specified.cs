using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace VkBot.Core.Timer
{
    internal class Specified : TimerJob
    {
        private readonly long _remaindedExecutionTime;

        public Specified(Action<CancellationToken> action, TimeSpan span, JobExecutionType executionType, long remaindedExecutionTime = 0) : base(action, span, executionType)
        {
            _remaindedExecutionTime = remaindedExecutionTime;
        }

        public override bool IsActual()
        {
            return _remaindedExecutionTime > 0;
        }

        public override bool IsWillBeActualAfterExecute()
        {
            return _remaindedExecutionTime > 1;
        }
    }
}
