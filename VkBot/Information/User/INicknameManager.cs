namespace VkBot.Information.User
{
    interface INicknameManager
    {
        bool Exists(long userId);
        void SetNickname(long userId, string nickname);
        string GetNickname(long userId);
        bool IsValid(string nickname);
    }
}
