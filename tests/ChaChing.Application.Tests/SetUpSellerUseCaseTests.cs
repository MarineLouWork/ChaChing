using ChaChing.Application.Commands;
using ChaChing.Application.Tests.Repositories;
using ChaChing.Application.UseCases;
using ChaChing.Domain.Common;
using ChaChing.Domain.Parties;

namespace ChaChing.Application.Tests
{
    public class SetUpSellerUseCaseTests
    {
        private readonly InMemorySellerRepository _repository;
        private readonly SetUpSellerUseCase _useCase;

        public SetUpSellerUseCaseTests()
        {
            _repository = new InMemorySellerRepository();
            _useCase = new SetUpSellerUseCase(_repository);
        }

        SetUpSellerCommand command = new("Throne Industry", new Address("Castle Rock", "33033", "South", "GT"), "12345678901010", VatRegime.Subject, "FR12345678901");

        [Fact]
        public void SetUpSeller_ValidData_Success()
        {
            Guid idSeller = _useCase.SetUpSeller(command);

            Assert.NotEqual(Guid.Empty, idSeller);
            Assert.Single(_repository.Sellers);
            Assert.Equal(_repository.Sellers.First().Id, idSeller);
            Assert.Equal(_repository.Sellers.First().LegalName, command.LegalName);
        }

        [Fact]
        public void SetUpSeller_InvalidSiret_ThrowsAndStoresNothing()
        {
            SetUpSellerCommand invalid = command with { Siret = "1234567890123" };

            var action = () => { _useCase.SetUpSeller(invalid); };

            ArgumentException exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("siret", exception.ParamName);
            Assert.Empty(_repository.Sellers);
        }

        [Fact]
        public void SetUpSeller_NullCommand_Throws()
        {
            var action = () => { _useCase.SetUpSeller(null!); };

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("command", exception.ParamName);
        }
    }
}
