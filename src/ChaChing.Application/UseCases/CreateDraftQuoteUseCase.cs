using ChaChing.Application.Commands;
using ChaChing.Application.Ports;
using ChaChing.Domain.Billing.Quotes;

namespace ChaChing.Application.UseCases
{
    public class CreateDraftQuoteUseCase
    {
        private readonly IQuoteRepository _quoteRepository;
        private readonly IClientRepository _clientRepository;

        public CreateDraftQuoteUseCase(IQuoteRepository quoteRepository, IClientRepository clientRepository)
        {
            _quoteRepository = quoteRepository;
            _clientRepository = clientRepository;
        }

        public async Task<Guid> CreateDraftQuoteAsync(CreateDraftQuoteCommand command, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            if (!await _clientRepository.ExistsForSellerAsync(command.ClientId, command.SellerId, cancellationToken))
                throw new ArgumentException("This client doesn't belong to this seller.", nameof(command.ClientId));

            Quote quote = Quote.CreateDraft(command.SellerId, command.ClientId, command.Currency, command.Discount);
            await _quoteRepository.AddAsync(quote, cancellationToken);

            return quote.Id;
        }
    }
}
