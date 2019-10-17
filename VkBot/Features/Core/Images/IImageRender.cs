using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace VkBot.Features.Core.Images
{
    public interface IImageRender : IDisposable
    {
        void CreateFromImage(Image image);
        void DrawImage(Image image, int x, int y, double angle = 0);
        void DrawImageScaled(Image image, int x, int y, int width, int height, double angle = 0);

        void DrawString(string text, Font font, Color color, int x, int y,
            TextAlignment alignment = TextAlignment.Left);

        void DrawImageWithMask(Bitmap image, Point position, Func<int, int, bool> mask);

        byte[] Render();
    }
}
