using ChaChing.Application.Commands;
using ChaChing.Application.Tests.InMemoryAggregates;
using ChaChing.Application.UseCases;
using ChaChing.Domain.Common;
using ChaChing.Domain.Parties;

namespace ChaChing.Application.Tests
{
    public class CreateClientUseCaseTests
    {
        private readonly InMemoryClientRepository _clientRepository;
        private readonly InMemorySellerRepository _sellerRepository;
        private readonly CreateClientUseCase _useCase;

        public CreateClientUseCaseTests()
        {
            _clientRepository = new InMemoryClientRepository();
            _sellerRepository = new InMemorySellerRepository();
            _useCase = new CreateClientUseCase(_clientRepository, _sellerRepository);
        }

        private readonly CreateClientCommand _command = new(Guid.NewGuid(), "Arya Stark", "Stark@msn.fr", new Address("Winterfell Castle", "88088", "North", "GL"), "123456789");

        private Seller AddSeller()
        {
            Seller seller = Seller.Create("Throne Industry", new Address("Castle Rock", "33033", "South", "GT"), "12345678901010", VatRegime.Subject, "FROK123456789");
            _sellerRepository.Add(seller);
            return seller;
        }

        [Fact]
        public void CreateClient_ExistingSeller_AddsClientToRepository()
        {
            Seller seller = AddSeller();
            CreateClientCommand command = _command with { SellerId = seller.Id };

            Guid idClient = _useCase.CreateClient(command);

            Client stored = Assert.Single(_clientRepository.Clients);
            Assert.Equal(idClient, stored.Id);
            Assert.Equal(seller.Id, stored.SellerId);
            Assert.Equal(command.Name, stored.Name);
        }

        [Fact]
        public void CreateClient_UnknownSeller_ThrowsAndStoresNothing()
        {
            AddSeller();
            CreateClientCommand command = _command with { SellerId = Guid.NewGuid() };

            var action = () => { _useCase.CreateClient(command); };

            ArgumentException exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal(nameof(CreateClientCommand.SellerId), exception.ParamName);
            Assert.Empty(_clientRepository.Clients);
        }

        [Fact]
        public void CreateClient_InvalidEmail_ThrowsAndStoresNothing()
        {
            Seller seller = AddSeller();
            CreateClientCommand command = _command with { SellerId = seller.Id, Email = "not-an-email" };

            var action = () => { _useCase.CreateClient(command); };

            ArgumentException exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("email", exception.ParamName);
            Assert.Empty(_clientRepository.Clients);
        }

        [Fact]
        public void CreateClient_NullCommand_Throws()
        {
            var action = () => { _useCase.CreateClient(null!); };

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("command", exception.ParamName);
        }
    }
}
