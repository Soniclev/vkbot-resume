using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Core;
using VkBot.Core.Rest;
using VkBot.Core.Structures;
using VkBot.Features.MemeGenerator;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;

namespace VkBot.Group.Bot.Updates.Messages.Routes
{
    public class MemeGeneratorManager : IMessageRoute
    {
        private enum State
        {
            Begin,
            Began,
            Sent,
            InvalidSelected
        }

        private readonly string MemeText = "Мемы";
        private const string BackwardText = "Назад";
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<long, IStateMachine<State, Message>> _stateMachines = new Dictionary<long, IStateMachine<State, Message>>();

        public MemeGeneratorManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private IStateMachine<State, Message> CreateStateMachine()
        {
            var machine = new StateMachine<State, Message>();
            machine.AddState(State.Began, (message) =>
            {
                var userId = message.PeerId ?? 0;
                var groupApi = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
                var randomManager = _serviceProvider.GetService<IRandomDataProvider>();
                var stringBuilder = new StringBuilder("Напиши номер мема:\n");
                var memeGenerator = _serviceProvider.GetService<IMemeGenerator>();
                var memes = memeGenerator.GetAvailableMemes();
                for (var i = 0; i < memes.Count(); i++)
                {
                    stringBuilder.AppendLine($"{i + 1}) {memes.ElementAt(i).Name}");
                }
                var keyboard = new KeyboardBuilder(true).AddButton(BackwardText, "", KeyboardButtonColor.Default).Build();
                groupApi.Messages.Send(new MessagesSendParams()
                {
                    Message = stringBuilder.ToString(),
                    PeerId = message.PeerId,
                    Keyboard = keyboard,
                    RandomId = randomManager.GetInt()
                });
            });
            machine.AddState(State.Sent, message =>
            {
                var userId = message.PeerId ?? 0;
                var groupApi = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
                var groupId = _serviceProvider.GetService<IGroupApiProvider>().GetGroupId();
                var randomManager = _serviceProvider.GetService<IRandomDataProvider>();
                int.TryParse(message.Text, out var number);
                groupApi.Messages.SetActivity(null, MessageActivityType.Typing, message.PeerId, (ulong)-groupId);
                byte[] meme;
                using (var memeGenerator = _serviceProvider.GetService<IMemeGenerator>())
                {
                    var memes = memeGenerator.GetAvailableMemes();
                    var name = memes.ElementAt(number - 1).Name;
                    meme = memeGenerator.Generate(name, userId);
                }

                var upload = groupApi.Photo.GetMessagesUploadServer(userId);

                var restClient = _serviceProvider.GetService<IRestClient>();
                var uploadResponse = restClient.UploadImage(upload.UploadUrl, meme, "png");

                var photos = groupApi.Photo.SaveMessagesPhoto(uploadResponse);
                var keyboard = new KeyboardBuilder(true).AddButton(BackwardText, "", KeyboardButtonColor.Default).Build();
                groupApi.Messages.Send(new MessagesSendParams()
                {
                    Attachments = photos,
                    Message = $"Держи",
                    PeerId = message.PeerId,
                    Keyboard = keyboard,
                    RandomId = randomManager.GetInt()
                });
            });
            machine.AddState(State.InvalidSelected, message =>
            {
                var groupApi = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
                var randomManager = _serviceProvider.GetService<IRandomDataProvider>();
                var keyboard = new KeyboardBuilder(true).AddButton(BackwardText, "", KeyboardButtonColor.Default).Build();
                groupApi.Messages.Send(new MessagesSendParams
                {

                    Message = $"Неправильно. Наверное клава залагала",
                    PeerId = message.PeerId,
                    Keyboard = keyboard,
                    RandomId = randomManager.GetInt()
                });
            });
            machine.AddTransitCondition(State.Begin, State.Began, (message) =>
               message.Text == MemeText
            );
            machine.AddTransitCondition(State.Began, State.Sent, (message) =>
            {
                var memeGenerator = _serviceProvider.GetService<IMemeGenerator>();
                var memes = memeGenerator.GetAvailableMemes();
                return int.TryParse(message.Text, out var number) && number >= 1 && number <= memes.Count();
            });
            machine.AddTransitCondition(State.Began, State.InvalidSelected, (message) => {
                var memeGenerator = _serviceProvider.GetService<IMemeGenerator>();
                var memes = memeGenerator.GetAvailableMemes();
                return !int.TryParse(message.Text, out var number) || (number < 1 || number > memes.Count());
            });
            machine.AddTransitCondition(State.InvalidSelected, State.Began, (message) =>
                message.Text == BackwardText
            );
            machine.AddTransitCondition(State.Sent, State.Began, (message) =>
                message.Text == BackwardText
            );

            machine.SetState(State.Begin);

            return machine;
        }

        public MessageKeyboardButton GetKeyboardButton()
        {
            return new MessageKeyboardButton()
            {
                Action = new MessageKeyboardButtonAction()
                {
                    Label = MemeText
                },
                Color = KeyboardButtonColor.Default
            };
        }

        public bool IsAcceptable(Message message)
        {
            var userId = message.PeerId ?? 0;
            if (_stateMachines.ContainsKey(userId))
            {
                var machine = _stateMachines[userId];
                return !(machine.GetState() == State.Began && message.Text == BackwardText);
            }
            else if (message.Text == MemeText)
                return true;
            return false;
        }

        public void ProccessMessage(Message message)
        {
            var userId = message.PeerId ?? 0;

            if (!_stateMachines.ContainsKey(userId))
            {
                _stateMachines[userId] = CreateStateMachine();
            }

            var machine = _stateMachines[userId];
            machine.ProceedNextState(message);
            machine.PerformCurrentStateAction(message);
        }

        public void ResetUserContext(long userId)
        {
            if (_stateMachines.ContainsKey(userId))
                _stateMachines.Remove(userId);
        }
    }
}
