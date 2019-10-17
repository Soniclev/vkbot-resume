using System;
using System.Collections.Generic;
using System.Text;
using VkBot.Features.Core.Images;

namespace VkBot.Features.MemeGenerator
{
    public interface IMemeCommand
    {
        string GetCommandName();
        void DoCommand(IImageRender imageRender);
        void Precache(long userId);
        void SetArgs(Dictionary<string, string> args);
        void SetMemeDirectory(string path);
    }
}
