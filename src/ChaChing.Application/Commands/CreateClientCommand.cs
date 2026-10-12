using ChaChing.Domain.Common;

namespace ChaChing.Application.Commands
{
    public record CreateClientCommand(Guid SellerId, string Name, string Email, Address Address, string? Siren = null);
}