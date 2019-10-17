using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VkBot.Core;
using VkBot.Core.Rest;
using VkNet.Model.GroupUpdate;
using VkNet.Model.RequestParams;

namespace VkBot.Group.Bot.Updates.Comments
{
    internal class MeaningJokeComment : INewPost
    {
        private const string ImagesDirectory = @"Data/MeaningJokes/";
        private readonly ILogger<MeaningJokeComment> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MeaningJokeComment(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetService<ILogger<MeaningJokeComment>>();
        }

        public string GetEventTypeCode()
        {
            return "wall_post_new";
        }

        public string SelectRandomJokeImage()
        {
            var files = GetAvailableImages();
            _logger.LogTrace("Найдено {Count} изображений", files.Count());
            var randomProvider = _serviceProvider.GetService<IRandomDataProvider>();
            return randomProvider.SelectElement(files);
        }

        private static System.Collections.Generic.IEnumerable<string> GetAvailableImages()
        {
            return Directory.GetFiles(ImagesDirectory).Where(x => x.EndsWith(".png") || x.EndsWith(".jpg") || x.EndsWith("jpeg"));
        }

        public void OnEvent(GroupUpdate update)
        {
            var api = _serviceProvider.GetService<IGroupApiProvider>().GetAdminApi();
            var groupId = _serviceProvider.GetService<IGroupApiProvider>().GetGroupId();
            var restClient = _serviceProvider.GetService<IRestClient>();
            var jokeImagePath = SelectRandomJokeImage();
            var imageExtension = jokeImagePath.Split('.').Last();
            var uploadServerInfo = api.Photo.GetWallUploadServer(-groupId);
            var uploadResponse = restClient.UploadImage(uploadServerInfo.UploadUrl, File.ReadAllBytes(jokeImagePath), imageExtension);
            var photos = api.Photo.SaveWallPhoto(uploadResponse, null, (ulong)-groupId);
            var post = update.WallPost;
            _logger.LogTrace("Создание комментария с изображением для поста {PostId}", post.Id ?? 0);
            api.Wall.CreateComment(new WallCreateCommentParams
            {
                OwnerId = post.OwnerId,
                PostId = post.Id ?? 0,
                Attachments = photos,
                FromGroup = -groupId
            });
            _logger.LogInformation("Создан комментарий с изображением для поста {PostId}", post.Id ?? 0);
        }
    }
}
