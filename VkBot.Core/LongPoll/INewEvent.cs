using VkNet.Model.GroupUpdate;

namespace VkBot.Core.LongPoll
{
    public interface INewEvent
    {
        void OnEvent(GroupUpdate update);
        string GetEventTypeCode();
    }
}
