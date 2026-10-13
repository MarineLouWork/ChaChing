using ChaChing.Application.Commands;
using ChaChing.Application.Tests.InMemoryAggregates;
using ChaChing.Application.UseCases;
using ChaChing.Domain.Billing;
using ChaChing.Domain.Billing.Quotes;
using ChaChing.Domain.Common;
using ChaChing.Domain.Parties;

namespace ChaChing.Application.Tests
{
    public class CreateDraftQuoteUseCaseTests
    {
        private readonly InMemoryQuoteRepository _quoteRepository;
        private readonly InMemoryClientRepository _clientRepository;
        private readonly CreateDraftQuoteUseCase _useCase;

        private readonly Guid _sellerId = Guid.NewGuid();

        public CreateDraftQuoteUseCaseTests()
        {
            _quoteRepository = new InMemoryQuoteRepository();
            _clientRepository = new InMemoryClientRepository();
            _useCase = new CreateDraftQuoteUseCase(_quoteRepository, _clientRepository);
        }

        private async Task<Client> AddClientAsync(Guid sellerId)
        {
            Client client = Client.Create(sellerId, "Arya Stark", "Stark@msn.fr", new Address("Winterfell Castle", "88088", "North", "GL"));
            await _clientRepository.AddAsync(client);
            return client;
        }

        [Fact]
        public async Task CreateDraftQuote_ClientOfTheSeller_StoresADraftQuote()
        {
            Client client = await AddClientAsync(_sellerId);
            PercentageDiscount discount = new(10m);
            CreateDraftQuoteCommand command = new(_sellerId, client.Id, Currency.EUR, discount);

            Guid idQuote = await _useCase.CreateDraftQuoteAsync(command);

            Quote stored = Assert.Single(_quoteRepository.Quotes);
            Assert.Equal(idQuote, stored.Id);
            Assert.Equal(QuoteStatus.Draft, stored.Status);
            Assert.Equal(_sellerId, stored.SellerId);
            Assert.Equal(client.Id, stored.ClientId);
            Assert.Equal(Currency.EUR, stored.Currency);
            Assert.Same(discount, stored.Discount);
            Assert.Empty(stored.Lines);
        }

        [Fact]
        public async Task CreateDraftQuote_UnknownClient_ThrowsAndStoresNothing()
        {
            await AddClientAsync(_sellerId);
            CreateDraftQuoteCommand command = new(_sellerId, Guid.NewGuid(), Currency.EUR);

            var action = async () => { await _useCase.CreateDraftQuoteAsync(command); };

            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(action);
            Assert.Equal(nameof(CreateDraftQuoteCommand.ClientId), exception.ParamName);
            Assert.Empty(_quoteRepository.Quotes);
        }

        [Fact]
        public async Task CreateDraftQuote_ClientOfAnotherSeller_ThrowsTheSameErrorAsUnknownClient()
        {
            Client client = await AddClientAsync(Guid.NewGuid());
            CreateDraftQuoteCommand command = new(_sellerId, client.Id, Currency.EUR);

            var action = async () => { await _useCase.CreateDraftQuoteAsync(command); };

            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(action);
            Assert.Equal(nameof(CreateDraftQuoteCommand.ClientId), exception.ParamName);
            Assert.Empty(_quoteRepository.Quotes);
        }

        [Fact]
        public async Task CreateDraftQuote_InvalidCurrency_ThrowsAndStoresNothing()
        {
            Client client = await AddClientAsync(_sellerId);
            CreateDraftQuoteCommand command = new(_sellerId, client.Id, (Currency)0);

            var action = async () => { await _useCase.CreateDraftQuoteAsync(command); };

            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(action);
            Assert.Equal("currency", exception.ParamName);
            Assert.Empty(_quoteRepository.Quotes);
        }

        [Fact]
        public async Task CreateDraftQuote_NullCommand_Throws()
        {
            var action = async () => { await _useCase.CreateDraftQuoteAsync(null!); };

            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(action);
            Assert.Equal("command", exception.ParamName);
        }
    }
}
