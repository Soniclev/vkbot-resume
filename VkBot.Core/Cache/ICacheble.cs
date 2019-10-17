using System;
using System.Collections.Generic;
using System.Text;

namespace VkBot.Core.Cache
{
    public interface ICacheble
    {
        void Clear();
        void ClearOld(TimeSpan timeSpan);
    }
}
