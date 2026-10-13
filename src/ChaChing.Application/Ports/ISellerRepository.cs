using ChaChing.Domain.Parties;

namespace ChaChing.Application.Ports
{
    public interface ISellerRepository
    {
        Task AddAsync(Seller seller, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}