using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Core.Cache;
using VkBot.Features.Core.Fonts;
using VkBot.Features.Core.Images;
using VkBot.Users;

namespace VkBot.Features.MemeGenerator.Commands
{
    public class RenderText : MemeCommand
    {
        private readonly IServiceProvider _serviceProvider;

        public RenderText(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void DoCommand(IImageRender imageRender)
        {
            var text = PreproccessText(_args["@main_argument"]);
            var fontSize = int.Parse(_args["fontSize"]);
            var font = _serviceProvider.GetService<IFontProvider>().GetFont("PF Agora Slab Pro Medium", FontSize.FromPixels(fontSize));
            var x = int.Parse(_args["x"]);
            var y = int.Parse(_args["y"]);
            var alignment = TextAlignment.Left;
            switch (_args["align"])
            {
                case "left":
                    alignment = TextAlignment.Left;
                    break;
                case "center":
                    alignment = TextAlignment.Center;
                    break;
                case "right":
                    alignment = TextAlignment.Right;
                    break;
            }
            imageRender.DrawString(text, font, Color.Black, x, y, alignment);
        }

        private string PreproccessText(string str)
        {
            var text = str;
            if (text.Contains("%first_name%"))
            {
                var cache = _serviceProvider.GetService<ICacheableUser>();
                var firstName = cache.GetUser(_userId).FirstName;
                text = text.Replace("%first_name%", firstName);
            }
            if (text.Contains("%last_name%"))
            {
                var cache = _serviceProvider.GetService<ICacheableUser>();
                var firstName = cache.GetUser(_userId).LastName;
                text = text.Replace("%last_name%", firstName);
            }

            return text;
        }

        public override string GetCommandName()
        {
            return "RenderText";
        }
    }
}
