using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VkBot.Core;
using VkNet.Model.RequestParams;

namespace VkBot.Group.Cover.GroupCoverDrawable
{
    internal class PostCount : IGroupCoverDrawable
    {
        private VkNet.Model.WallGetObject _wallInfo;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PostCount> _logger;

        public PostCount(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetService<ILogger<PostCount>>();
        }

        public void Draw( IGroupCoverRenderer groupCover)
        {
            try
            {
                groupCover.DrawPostsCount((long)_wallInfo.TotalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отрисовке количества постов!");
            }
        }

        public Task Precache()
        {
            var adminApi = _serviceProvider.GetService<IGroupApiProvider>().GetAdminApi();
            var groupId = _serviceProvider.GetService<IGroupApiProvider>().GetGroupId();
            return Task.Run(()=> _wallInfo = adminApi.Wall.Get(new WallGetParams() { Count = 1, OwnerId = groupId }));
        }

        public async Task DrawAsync(IGroupCoverRenderer groupCover)
        {
            await Task.Run(() => Draw(groupCover)).ConfigureAwait(false);
        }
    }
}
