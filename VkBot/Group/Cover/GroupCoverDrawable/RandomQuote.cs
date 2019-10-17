using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Core;

namespace VkBot.Group.Cover.GroupCoverDrawable
{
    internal class RandomQuote : IGroupCoverDrawable
    {
        private string _quote;
        private DateTime? _lastQuoteUpdateTime;
        private readonly TimeSpan _quoteUpdateInterval = TimeSpan.FromMinutes(5);
        private readonly IServiceProvider _serviceProvider;

        public RandomQuote(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Draw(IGroupCoverRenderer groupCover)
        {
            groupCover.DrawQuote(_quote);
        }

        public Task Precache()
        {
            return Task.Run(() => _quote = GetQuote());
        }

        private string GetQuote()
        {
            if (_lastQuoteUpdateTime == null || _lastQuoteUpdateTime?.Add(_quoteUpdateInterval) <= DateTime.Now)
            {
                _lastQuoteUpdateTime = DateTime.Now;
                return GetRandomQuote();
            }

            return _quote;
        }

        private string GetRandomQuote()
        {
            var quotes = File.ReadAllLines("Data/Quotes.txt").Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var randomProvider = _serviceProvider.GetService<IRandomDataProvider>();
            return randomProvider.SelectElement(quotes);
        }

        public async Task DrawAsync(IGroupCoverRenderer groupCover)
        {
            await Task.Run(() => Draw(groupCover)).ConfigureAwait(false);
        }
    }
}
