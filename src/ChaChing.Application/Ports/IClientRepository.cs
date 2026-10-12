using ChaChing.Domain.Parties;

namespace ChaChing.Application.Ports
{
    public interface IClientRepository
    {
        void Add(Client client);
    }
}