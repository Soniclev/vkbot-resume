namespace VkBot.Core
{
    public class WordUtils : IWordUtils
    {
        public string FormatCaseWord(int number, string zero, string one, string two)
        {
            number %= 100;
            if (number > 19)
                number %= 10;

            if (number == 1)
                return one;
            if (number >= 2 && number <= 4)
                return two;
            return zero;
        }
    }
}
