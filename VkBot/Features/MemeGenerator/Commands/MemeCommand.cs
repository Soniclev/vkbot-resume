using System;
using System.Collections.Generic;
using System.Text;
using VkBot.Features.Core.Images;

namespace VkBot.Features.MemeGenerator.Commands
{
    public abstract class MemeCommand : IMemeCommand
    {
        protected Dictionary<string, string> _args;
        protected long _userId;
        protected string _directory;

        public abstract void DoCommand(IImageRender imageRender);

        public abstract string GetCommandName();

        public void Precache(long userId)
        {
            _userId = userId;
        }

        public void SetArgs(Dictionary<string, string> args)
        {
            _args = args;
        }

        public void SetMemeDirectory(string path)
        {
            _directory = path;
        }
    }
}
