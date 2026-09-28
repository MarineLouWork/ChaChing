using ChaChing.Domain.Billing.Taxes;
using ChaChing.Domain.Clients;
using ChaChing.Domain.Common;
using ChaChing.Domain.Sellers;

namespace ChaChing.Domain.Tests
{
    /// <summary>Valid objects shared by the tests, built fresh on every call.</summary>
    internal static class TestData
    {
        public static Address Address() => new("12 rue de la Paix", "13001", "Marseille", "FR");

        public static Seller Seller(VatRegime vatRegime = VatRegime.Subject)
            => Domain.Sellers.Seller.Create("Arya Stark Design", Address(), "12345678900012", vatRegime);

        public static Client Client(Guid sellerId)
            => Domain.Clients.Client.Create(sellerId, "Winterfell SAS", "contact@winterfell.fr", Address(), "123456789");
    }
}
