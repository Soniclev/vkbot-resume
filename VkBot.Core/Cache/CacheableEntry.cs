using System;
using System.Collections.Generic;
using System.Text;

namespace VkBot.Core.Cache
{
    public class CacheableEntry<T>
    {
        private T _entry;
        private DateTime _lastUseTime = DateTime.Now;

        public T Entry
        {
            get
            {
                _lastUseTime = DateTime.Now;
                return _entry;
            }
            set => _entry = value;
        }

        public CacheableEntry()
        {
        }

        public CacheableEntry(T entry)
        {
            Entry = entry;
        }

        public DateTime GetLastUseTime() => _lastUseTime;

        public bool IsOld(TimeSpan lifeTime)
        {
            return GetLastUseTime().Add(lifeTime) < DateTime.Now;
        }
    }
}
