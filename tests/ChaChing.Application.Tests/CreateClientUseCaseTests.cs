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

        private async Task<Seller> AddSellerAsync()
        {
            Seller seller = Seller.Create("Throne Industry", new Address("Castle Rock", "33033", "South", "GT"), "12345678901010", VatRegime.Subject, "FROK123456789");
            await _sellerRepository.AddAsync(seller);
            return seller;
        }

        [Fact]
        public async Task CreateClient_ExistingSeller_AddsClientToRepository()
        {
            Seller seller = await AddSellerAsync();
            CreateClientCommand command = _command with { SellerId = seller.Id };

            Guid idClient = await _useCase.CreateClientAsync(command);

            Client stored = Assert.Single(_clientRepository.Clients);
            Assert.Equal(idClient, stored.Id);
            Assert.Equal(seller.Id, stored.SellerId);
            Assert.Equal(command.Name, stored.Name);
        }

        [Fact]
        public async Task CreateClient_UnknownSeller_ThrowsAndStoresNothing()
        {
            await AddSellerAsync();
            CreateClientCommand command = _command with { SellerId = Guid.NewGuid() };

            var action = async () => { await _useCase.CreateClientAsync(command); };

            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(action);
            Assert.Equal(nameof(CreateClientCommand.SellerId), exception.ParamName);
            Assert.Empty(_clientRepository.Clients);
        }

        [Fact]
        public async Task CreateClient_InvalidEmail_ThrowsAndStoresNothing()
        {
            Seller seller = await AddSellerAsync();
            CreateClientCommand command = _command with { SellerId = seller.Id, Email = "not-an-email" };

            var action = async () => { await _useCase.CreateClientAsync(command); };

            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(action);
            Assert.Equal("email", exception.ParamName);
            Assert.Empty(_clientRepository.Clients);
        }

        [Fact]
        public async Task CreateClient_NullCommand_Throws()
        {
            var action = async () => { await _useCase.CreateClientAsync(null!); };

            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(action);
            Assert.Equal("command", exception.ParamName);
        }
    }
}
