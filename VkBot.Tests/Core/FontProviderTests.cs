using VkBot.Features.Core.Fonts;
using Xunit;

namespace VkBot.Tests.Core
{
    public class FontProviderTests
    {
        private readonly string[] _fontNames = {
            "Carefree Cyrillic",
            "Collect Em All BB(RUS BY LYAJKA",
            "PF Agora Slab Pro Medium"
        };

        [Fact]
        public void CanLoadAllNecessaryFonts()
        {
            var fontProvider = new FontProvider();
            foreach (var fontName in _fontNames)
            {
                Assert.NotNull(fontProvider.GetFontFamily(fontName));
            }
            foreach (var fontName in _fontNames)
            {
                Assert.NotNull(fontProvider.GetFont(fontName, FontSize.FromPixels(23)));
            }
            foreach (var fontName in _fontNames)
            {
                Assert.NotNull(fontProvider.GetFont(fontProvider.GetFontFamily(fontName), FontSize.FromPixels(23)));
            }
        }
    }
}
