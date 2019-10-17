using System;
using System.Collections.Generic;
using System.Text;

namespace VkBot.Group.Bot.Jobs.Horoscope
{
    interface IHoroscopeAutoPoster
    {
        void Enable();
        void Disable();
        void PostNow();
    }
}
