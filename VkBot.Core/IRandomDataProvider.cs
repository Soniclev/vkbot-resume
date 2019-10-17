using System.Collections.Generic;

namespace VkBot.Core
{
    public interface IRandomDataProvider
    {
        int GetInt();
        int GetInt(int max);
        int GetInt(int min, int max);

        long GetLong();
        long GetLong(long max);
        long GetLong(long min, long max);

        double GetDouble();

        T SelectElement<T>(IEnumerable<T> enumerable);
        IEnumerable<T> SelectElements<T>(IEnumerable<T> enumerable, int count);
    }
}
