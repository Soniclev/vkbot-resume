using System;
using System.Collections.Generic;
using System.Text;

namespace VkBot.Core.Timer
{
    public enum JobExecutionType
    {
        OnlyOneInstance,
        KillPreviousInstance,
        AllowSeveralInstances
    }
}
