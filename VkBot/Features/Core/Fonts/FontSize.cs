using System;
using System.Collections.Generic;
using System.Text;

namespace VkBot.Features.Core.Fonts
{
    public struct FontSize
    {
        public float Pixels { get; set; }

        public float Points
        {
            get => Pixels * 12 / 16;
            set => Pixels = value * 16 / 12;
        }

        public FontSize(float sizeInPixels)
        {
            Pixels = sizeInPixels;
        }

        public static FontSize FromPixels(float size)
        {
            return new FontSize(size);
        }

        public static FontSize FromPoints(float size)
        {
            return new FontSize(size)
            {
                Points = size
            };
        }
    }
}
