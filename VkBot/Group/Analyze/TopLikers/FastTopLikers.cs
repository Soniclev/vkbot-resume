using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VkNet.Abstractions;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace VkBot.Group.Analyze.TopLikers
{
    public class FastTopLikers : ITopLikers
    {
        private const int MaxCallsInExecute = 25;
        private readonly ILogger<FastTopLikers> _logger;
        private readonly IServiceProvider _serviceProvider;

        public FastTopLikers(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetService<ILogger<FastTopLikers>>();
        }

        public IEnumerable<KeyValuePair<long, int>> GetTopLikers(IVkApiCategories api, long ownerId, int lastDays = 7)
        {
            var posts = GetLastPosts(api, ownerId, lastDays);
            _logger.LogTrace("Получено {PostCount} постов за {Day} дней.", posts.Count, lastDays);
            var likers = new List<long>();
            while (posts.Count > 0)
            {
                var postsIds = posts.Take(MaxCallsInExecute).Select(x => x.Id);
                var executeScript = CreateExecuteScript(ownerId, postsIds);

                var response = api.Execute.Execute<List<VkCollection<long>>>(executeScript);
                foreach (var post in response)
                {
                    likers.AddRange(post);
                }

                posts = posts.Skip(MaxCallsInExecute).ToList();
            }

            var table = new Dictionary<long, int>();
            foreach (var liker in likers)
            {
                if (!table.ContainsKey(liker))
                {
                    table[liker] = 0;
                }

                table[liker]++;
            }

            var topLikers = table.ToList();
            topLikers.Sort((p1, p2) => p2.Value.CompareTo(p1.Value));
            return topLikers;
        }

        private static string CreateExecuteScript(long ownerId, IEnumerable<long?> postsIds)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("return [");
            for (var i = 0; i < postsIds.Count(); i++)
            {
                stringBuilder.Append(
                    $"API.likes.getList({{\"type\":\"post\", \"owner_id\": {ownerId}, \"item_id\": {postsIds.ElementAt(i)}}})");
                if (i != postsIds.Count() - 1)
                    stringBuilder.Append(",");
            }
            stringBuilder.Append("];");
            return stringBuilder.ToString();
        }

        private List<Post> GetLastPosts(IVkApiCategories api, long ownerId, int lastDays)
        {
            var result = new List<Post>();
            const int count = 30;
            ulong offset = 0;
            var stopFlag = false;
            while (!stopFlag)
            {
                var wall = api.Wall.Get(new WallGetParams()
                {
                    Count = count,
                    Offset = offset,
                    OwnerId = ownerId
                });
                if (wall.WallPosts.Count > 0)
                {
                    foreach (var post in wall.WallPosts)
                    {
                        if (post.Date?.Date.AddDays(lastDays) >= DateTime.Now.Date || post.IsPinned == true)
                            result.Add(post);
                        else
                        {
                            stopFlag = true;
                            break;
                        }
                    }

                    offset += count;
                }
                else
                {
                    break;
                }
            }

            return result;
        }
    }
}
