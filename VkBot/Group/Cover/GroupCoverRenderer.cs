using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Core;
using VkBot.Features.Core.Fonts;
using VkBot.Features.Core.Images;

namespace VkBot.Group.Cover
{
    public class GroupCoverRenderer : IGroupCoverRenderer
    {
        private readonly IImageRender _imageRender;
        private readonly IFontProvider _fontProvider;
        private readonly IServiceProvider _serviceProvider;

        public GroupCoverRenderer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _imageRender = serviceProvider.GetService<IImageRender>();
            using (var background = Image.FromFile(GetBackgroundImagePath()))
            {
                _imageRender.CreateFromImage(background);
            }
            _fontProvider = serviceProvider.GetService<IFontProvider>();
        }

        private string GetBackgroundImagePath()
        {
            var random = _serviceProvider.GetService<IRandomDataProvider>();
            var images = Directory.GetFiles("Data/GroupCoverBackgrounds").Where(x=>x.EndsWith(".png", StringComparison.OrdinalIgnoreCase));
            return random.SelectElement(images);
        }

        public void DrawTopLikersHeader(string text)
        {
            _imageRender.DrawString(text, GetDefaultFont(25), Color.FromArgb(231, 165, 128), 634, 231, TextAlignment.Left);
        }

        public void DrawTopLikerAvatar(Image avatar, int position)
        {
            const int radius = 15;
            int xCenter = radius;
            int yCenter = radius;
            var image = (Bitmap)avatar.GetThumbnailImage(2 * radius, 2 * radius, null, IntPtr.Zero);
            Func<int, int, bool> f = (x, y) => Math.Sqrt(Math.Pow(Math.Abs(x - xCenter), 2) + Math.Pow(Math.Abs(y - yCenter), 2)) < radius;
            Point pos = new Point(629, 228 + (position * 34));
            _imageRender.DrawImageWithMask(image, pos, f);
        }

        public void DrawTopLiker(string fullName, int likesAmount, int position)
        {
            if (position < 1 || position > 3)
                throw new ArgumentException(nameof(position));
            var wordUtils = _serviceProvider.GetService<IWordUtils>();
            var likesInfo = $"{likesAmount} {wordUtils.FormatCaseWord(likesAmount, "лайков", "лайк", "лайка")}";
            _imageRender.DrawString($"{position}  {fullName}, {likesInfo}", GetDefaultFont(25), Color.FromArgb(231, 165, 128), 636, 230 + (position * 34), TextAlignment.Left);
        }

        public void DrawUpdateTime()
        {
            _imageRender.DrawString($"Обновлено в {DateTime.Now.ToString("HH:mm")}", GetDefaultFont(36), Color.FromArgb(0, 255, 12), 976, 358);
        }

        public void DrawPostsCount(long count)
        {
            _imageRender.DrawString($"Всего постов: {count}", GetDefaultFont(25), Color.FromArgb(231, 165, 128), 706, 370, TextAlignment.Left);
        }

        public void DrawTodayVisitorsAmount(long count)
        {
            _imageRender.DrawString($"Просмотров сегодня: {count}", GetDefaultFont(25), Color.FromArgb(231, 165, 128), 675, 14, TextAlignment.Left);
        }

        public void DrawQuote(string text)
        {
            _imageRender.DrawString(text, GetQuoteFont(60), Color.FromArgb(128, 231, 187), 795, 91, TextAlignment.Center);
        }

        private Font GetQuoteFont(float fontSize)
        {
            return _fontProvider.GetFont("Carefree Cyrillic", FontSize.FromPixels(fontSize));
        }

        public void DrawWindSpeed(double speed)
        {
            _imageRender.DrawString($"Ветер {speed} м/c", GetFontForWeather(25), Color.FromArgb(106, 192, 243), 391, 327, TextAlignment.Left);
        }

        public void DrawTemperature(double temperature)
        {
            _imageRender.DrawString($"Температура {temperature} °C", GetFontForWeather(25), Color.FromArgb(106, 192, 243), 391, 361, TextAlignment.Left);
        }

        public void DrawWindArrow(double angle)
        {
            using (var image = Image.FromFile("Data/WindArrow.png"))
            {
                _imageRender.DrawImage(image, 355, 327, angle);
            }
        }

        public void DrawSunStatus(DateTime sunset, DateTime sunrise)
        {
            string text;
            if (DateTime.Now > sunset)
                text = $"Восход в {sunrise.ToString("HH:mm")}";
            else
                text = $"Заход в {sunset.ToString("HH:mm")}";
            _imageRender.DrawString(text, GetFontForWeather(25), Color.FromArgb(106, 192, 243), 391, 290, TextAlignment.Left);
        }

        public void DrawCloudnessImage()
        {
            using (var image = Image.FromFile("Data/Cloud.png"))
            {
                _imageRender.DrawImage(image, 352, 262);
            }
        }

        public void DrawCloudnessStatus(double cloudness)
        {
            var text = $"Облачность {cloudness}%";
            _imageRender.DrawString(text, GetFontForWeather(25), Color.FromArgb(106, 192, 243), 391, 258, TextAlignment.Left);
        }

        private Font GetFontForWeather(float fontSize)
        {
            return _fontProvider.GetFont("Collect Em All BB(RUS BY LYAJKA", FontSize.FromPixels(fontSize));
        }

        public void DrawSunImage()
        {
            using (var image = Image.FromFile("Data/Sun.png"))
            {
                _imageRender.DrawImage(image, 357, 292);
            }
        }

        public void DrawWeatherIcon(Image image)
        {
            _imageRender.DrawImage(image, (348 + 19) - (image.Width / 2), (367 + 14) - (image.Height / 2));
        }

        private Font GetDefaultFont(float fontSize)
        {
            return _fontProvider.GetFont("PF Agora Slab Pro Medium", FontSize.FromPixels(fontSize));
        }

        public byte[] GetCover()
        {
            return _imageRender.Render();
        }

        public void Dispose()
        {
            _imageRender.Dispose();
        }

        public void DrawString(string text, Font font, Color color, int x, int y, TextAlignment alignment = TextAlignment.Left)
        {
            _imageRender.DrawString(text, font, color, x, y, alignment);
        }
    }
}
