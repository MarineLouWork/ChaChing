using ChaChing.Domain.Billing.Quotes;

namespace ChaChing.Application.Ports
{
    public interface IQuoteRepository
    {
        Task AddAsync(Quote quote, CancellationToken cancellationToken = default);
    }
}
