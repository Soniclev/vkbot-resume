using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;

namespace VkBot.Features.Core.Images
{
    public class ImageRender : IImageRender
    {
        private Graphics _graphics;
        private Bitmap _base;
        private readonly object _lock = new object();

        public void CreateFromImage(Image image)
        {
            _base = (Bitmap)image.Clone();
            _graphics = Graphics.FromImage(_base);
        }

        public void Dispose()
        {
            _graphics?.Dispose();
            _base?.Dispose();
        }

        public void DrawImage(Image image, int x, int y, double angle = 0)
        {
            lock (_lock)
            {
                if (Math.Abs(angle) > 0.1)
                {
                    using (var bitmap = RotateImage(image, (float)angle))
                    {
                        _graphics.DrawImage(bitmap, x, y);
                    }
                }
                else
                {
                    _graphics.DrawImage(image, x, y);
                } 
            }
        }

        public void DrawImageScaled(Image image, int x, int y, int width, int height, double angle = 0)
        {
            lock (_lock)
            {
                if (Math.Abs(angle) > 0.1)
                {
                    using (var bitmap = RotateImage(image, (float)angle))
                    {
                        _graphics.DrawImage(bitmap, x, y, width, height);
                    }
                }
                else
                {
                    _graphics.DrawImage(image, x, y, width, height);
                } 
            }
        }

        public void DrawString(string text, Font font, Color color, int x, int y, TextAlignment alignment = TextAlignment.Left)
        {
            lock (_lock)
            {
                var brush = new SolidBrush(color);
                var textSize = _graphics.MeasureString(text, font);
                switch (alignment)
                {
                    case TextAlignment.Left:
                        break;
                    case TextAlignment.Center:
                        x -= (int)(textSize.Width / 2);
                        break;
                    case TextAlignment.Right:
                        x -= (int)(textSize.Width);
                        break;
                    default:
                        break;
                }
                _graphics.DrawString(text, font, brush, x, y); 
            }
        }

        public void DrawPixel(int x, int y, Color color)
        {
            _base.SetPixel(x, y, color);
        }

        public byte[] Render()
        {
            lock (_lock)
            {
                byte[] result;
                using (var stream = new MemoryStream())
                {
                    _base.Save(stream, ImageFormat.Png);
                    result = stream.ToArray();
                }
                return result; 
            }
        }

        private static Image RotateImage(Image b, float angle)
        {
            //create a new empty bitmap to hold rotated image
            var returnBitmap = new Bitmap(b.Width, b.Height);
            //make a graphics object from the empty bitmap
            using (var g = Graphics.FromImage(returnBitmap))
            {
                //move rotation point to center of image
                g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);
                //rotate
                g.RotateTransform(angle);
                //move image back
                g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);
                //draw passed in image onto graphics object
                g.DrawImage(b, new Point(0, 0));
            }
            return returnBitmap;
        }

        public void DrawImageWithMask(Bitmap image, Point position, Func<int, int, bool> mask)
        {
            lock (_lock)
            {
                unsafe
                {
                    BitmapData bmd = _base.LockBits(new Rectangle(position, image.Size), ImageLockMode.ReadWrite,
                        PixelFormat.Format32bppArgb);
                    for (int y = 0; y < bmd.Height; y++)
                    {
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                        for (int x = 0; x < bmd.Width; x++)
                        {
                            if (mask(x, y))
                            {
                                var color = image.GetPixel(x, y);
                                row[x * 4] = color.B;   //Blue  0-255
                                row[x * 4 + 1] = color.G; //Green 0-255
                                row[x * 4 + 2] = color.R;   //Red   0-255
                                row[x * 4 + 3] = color.A;  //Alpha 0-255
                            }

                        }
                    }
                    _base.UnlockBits(bmd);
                }
            }
        }
    }
}
