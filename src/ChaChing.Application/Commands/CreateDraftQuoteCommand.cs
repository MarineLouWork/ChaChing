using ChaChing.Domain.Billing;
using ChaChing.Domain.Common;

namespace ChaChing.Application.Commands
{
    public record CreateDraftQuoteCommand(Guid SellerId, Guid ClientId, Currency Currency, Discount? Discount = null);
}
