using System;
using System.Threading;

namespace VkBot.Group.Cover
{
    interface IGroupCoverUpdater : IDisposable
    {
        void Start();
        void Stop();
        void SetInterval(TimeSpan updateInterval);
        void UpdateGroupCover(CancellationToken cancellationToken);
    }
}
