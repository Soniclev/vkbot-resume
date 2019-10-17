using System;
using System.Collections.Generic;
using System.Text;
using VkBot.Core;
using Xunit;

namespace VkBot.Tests.Core
{
    public class WordUtilsTests
    {
        [Fact]
        public void FormatCaseWordTest()
        {
            IWordUtils wordUtils = new WordUtils();
            Assert.Equal("лайков", wordUtils.FormatCaseWord(0, "лайков", "лайк", "лайка"));
            Assert.Equal("лайк", wordUtils.FormatCaseWord(1, "лайков", "лайк", "лайка"));
            Assert.Equal("лайка", wordUtils.FormatCaseWord(2, "лайков", "лайк", "лайка"));
            Assert.Equal("лайка", wordUtils.FormatCaseWord(3, "лайков", "лайк", "лайка"));
            Assert.Equal("лайка", wordUtils.FormatCaseWord(4, "лайков", "лайк", "лайка"));

            for (var i = 5; i <= 20; i++)
            {
                Assert.Equal("лайков", wordUtils.FormatCaseWord(i, "лайков", "лайк", "лайка"));
            }

            Assert.Equal("лайк", wordUtils.FormatCaseWord(21, "лайков", "лайк", "лайка"));
            Assert.Equal("лайка", wordUtils.FormatCaseWord(22, "лайков", "лайк", "лайка"));
            Assert.Equal("лайка", wordUtils.FormatCaseWord(23, "лайков", "лайк", "лайка"));
            Assert.Equal("лайка", wordUtils.FormatCaseWord(24, "лайков", "лайк", "лайка"));

            Assert.Equal("лайков", wordUtils.FormatCaseWord(25, "лайков", "лайк", "лайка"));
            Assert.Equal("лайков", wordUtils.FormatCaseWord(125, "лайков", "лайк", "лайка"));
            Assert.Equal("лайков", wordUtils.FormatCaseWord(1025, "лайков", "лайк", "лайка"));
            Assert.Equal("лайков", wordUtils.FormatCaseWord(1125, "лайков", "лайк", "лайка"));
            Assert.Equal("лайков", wordUtils.FormatCaseWord(10025, "лайков", "лайк", "лайка"));
            Assert.Equal("лайков", wordUtils.FormatCaseWord(10125, "лайков", "лайк", "лайка"));
            Assert.Equal("лайков", wordUtils.FormatCaseWord(11125, "лайков", "лайк", "лайка"));
        }
    }
}
