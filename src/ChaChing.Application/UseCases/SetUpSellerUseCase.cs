using ChaChing.Application.Commands;
using ChaChing.Application.Ports;
using ChaChing.Domain.Parties;

namespace ChaChing.Application.UseCases
{
    public class SetUpSellerUseCase
    {
        private readonly ISellerRepository _repository;

        public SetUpSellerUseCase(ISellerRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> SetUpSellerAsync(SetUpSellerCommand command, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            Seller seller = Seller.Create(command.LegalName, command.Address, command.Siret, command.VatRegime, command.VatNumber);
            await _repository.AddAsync(seller, cancellationToken);
            return seller.Id;
        }
    }
}