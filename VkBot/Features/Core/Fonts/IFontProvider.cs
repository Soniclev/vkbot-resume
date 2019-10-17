using System.Drawing;

namespace VkBot.Features.Core.Fonts
{
    public interface IFontProvider
    {
        FontFamily GetFontFamily(string fontName);
        Font GetFont(FontFamily fontFamily, FontSize fontSize);
        Font GetFont(string fontName, FontSize fontSize);
    }
}
