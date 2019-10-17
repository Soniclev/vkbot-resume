using VkNet.Model;
using VkNet.Model.Keyboard;

namespace VkBot.Group.Bot.Updates.Messages
{
    interface IMessageRoute
    {
        bool IsAcceptable(Message message);
        void ProccessMessage(Message message);
        void ResetUserContext(long userId);
        MessageKeyboardButton GetKeyboardButton();
    }
}
