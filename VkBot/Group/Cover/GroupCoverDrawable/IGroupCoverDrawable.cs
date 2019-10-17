using System.Threading.Tasks;

namespace VkBot.Group.Cover.GroupCoverDrawable
{
    internal interface IGroupCoverDrawable
    {
        Task Precache();
        void Draw(IGroupCoverRenderer groupCover);
        Task DrawAsync(IGroupCoverRenderer groupCover);
    }
}
