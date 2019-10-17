using System;
using System.Drawing;
using VkBot.Features.Core.Images;

namespace VkBot.Group.Cover
{
    public interface IGroupCoverRenderer : IDisposable
    {
        void DrawCloudnessImage();
        void DrawCloudnessStatus(double cloudness);
        void DrawPostsCount(long count);
        void DrawQuote(string text);
        void DrawString(string text, Font font, Color color, int x, int y, TextAlignment alignment = TextAlignment.Left);
        void DrawSunImage();
        void DrawSunStatus(DateTime sunset, DateTime sunrise);
        void DrawTemperature(double temperature);
        void DrawTopLiker(string fullName, int likesAmount, int position);
        void DrawTopLikerAvatar(Image avatar, int position);
        void DrawTopLikersHeader(string text);
        void DrawUpdateTime();
        void DrawWeatherIcon(Image image);
        void DrawWindArrow(double angle);
        void DrawWindSpeed(double speed);
        void DrawTodayVisitorsAmount(long count);
        byte[] GetCover();
    }
}