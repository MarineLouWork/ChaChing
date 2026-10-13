using ChaChing.Application.Ports;
using ChaChing.Domain.Parties;

namespace ChaChing.Application.Tests.InMemoryAggregates
{
    public class InMemorySellerRepository : ISellerRepository
    {
        private readonly List<Seller> _sellers = new();
        public IReadOnlyList<Seller> Sellers => _sellers;
        public Task AddAsync(Seller seller, CancellationToken cancellationToken = default)
        {
            _sellers.Add(seller);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_sellers.Any(x => x.Id == id));

    }
}
