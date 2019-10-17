using VkNet.Model;

namespace VkBot.Group.Bot.Updates.Messages
{
    interface IMessageRouter : INewMessage
    {
        void OnNewMessage(Message message);
    }
}
