using ChaChing.Domain.Common;
using ChaChing.Domain.Parties;

namespace ChaChing.Application.Commands
{
    public record SetUpSellerCommand(string LegalName, Address Address, string Siret, VatRegime VatRegime, string? VatNumber = null);
}