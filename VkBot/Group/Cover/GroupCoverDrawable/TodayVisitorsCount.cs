using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VkBot.Core;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;

namespace VkBot.Group.Cover.GroupCoverDrawable
{
    internal class TodayVisitorsCount : IGroupCoverDrawable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TodayVisitorsCount> _logger;
        private long? _todayVisitorsAmount = null;

        public TodayVisitorsCount(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetService<ILogger<TodayVisitorsCount>>();
        }

        public void Draw(IGroupCoverRenderer groupCover)
        {
            try
            {
                if (_todayVisitorsAmount != null)
                    groupCover.DrawTodayVisitorsAmount(_todayVisitorsAmount.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отрисовке количества посетителей!");
            }
        }

        public Task Precache()
        {
            var adminApi = _serviceProvider.GetService<IGroupApiProvider>().GetAdminApi();
            var groupId = _serviceProvider.GetService<IGroupApiProvider>().GetGroupId();
            var today = DateTime.Today.AddHours(3);
            var todayEnd = DateTime.Today.AddDays(1).AddSeconds(-1);
            return Task.Run(() =>
            {
                var stats = adminApi.Stats.Get(new StatsGetParams()
                {
                    GroupId = (ulong)-groupId,
                    TimestampFrom = today,
                    TimestampTo = todayEnd,
                    Interval = StateInterval.Day
                });
                if (stats.Any())
                {
                    _todayVisitorsAmount = stats.First().Visitors.Views;
                }
            });
        }

        public async Task DrawAsync(IGroupCoverRenderer groupCover)
        {
            await Task.Run(() => Draw(groupCover)).ConfigureAwait(false);
        }
    }
}
