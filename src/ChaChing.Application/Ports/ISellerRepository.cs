using ChaChing.Domain.Parties;

namespace ChaChing.Application.Ports
{
    public interface ISellerRepository
    {
        void Add(Seller seller);
        bool Exists(Guid id);
    }
}