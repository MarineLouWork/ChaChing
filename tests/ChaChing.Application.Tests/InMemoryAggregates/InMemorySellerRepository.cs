using ChaChing.Application.Ports;
using ChaChing.Domain.Parties;

namespace ChaChing.Application.Tests.InMemoryAggregates
{
    public class InMemorySellerRepository : ISellerRepository
    {
        private readonly List<Seller> _sellers = new();
        public IReadOnlyList<Seller> Sellers => _sellers;
        public void Add(Seller seller) => _sellers.Add(seller);

        public bool Exists(Guid id)
        {
            if (Sellers.Any(x => x.Id == id))
                return true;
            else
                return false;
        }
    }
}
