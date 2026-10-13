using ChaChing.Application.Ports;
using ChaChing.Domain.Parties;

namespace ChaChing.Application.Tests.InMemoryAggregates
{
    public class InMemoryClientRepository : IClientRepository
    {
        private readonly List<Client> _clients = new();
        public IReadOnlyList<Client> Clients => _clients;
        public void Add(Client client) => _clients.Add(client);
    }
}
