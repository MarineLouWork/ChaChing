using ChaChing.Domain.Parties;

namespace ChaChing.Application.Ports
{
    public interface IClientRepository
    {
        Task AddAsync(Client client, CancellationToken cancellationToken = default);
        Task<bool> ExistsForSellerAsync(Guid idClient, Guid idSeller, CancellationToken cancellationToken = default);
    }
}