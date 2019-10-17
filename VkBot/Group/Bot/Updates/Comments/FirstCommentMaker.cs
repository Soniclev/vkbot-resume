using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VkBot.Core;
using VkNet.Model.GroupUpdate;
using VkNet.Model.RequestParams;

namespace VkBot.Group.Bot.Updates.Comments
{
    internal class FirstCommentMaker : INewPost
    {
        private const double Probability = 0.3;
        private readonly ILogger<FirstCommentMaker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public FirstCommentMaker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetService<ILogger<FirstCommentMaker>>();
        }

        public string GetEventTypeCode()
        {
            return "wall_post_new";
        }

        public bool IsNeedToCreateComment()
        {
            var randomProvider = _serviceProvider.GetService<IRandomDataProvider>();
            return randomProvider.GetDouble() < Probability;
        }

        public void OnEvent(GroupUpdate update)
        {
            if (IsNeedToCreateComment())
            {
                var api = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
                var post = update.WallPost;
                _logger.LogTrace("Создание комментария \"Первый!\" для поста {PostId}", post.Id ?? 0);
                api.Wall.CreateComment(new WallCreateCommentParams
                {
                    OwnerId = post.OwnerId,
                    PostId = post.Id ?? 0,
                    Message = "Первый!"
                });
                _logger.LogInformation("Первый комментарий создан для поста {PostId}", post.Id ?? 0);
            }
        }
    }
}
