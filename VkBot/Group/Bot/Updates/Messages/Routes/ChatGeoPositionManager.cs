using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Core;
using VkBot.Core.Cache;
using VkBot.Core.Structures;
using VkBot.Information.User;
using VkBot.Information.User.Models;
using VkBot.Users;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;

namespace VkBot.Group.Bot.Updates.Messages.Routes
{
    internal class ChatGeoPositionManager : IMessageRoute
    {
        private enum State
        {
            Begin,
            Began,
            PrintPeopleGeo,
            EnterGeo,
            GeoEntered,
            GeoCommentaryEntered,
            DeleteGeo,
            GeoDeleted
        }

        private const string KeyBoardText = "Местоположение";
        private const string BackwardText = "Назад";
        private const string YesText = "Да";
        private const string NoText = "Нет";
        private const string DoneText = "Готово";
        private const string WhoIsWhereText = "Кто где?";
        private const string FillPlaceText = "Указать своё местоположение";
        private const string DeletePlaceText = "Удалить своё местоположение";
        private readonly Dictionary<long, GeoPosition> _contextData = new Dictionary<long, GeoPosition>();
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<long, IStateMachine<State, Message>> _stateMachines = new Dictionary<long, IStateMachine<State, Message>>();


        public ChatGeoPositionManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private IStateMachine<State, Message> CreateStateMachine()
        {
            var machine = new StateMachine<State, Message>();

            machine.AddState(State.Began, message =>
            {
                var userId = message.PeerId ?? 0;
                var groupApi = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
                var randomManager = _serviceProvider.GetService<IRandomDataProvider>();
                _contextData[userId] = new GeoPosition
                {
                    UserId = userId
                };
                var keyboard = new KeyboardBuilder()
                    .AddButton(WhoIsWhereText, "", KeyboardButtonColor.Default)
                    .AddLine()
                    .AddButton(FillPlaceText, "", KeyboardButtonColor.Default)
                    .AddLine()
                    .AddButton(DeletePlaceText, "", KeyboardButtonColor.Default)
                    .AddLine()
                    .AddButton(BackwardText, "", KeyboardButtonColor.Default)
                    .Build();
                groupApi.Messages.Send(new MessagesSendParams
                {
                    Message = $"Выбери действие",
                    PeerId = message.PeerId,
                    Keyboard = keyboard,
                    RandomId = randomManager.GetInt()
                });
            });

            machine.AddState(State.PrintPeopleGeo, message =>
            {
                var groupApi = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
                var geoPositionManager = _serviceProvider.GetService<IGeoPositionManager>();
                var nickManager = _serviceProvider.GetService<INicknameManager>();
                var randomManager = _serviceProvider.GetService<IRandomDataProvider>();
                var geos = geoPositionManager.GetAll();
                var keyboard = new KeyboardBuilder().AddButton(BackwardText, "", KeyboardButtonColor.Default).Build();
                if (geos.Count == 0)
                {
                    groupApi.Messages.Send(new MessagesSendParams
                    {
                        Message = "Никто ещё не указал своё местоположение",
                        PeerId = message.PeerId,
                        Keyboard = keyboard,
                        RandomId = randomManager.GetInt()
                    });
                    return;
                }
                var cacheableUser = _serviceProvider.GetService<ICacheableUser>();
                var stringBuilder = new StringBuilder();
                for (var i = 0; i < geos.Count; i++)
                {
                    var geoPosition = geos[i];
                    var user = geoPosition.UserId;
                    if (nickManager.Exists(user))
                    {
                        stringBuilder.AppendLine(nickManager.GetNickname(user));
                    }
                    else
                    {
                        var cacheUser = cacheableUser.GetUser(user, false);
                        stringBuilder.Append(cacheUser.FirstName).Append(" ").AppendLine(cacheUser.LastName);
                    }

                    stringBuilder.Append("Место: ").AppendLine(geoPosition.Place);
                    if (!string.IsNullOrWhiteSpace(geoPosition.Commentary))
                        stringBuilder.Append("Комментарий: ").AppendLine(geoPosition.Commentary);

                    stringBuilder.Append("Последнее обновление: ").AppendFormat("{0:HH:mm dd.MM}", geoPosition.UpdateTime).AppendLine();

                    if (i != geos.Count - 1)
                        stringBuilder.AppendLine();
                }
                groupApi.Messages.Send(new MessagesSendParams
                {
                    Message = stringBuilder.ToString(),
                    PeerId = message.PeerId,
                    Keyboard = keyboard,
                    RandomId = randomManager.GetInt()
                });
            });

            machine.AddState(State.EnterGeo, message =>
            {
                var groupApi = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
                var randomManager = _serviceProvider.GetService<IRandomDataProvider>();
                var keyboard = new KeyboardBuilder().AddButton(BackwardText, "", KeyboardButtonColor.Default).Build();
                groupApi.Messages.Send(new MessagesSendParams
                {
                    Message =
                        $"Для осведомления других, ты можешь указать своё текущее местоположение. Это может быть Должа, хата Юсупа, стадион, Буяны и т.д.",
                    PeerId = message.PeerId,
                    Keyboard = keyboard,
                    RandomId = randomManager.GetInt()
                });
            });

            machine.AddState(State.GeoEntered, message =>
            {
                var userId = message.PeerId ?? 0;
                var groupApi = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
                var geoPositionManager = _serviceProvider.GetService<IGeoPositionManager>();
                var randomManager = _serviceProvider.GetService<IRandomDataProvider>();
                _contextData[userId].Place = message.Text;
                geoPositionManager.Set(_contextData[userId]);
                var keyboard = new KeyboardBuilder().AddButton(DoneText, "", KeyboardButtonColor.Default).Build();
                groupApi.Messages.Send(new MessagesSendParams
                {
                    Message = $"Принято. Можно нажать готово. Либо можно дополнительно указать комментарий, например: буду в Долже до понедельника, приеду в Должу на выходных",
                    PeerId = message.PeerId,
                    Keyboard = keyboard,
                    RandomId = randomManager.GetInt()
                });
            });

            machine.AddState(State.GeoCommentaryEntered, message =>
            {
                var userId = message.PeerId ?? 0;
                var groupApi = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
                var geoPositionManager = _serviceProvider.GetService<IGeoPositionManager>();
                var randomManager = _serviceProvider.GetService<IRandomDataProvider>();
                _contextData[userId].Commentary = message.Text;
                geoPositionManager.Set(_contextData[userId]);
                var keyboard = new KeyboardBuilder().AddButton(DoneText, "", KeyboardButtonColor.Default).Build();
                groupApi.Messages.Send(new MessagesSendParams
                {
                    Message = $"Принято.",
                    PeerId = message.PeerId,
                    Keyboard = keyboard,
                    RandomId = randomManager.GetInt()
                });
            });

            machine.AddState(State.DeleteGeo, message =>
            {
                var groupApi = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
                var randomManager = _serviceProvider.GetService<IRandomDataProvider>();
                var keyboard = new KeyboardBuilder()
                    .AddButton(YesText, "", KeyboardButtonColor.Negative)
                    .AddButton(NoText, "", KeyboardButtonColor.Default)
                    .Build();
                groupApi.Messages.Send(new MessagesSendParams
                {
                    Message = "Удалить местоположение?",
                    PeerId = message.PeerId,
                    Keyboard = keyboard,
                    RandomId = randomManager.GetInt()
                });
            });
            machine.AddState(State.GeoDeleted, message =>
            {
                var userId = message.PeerId ?? 0;
                var groupApi = _serviceProvider.GetService<IGroupApiProvider>().GetGroupApi();
                var geoPositionManager = _serviceProvider.GetService<IGeoPositionManager>();
                var randomManager = _serviceProvider.GetService<IRandomDataProvider>();
                var result = geoPositionManager.Remove(userId);
                var keyboard = new KeyboardBuilder().AddButton(DoneText, "", KeyboardButtonColor.Default).Build();
                groupApi.Messages.Send(new MessagesSendParams
                {
                    Message = result ? "Удалено" : "Твоего местоположения не указано, поэтому и удалять нечего",
                    PeerId = message.PeerId,
                    Keyboard = keyboard,
                    RandomId = randomManager.GetInt()
                });
            });

            machine.AddTransitCondition(State.Begin, State.Began, (message) =>
                message.Text == KeyBoardText
            );
            machine.AddTransitCondition(State.Began, State.PrintPeopleGeo, (message) =>
                message.Text == WhoIsWhereText
            );
            machine.AddTransitCondition(State.Began, State.EnterGeo, (message) =>
                message.Text == FillPlaceText
            );
            machine.AddTransitCondition(State.Began, State.DeleteGeo, (message) =>
                message.Text == DeletePlaceText
            );

            machine.AddTransitCondition(State.PrintPeopleGeo, State.Began, (message) =>
                message.Text == BackwardText
            );

            machine.AddTransitCondition(State.EnterGeo, State.Began, (message) =>
                message.Text == BackwardText
            );

            machine.AddTransitCondition(State.EnterGeo, State.GeoEntered, (message) =>
                message.Text != BackwardText
            );
            machine.AddTransitCondition(State.GeoEntered, State.Began, (message) =>
                message.Text == DoneText
            );

            machine.AddTransitCondition(State.GeoEntered, State.GeoCommentaryEntered, (message) =>
                message.Text != DoneText
            );

            machine.AddTransitCondition(State.GeoCommentaryEntered, State.Began, (message) =>
                message.Text == DoneText
            );

            machine.AddTransitCondition(State.DeleteGeo, State.GeoDeleted, (message) =>
                message.Text == YesText
            );
            machine.AddTransitCondition(State.DeleteGeo, State.Began, (message) =>
                message.Text == NoText
            );

            machine.AddTransitCondition(State.GeoDeleted, State.Began, (message) =>
                message.Text == DoneText
            );

            return machine;
        }

        public MessageKeyboardButton GetKeyboardButton()
        {
            return new MessageKeyboardButton
            {
                Action = new MessageKeyboardButtonAction
                {
                    Label = KeyBoardText
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
            else if (message.Text == KeyBoardText)
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
            if (_contextData.ContainsKey(userId))
                _contextData.Remove(userId);
            if (_stateMachines.ContainsKey(userId))
                _stateMachines.Remove(userId);
        }
    }
}
