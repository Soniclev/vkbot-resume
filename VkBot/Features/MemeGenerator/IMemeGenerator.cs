using System;
using System.Collections.Generic;
using System.Text;

namespace VkBot.Features.MemeGenerator
{
    public interface IMemeGenerator : IDisposable
    {
        byte[] Generate(string memeName, long userId);
        IEnumerable<Meme> GetAvailableMemes();
    }
}
