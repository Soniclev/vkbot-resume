using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VkBot.Core;
using VkBot.Group.Bot.Updates.Messages.Routes;
using VkNet.Model;
using VkNet.Model.GroupUpdate;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;

namespace VkBot.Group.Bot.Updates.Messages
{
    internal class BaseMessageRouter : IMessageRouter
    {
        private readonly Dictionary<long, IMessageRoute> _messageRoutes = new Dictionary<long, IMessageRoute>();
        private readonly List<IMessageRoute> _routes = new List<IMessageRoute>();
        private readonly ILogger<BaseMessageRouter> _logger;
        private readonly IServiceProvider _serviceProvider;

        public BaseMessageRouter(IServiceProvider serviceProvider)
        {
            _routes.Add(new ChatNicknameManager(serviceProvider));
            _routes.Add(new ChatGeoPositionManager(serviceProvider));
            _routes.Add(new MemeGeneratorManager(serviceProvider));
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetService<ILogger<BaseMessageRouter>>();
        }

        public string GetEventTypeCode()
        {
            return "message_new";
        }

        public void OnEvent(GroupUpdate update)
        {
            if (update.Message != null)
                OnNewMessage(update.Message);
        }

        public void OnNewMessage(Message message)
        {
            var peerId = message.PeerId ?? 0;
            _logger.LogInformation("Новое сообщение #{MessageId} \"{Message}\" от пользователя {PeerId}", message.Id, message.Text, peerId);
            if (_messageRoutes.ContainsKey(peerId))
            {
                var route = _messageRoutes[peerId];
                if (route.IsAcceptable(message))
                {
                    _logger.LogDebug("Обработка сообщения #{MessageId} в {RouteType}", message.Id, route.GetType().ToString());
                    route.ProccessMessage(message);
                }
                else
                {
                    _logger.LogDebug("Cброс состояния {RouteType} для сообщения #{MessageId}", route.GetType().ToString(), message.Id);
                    route.ResetUserContext(peerId);
                    _messageRoutes.Remove(peerId);
                    SendDefaultMessage(message.PeerId);
                }
                return;
            }

            var routes = _routes.Where(x => x.IsAcceptable(message)).ToList();

            if (routes.Count == 1)
            {
                var route = routes[0];
                _logger.LogDebug("Создание состояния {RouteType} для сообщения #{MessageId}", route.GetType().ToString(), message.Id);
                _messageRoutes[peerId] = route;
                route.ProccessMessage(message);
                return;
            }

            if (routes.Count >= 2)
            {
                _logger.LogWarning("Найдено два возможных ответа для сообщения!");
                OnUnknownMessage(message);
                return;
            }

            _logger.LogDebug("Отправка стандартного сообщения в ответ сообщению #{MessageId} пользователю {PeerId}", message.Id, peerId);
            SendDefaultMessage(message.PeerId);
        }

        private void SendDefaultMessage(long? peerId)
        {
            var groupApi = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
            var randomManager = _serviceProvider.GetService<IRandomDataProvider>();
            groupApi.Messages.Send(new MessagesSendParams()
            {
                Message = $"Вот доступные действия",
                PeerId = peerId,
                Keyboard = GetDefaultKeyboard(),
                RandomId = randomManager.GetInt()
            });
        }

        private MessageKeyboard GetDefaultKeyboard()
        {
            var buttons = new List<MessageKeyboardButton>();

            foreach (var route in _routes)
            {
                var button = route.GetKeyboardButton();
                if (button != null)
                    buttons.Add(button);
            }

            var rows = new List<List<MessageKeyboardButton>>();
            var row = new List<MessageKeyboardButton>();
            foreach (var button in buttons)
            {
                if (button != null)
                    row.Add(button);
                if (row.Count == 4)
                {
                    rows.Add(row);
                    row = new List<MessageKeyboardButton>();
                }
            }

            if (row.Count != 0)
            {
                rows.Add(row);
            }

            return new MessageKeyboard()
            {
                Buttons = rows
            };
        }

        private void OnUnknownMessage(Message message)
        {
            var groupApi = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
            var randomManager = _serviceProvider.GetService<IRandomDataProvider>();
            groupApi.Messages.Send(new MessagesSendParams()
            {
                Message = $"Ничего не понял шо ты написал",
                PeerId = message.PeerId,
                Keyboard = GetDefaultKeyboard(),
                RandomId = randomManager.GetInt()
            });
        }
    }
}
