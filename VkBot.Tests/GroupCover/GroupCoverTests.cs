using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Core;
using VkBot.Core.Rest;
using VkBot.Features.Core.Fonts;
using VkBot.Features.Core.Images;
using VkBot.Group.Cover;
using Xunit;

namespace VkBot.Tests.GroupCover
{
    public class GroupCoverTests
    {
        private readonly byte[] _pngHeader = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private readonly IServiceProvider _serviceProvider = new ServiceCollection()
            .AddSingleton<IFontProvider, FontProvider>()
            .AddSingleton<IRestClient, RestHttpClient>()
            .AddTransient<IImageRender, ImageRender>()
            .AddSingleton<IRandomDataProvider, RandomDataProvider>()
            .BuildServiceProvider();

        public GroupCoverTests()
        {
            if(!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Data")))
                Environment.CurrentDirectory = @"../../../../VkBot/";
        }

        [Fact]
        public void CheckIsImagePng()
        {
            var groupCover = new GroupCoverRenderer(_serviceProvider);
            Assert.True(groupCover.GetCover().Length > 0);
            var bytes = groupCover.GetCover().Take(8).ToArray();
            Assert.Equal(_pngHeader, bytes);
        }

        [Fact]
        public void SelfTest_ImageChangingTest()
        {
            var groupCover = new GroupCoverRenderer(_serviceProvider);
            var image = groupCover.GetCover();
            Assert.False(IsImageChanged(groupCover, ref image));
            groupCover.DrawString("Test String", new Font(FontFamily.GenericSansSerif, 25), Color.Blue, 50, 50);
            Assert.True(IsImageChanged(groupCover, ref image));
            Assert.False(IsImageChanged(groupCover, ref image));
        }

        [Fact]
        public void CheckTextAlignment()
        {
            var groupCover1 =
                new GroupCoverRenderer(_serviceProvider);
            groupCover1.DrawString("Test String", new Font(FontFamily.GenericSansSerif, 25), Color.Blue, 50, 50, TextAlignment.Left);
            var image1 = groupCover1.GetCover();

            var groupCover2 =
                new GroupCoverRenderer(_serviceProvider);
            groupCover2.DrawString("Test String", new Font(FontFamily.GenericSansSerif, 25), Color.Blue, 50, 50, TextAlignment.Center);
            var image2 = groupCover2.GetCover();

            var groupCover3 =
                new GroupCoverRenderer(_serviceProvider);
            groupCover3.DrawString("Test String", new Font(FontFamily.GenericSansSerif, 25), Color.Blue, 50, 50, TextAlignment.Right);
            var image3 = groupCover3.GetCover();

            Assert.False(IsImageEqual(image1, image2));
            Assert.False(IsImageEqual(image2, image3));
            Assert.False(IsImageEqual(image1, image3));
        }

        [Fact]
        public void CheckImageRendering()
        {
            var groupCover = new GroupCoverRenderer(_serviceProvider);
            var image = groupCover.GetCover();
            groupCover.DrawPostsCount(228);
            Assert.True(IsImageChanged(groupCover, ref image));

            groupCover.DrawUpdateTime();
            Assert.True(IsImageChanged(groupCover, ref image));

            groupCover.DrawQuote("Цитата 1");
            Assert.True(IsImageChanged(groupCover, ref image));

            groupCover.DrawWeatherIcon(Image.FromFile(@"Data/Cloud.png"));
            Assert.True(IsImageChanged(groupCover, ref image));

            groupCover.DrawTemperature(-11.6d);
            Assert.True(IsImageChanged(groupCover, ref image));

            groupCover.DrawWindSpeed(3.4d);
            Assert.True(IsImageChanged(groupCover, ref image));

            groupCover.DrawWindArrow(170);
            Assert.True(IsImageChanged(groupCover, ref image));

            groupCover.DrawSunImage();
            Assert.True(IsImageChanged(groupCover, ref image));

            groupCover.DrawSunStatus(DateTime.Now, DateTime.Now.AddHours(3));
            Assert.True(IsImageChanged(groupCover, ref image));

            groupCover.DrawCloudnessImage();
            Assert.True(IsImageChanged(groupCover, ref image));

            groupCover.DrawCloudnessStatus(60);
            Assert.True(IsImageChanged(groupCover, ref image));
        }

        private bool IsImageEqual(byte[] image1, byte[] image2)
        {
            if (image1.Length != image2.Length)
            {
                return false;
            }

            for (var i = 0; i < image1.Length; i++)
            {
                if (image1[i] != image2[i])
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsImageChanged(GroupCoverRenderer groupCover, ref byte[] image)
        {
            var equal = IsImageEqual(groupCover.GetCover(), image);
            if (!equal)
                image = groupCover.GetCover();

            return !equal;
        }
    }
}
