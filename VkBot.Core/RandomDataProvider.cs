using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VkBot.Core
{
    public class RandomDataProvider : IRandomDataProvider
    {
        private readonly Random _random = new Random();

        public int GetInt()
        {
            return _random.Next();
        }

        public int GetInt(int max)
        {
            return _random.Next(max);
        }

        public int GetInt(int min, int max)
        {
            return _random.Next(min, max);
        }

        public double GetDouble()
        {
            return _random.NextDouble();
        }

        public long GetLong()
        {
            return ((long)_random.Next() << 32) | (long)_random.Next();
        }

        public long GetLong(long max)
        {
            return GetLong() % max;
        }

        public long GetLong(long min, long max)
        {
            return min + GetLong(max - min);
        }

        public T SelectElement<T>(IEnumerable<T> enumerable)
        {
            var count = enumerable.Count();
            var index = GetInt(count);
            return enumerable.ElementAt(index);
        }

        public IEnumerable<T> SelectElements<T>(IEnumerable<T> enumerable, int count)
        {
            if (count > enumerable.Count() || count < 0)
                throw new ArgumentException(nameof(count));

            if (count == enumerable.Count())
                return enumerable;

            var indexes = new List<int>(count);
            var result = new List<T>(count);

            if (count == 0)
                return result;

            while (indexes.Count != count)
            {
                var index = GetInt(enumerable.Count());
                if (indexes.Contains(index))
                    continue;
                result.Add(enumerable.ElementAt(index));
                indexes.Add(index);
            }

            return result;
        }
    }
}
