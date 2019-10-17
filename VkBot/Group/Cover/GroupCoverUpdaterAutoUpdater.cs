using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VkBot.Core;
using VkBot.Core.Rest;
using VkBot.Core.Timer;
using VkBot.Group.Cover.GroupCoverDrawable;

namespace VkBot.Group.Cover
{
    internal class GroupCoverUpdaterAutoUpdater : IGroupCoverUpdater
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITimer _timer;
        private readonly ILogger<GroupCoverUpdaterAutoUpdater> _logger;
        private TimeSpan _updateInterval = TimeSpan.FromSeconds(60);

        public GroupCoverUpdaterAutoUpdater(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _timer = serviceProvider.GetService<ITimer>();
            _logger = _serviceProvider.GetService<ILogger<GroupCoverUpdaterAutoUpdater>>();
        }

        public void Start()
        {
            _logger.LogInformation("Автообновление обложки было активировано с интервалом {Interval}", _updateInterval);
            _timer.Remove(UpdateGroupCover);
            _timer.AddNumberTimes(UpdateGroupCover, _updateInterval, 0);
        }

        public void Stop()
        {
            _logger.LogInformation("Автообновление обложки было приостановлено");
            _timer.Remove(UpdateGroupCover);
        }

        public void UpdateGroupCover(CancellationToken cancellationToken)
        {
            _logger.LogTrace("Начинаем обновлять обложку...");
            var groupCover = _serviceProvider.GetService<IGroupCoverRenderer>();
            var drawables = _serviceProvider.GetServices<IGroupCoverDrawable>().ToList();
            var tasks = new List<Task>();

            _logger.LogTrace("Доступно {Count} отрисовок обложки", drawables.Count);

            foreach (var drawable in drawables)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        drawable.Precache().Wait(cancellationToken);
                        drawable.DrawAsync(groupCover).Wait(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }, cancellationToken));
            }

            var api = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
            var groupId = _serviceProvider.GetService<IGroupApiProvider>().GetGroupId();
            var restClient = _serviceProvider.GetService<IRestClient>();

            _logger.LogTrace("Запрашиваем сервер для загрузки фотографии...");
            var uploadInfo = api.Photo.GetOwnerCoverPhotoUploadServer(-groupId, cropX2: 1590, cropY2: 400);

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Обновление обложки было отменено.");
                return;
            }

            _logger.LogTrace("Ждём завершения всех отрисовок обложки...");
            Task.WaitAll(tasks.ToArray(), cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Обновление обложки было отменено.");
                return;
            }

            groupCover.DrawUpdateTime();

            _logger.LogTrace("Загружаем обложку на сервера ВК...");
            var cover = groupCover.GetCover();
            var uploadTask = restClient.UploadImageAsync(uploadInfo.UploadUrl, cover, "png");
            uploadTask.Wait(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Обновление обложки было отменено.");
                return;
            }

            _logger.LogTrace("Обновляем обложку группы...");
            api.Photo.SaveOwnerCoverPhoto(uploadTask.Result);
            _logger.LogInformation("Обложка была успешна загружена и обновлена.");
            groupCover.Dispose();
        }

        public void SetInterval(TimeSpan updateInterval)
        {
            _logger.LogInformation("Новый интервал обновления обложки: {Interval}", updateInterval);
            Stop();
            _updateInterval = updateInterval;
            Start();
        }

        public void Dispose()
        {
        }
    }
}
