using ChaChing.Application.Ports;
using ChaChing.Domain.Parties;

namespace ChaChing.Application.Tests.InMemoryAggregates
{
    public class InMemoryClientRepository : IClientRepository
    {
        private readonly List<Client> _clients = new();
        public IReadOnlyList<Client> Clients => _clients;
        public Task AddAsync(Client client, CancellationToken cancellationToken = default)
        {
            _clients.Add(client);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsForSellerAsync(Guid idClient, Guid idSeller, CancellationToken cancellationToken = default)
            => Task.FromResult(_clients.Any(x => x.Id == idClient && x.SellerId == idSeller));
    }
}
