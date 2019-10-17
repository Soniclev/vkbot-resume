using VkNet.Model;

namespace VkBot.Utils.VkNetExtensions
{
    public static class PhotoExtensions
    {
        public static string GetMaxPhotoUrl(this Previews previews)
        {
            if (previews.Photo400 != null)
                return previews.Photo400.AbsoluteUri;
            if (previews.Photo200 != null)
                return previews.Photo200.AbsoluteUri;
            if (previews.Photo130 != null)
                return previews.Photo130.AbsoluteUri;
            if (previews.Photo100 != null)
                return previews.Photo100.AbsoluteUri;
            if (previews.Photo50 != null)
                return previews.Photo50.AbsoluteUri;
            return previews.PhotoMax.AbsoluteUri;
        }
    }
}
