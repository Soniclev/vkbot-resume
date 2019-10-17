using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NLog.Extensions.Logging;
using VkBot.Group.Analyze.TopLikers;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using Xunit;

namespace VkBot.Tests.Information.GroupAnalyze
{
    public class FastTopLikersTests
    {
        [Fact]
        public void TestAllBehaviour()
        {
            // Setup
            const int lastDays = 7;
            const long ownerId = 123;

            const int liker1Id = 1;
            const int liker2Id = 2;
            const int liker3Id = 3;

            const int post1Id = 1;
            const int post2Id = 2;
            const int post3Id = 3;
            const int post4Id = 4;
            var mock = new Mock<IVkApiCategories>();
            mock.Setup(a => a.Wall.Get(It.IsAny<WallGetParams>(), false)).Returns(new WallGetObject
            {
                WallPosts = new List<Post>
                {
                    new Post
                    {
                        Id = post1Id,
                        Date = DateTime.Now
                    },
                    new Post
                    {
                        Id = post2Id,
                        Date = DateTime.Now.AddDays(-lastDays)
                    },
                    new Post
                    {
                        Id = post3Id,
                        Date = DateTime.Now.AddDays(-lastDays-1)
                    },
                    new Post
                    {
                        Id = post4Id,
                        Date = DateTime.Now.AddDays(-lastDays-10),
                        IsPinned = true
                    }
                }.ToReadOnlyCollection()
            });

            // исключаем третий пост, т.к. он вне диапазона и он не закреплён
            mock.Setup(a => a.Execute.Execute<List<VkCollection<long>>>(It.Is<string>(x => !x.Contains($"\"item_id\": {post3Id}}}")))).Returns(
                new List<VkCollection<long>>
                {
                    new VkCollection<long>(post4Id, new long[] { liker1Id, liker2Id, liker3Id}),
                    new VkCollection<long>(post3Id, new long[] { liker1Id, liker2Id, liker3Id}),
                    new VkCollection<long>(post2Id, new long[] { liker1Id, liker3Id})
                });


            var serviceProvider = new ServiceCollection()
                    .AddNullNLog()
                    .BuildServiceProvider();

            // Act
            var topLikers = new FastTopLikers(serviceProvider);
            var likers = topLikers.GetTopLikers(mock.Object, ownerId, lastDays);

            // Verify
            mock.Verify(a => a.Wall.Get(It.IsAny<WallGetParams>(), false), Times.Once);
            mock.Verify(a => a.Execute.Execute<List<VkCollection<long>>>(It.Is<string>(x => !x.Contains($"\"item_id\": {post3Id}}}"))), Times.Once);
            mock.Verify(a => a.Execute.Execute<List<VkCollection<long>>>(It.Is<string>(x => x.Contains($"\"item_id\": {post3Id}}}"))), Times.Never);

            AssertLikerPosition(likers, liker1Id, 0, 1);
            AssertLikerPosition(likers, liker2Id, 2);
            AssertLikerPosition(likers, liker3Id, 0, 1);

            AssertLikerLikesAmount(likers, liker1Id, 3);
            AssertLikerLikesAmount(likers, liker2Id, 2);
            AssertLikerLikesAmount(likers, liker3Id, 3);
        }

        private void AssertLikerPosition(IEnumerable<KeyValuePair<long, int>> likers, long likerId,
            params int[] positions)
        {
            foreach (var position in positions)
            {
                if (likers.ElementAt(position).Key == likerId)
                    return;
            }
            Assert.True(false, $"User with ID {likerId} there is not in the likers list at the specified position");
        }

        private void AssertLikerLikesAmount(IEnumerable<KeyValuePair<long, int>> likers, long likerId, int likes)
        {
            Assert.Equal(likers.First(x => x.Key == likerId).Value, likes);
        }
    }
}
