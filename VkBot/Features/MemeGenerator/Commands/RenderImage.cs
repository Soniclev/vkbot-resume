using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using VkBot.Features.Core.Images;

namespace VkBot.Features.MemeGenerator.Commands
{
    public class RenderImage : MemeCommand
    {
        private readonly IServiceProvider _serviceProvider;

        public RenderImage(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void DoCommand(IImageRender imageRender)
        {
            var imagePath = Path.Combine(_directory, _args["@main_argument"]);
            var image = Image.FromFile(imagePath);
            var x = int.Parse(_args["x"]);
            var y = int.Parse(_args["y"]);
            var w = int.Parse(_args["w"]);
            var h = int.Parse(_args["h"]);
            imageRender.DrawImageScaled(image, x, y, w, h);
        }

        public override string GetCommandName()
        {
            return "RenderImage";
        }
    }
}
