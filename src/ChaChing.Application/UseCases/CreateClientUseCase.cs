using ChaChing.Application.Commands;
using ChaChing.Application.Ports;
using ChaChing.Domain.Parties;

namespace ChaChing.Application.UseCases
{
    public class CreateClientUseCase
    {
        private readonly IClientRepository _clientRepository;
        private readonly ISellerRepository _sellerRepository;

        public CreateClientUseCase(IClientRepository clientRepository, ISellerRepository sellerRepository)
        {
            _clientRepository = clientRepository;
            _sellerRepository = sellerRepository;
        }

        public Guid CreateClient(CreateClientCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            if (!_sellerRepository.Exists(command.SellerId))
                throw new ArgumentException("The seller doesn't exist.", nameof(command.SellerId));

            Client client = Client.Create(command.SellerId, command.Name, command.Email, command.Address, command.Siren);
            _clientRepository.Add(client);
            return client.Id;
        }
    }
}