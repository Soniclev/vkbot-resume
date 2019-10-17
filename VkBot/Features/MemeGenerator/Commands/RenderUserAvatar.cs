using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Core.Cache;
using VkBot.Features.Core.Images;
using VkBot.Users;

namespace VkBot.Features.MemeGenerator.Commands
{
    public class RenderUserAvatar : MemeCommand
    {
        private readonly IServiceProvider _serviceProvider;

        public RenderUserAvatar(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void DoCommand(IImageRender imageRender)
        {
            var cache = _serviceProvider.GetService<ICacheableUser>();
            var userAvatar = cache.GetUserAvatar(_userId, true);
            var x = int.Parse(_args["x"]);
            var y = int.Parse(_args["y"]);
            var w = int.Parse(_args["w"]);
            var h = int.Parse(_args["h"]);
            imageRender.DrawImageScaled(userAvatar, x, y, w, h);
        }

        public override string GetCommandName()
        {
            return "RenderUserAvatar";
        }
    }
}
