using System.Collections.Generic;
using VkNet.Abstractions;

namespace VkBot.Group.Analyze.TopLikers
{
    internal interface ITopLikers
    {
        IEnumerable<KeyValuePair<long, int>> GetTopLikers(IVkApiCategories api, long ownerId, int lastDays = 7);
    }
}
