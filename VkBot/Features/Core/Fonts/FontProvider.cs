using System.Collections.Generic;
using System.Drawing;

namespace VkBot.Features.Core.Fonts
{
    public class FontProvider : IFontProvider
    {
        private readonly Dictionary<string, FontFamily> _fontCache = new Dictionary<string, FontFamily>();

        public Font GetFont(FontFamily fontFamily, FontSize fontSize)
        {
            return new Font(fontFamily, fontSize.Points);
        }

        public Font GetFont(string fontName, FontSize fontSize)
        {
            return new Font(GetFontFamily(fontName), fontSize.Points);
        }

        public FontFamily GetFontFamily(string fontName)
        {
             if (!_fontCache.ContainsKey(fontName))
            {
                _fontCache[fontName] = new FontFamily(fontName);
            }

            return _fontCache[fontName];
        }
    }
}
