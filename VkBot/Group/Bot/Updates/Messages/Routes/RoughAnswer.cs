using System;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Core;
using VkNet.Model.GroupUpdate;
using VkNet.Model.RequestParams;

namespace VkBot.Group.Bot.Updates.Messages.Routes
{
    internal class RoughAnswer : INewMessage
    {
        private static readonly string[] Messages = { "1", "2", "3", "4" };
        private readonly IServiceProvider _serviceProvider;

        public RoughAnswer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void OnEvent(GroupUpdate update)
        {
            var api = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
            var randomProvider = _serviceProvider.GetService<IRandomDataProvider>();
            var message = update.Message;
            api.Messages.Send(new MessagesSendParams()
            {
                Message = GetMessage(),
                PeerId = message.PeerId,
                ChatId = message.ChatId,
                UserId = message.UserId,
                RandomId = randomProvider.GetInt()
            });
            Console.WriteLine($"{message.Body} from {message.UserId}");
        }

        private string GetMessage()
        {
            var randomProvider = _serviceProvider.GetService<IRandomDataProvider>();
            return randomProvider.SelectElement(Messages);
        }

        public string GetEventTypeCode()
        {
            return "message_new";
        }
    }
}
