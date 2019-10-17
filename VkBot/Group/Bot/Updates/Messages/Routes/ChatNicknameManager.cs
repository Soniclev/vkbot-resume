using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Core;
using VkBot.Core.Structures;
using VkBot.Information.User;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;

namespace VkBot.Group.Bot.Updates.Messages.Routes
{
    public class ChatNicknameManager : IMessageRoute
    {
        private enum State
        {
            Begin,
            Began,
            NickTyped,
            WrongNickTyped
        }

        private const string TypeNickTitle = "Задать ник";
        private const string BackwardText = "Назад";
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<long, IStateMachine<State, Message>> _stateMachines = new Dictionary<long, IStateMachine<State, Message>>();

        public ChatNicknameManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private IStateMachine<State, Message> CreateStateMachine()
        {
            var machine = new StateMachine<State, Message>();
            machine.AddState(State.Began, message =>
            {
                var groupApi = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
                var randomManager = _serviceProvider.GetService<IRandomDataProvider>();
                var keyboard = new KeyboardBuilder().AddButton(BackwardText, "", KeyboardButtonColor.Default).Build();
                groupApi.Messages.Send(new MessagesSendParams()
                {
                    Message = "Отправь мне сообщением свой ник. Его можно будет сменить в любой момент.",
                    PeerId = message.PeerId,
                    RandomId = randomManager.GetInt(),
                    Keyboard = keyboard
                });
            });
            machine.AddState(State.NickTyped, message =>
            {
                var nick = message.Text;
                var userId = message.PeerId ?? 0;
                var groupApi = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
                var nicknameManager = _serviceProvider.GetService<INicknameManager>();
                var randomManager = _serviceProvider.GetService<IRandomDataProvider>();
                nicknameManager.SetNickname(userId, nick);
                var keyboard = new KeyboardBuilder().AddButton(BackwardText, "", KeyboardButtonColor.Default).Build();
                groupApi.Messages.Send(new MessagesSendParams()
                {
                    Message = $"Хорошо {nick}. Теперь это имя будет использоваться в группе",
                    PeerId = message.PeerId,
                    Keyboard = keyboard,
                    RandomId = randomManager.GetInt()
                });
            });
            machine.AddState(State.WrongNickTyped, message =>
            {
                var groupApi = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
                var randomManager = _serviceProvider.GetService<IRandomDataProvider>();
                var keyboard = new KeyboardBuilder().AddButton(BackwardText, "", KeyboardButtonColor.Default).Build();
                groupApi.Messages.Send(new MessagesSendParams()
                {
                    Message = $"Недопустимый ник!",
                    PeerId = message.PeerId,
                    Keyboard = keyboard,
                    RandomId = randomManager.GetInt()
                });
            });
            machine.AddTransitCondition(State.Begin, State.Began, message => message.Text == TypeNickTitle);
            machine.AddTransitCondition(State.Began, State.NickTyped, message =>
            {
                var nicknameManager = _serviceProvider.GetService<INicknameManager>();
                return nicknameManager.IsValid(message.Text);
            });
            machine.AddTransitCondition(State.Began, State.WrongNickTyped, message =>
            {
                var nicknameManager = _serviceProvider.GetService<INicknameManager>();
                return !nicknameManager.IsValid(message.Text);
            });
            machine.AddTransitCondition(State.WrongNickTyped, State.Began, message => message.Text == BackwardText);

            return machine;
        }

        public bool IsAcceptable(Message message)
        {
            var userId = message.PeerId ?? 0;
            if (_stateMachines.ContainsKey(userId))
            {
                var machine = _stateMachines[userId];
                if (machine.GetState() == State.NickTyped)
                    return false;
                return !(machine.GetState() == State.Began && message.Text == BackwardText);
            }
            else if (message.Text == TypeNickTitle)
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

        public MessageKeyboardButton GetKeyboardButton()
        {
            return new MessageKeyboardButton()
            {
                Action = new MessageKeyboardButtonAction()
                {
                    Label = TypeNickTitle
                },
                Color = KeyboardButtonColor.Default
            };
        }
    }
}
