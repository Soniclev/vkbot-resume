using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using VkBot.Features.Core.Images;

namespace VkBot.Features.MemeGenerator.Commands
{
    public class CreateImageFrom : MemeCommand
    {
        private readonly IServiceProvider _serviceProvider;

        public CreateImageFrom(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void DoCommand(IImageRender imageRender)
        {
            var imagePath = Path.Combine(_directory, _args["@main_argument"]);
            var image = Image.FromFile(imagePath);
            imageRender.CreateFromImage(image);
        }

        public override string GetCommandName()
        {
            return "CreateFrom";
        }
    }
}
