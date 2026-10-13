using ChaChing.Application.Ports;
using ChaChing.Domain.Billing.Quotes;

namespace ChaChing.Application.Tests.InMemoryAggregates
{
    public class InMemoryQuoteRepository : IQuoteRepository
    {
        private readonly List<Quote> _quotes = new();
        public IReadOnlyList<Quote> Quotes => _quotes;

        public Task AddAsync(Quote quote, CancellationToken cancellationToken = default)
        {
            _quotes.Add(quote);
            return Task.CompletedTask;
        }
    }
}
