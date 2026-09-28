using ChaChing.Domain.Billing.Taxes;
using ChaChing.Domain.Common;
using ChaChing.Domain.Sellers;

namespace ChaChing.Domain.Billing.Parties
{
    /// <summary>
    /// The seller's details frozen on the day a document is sent (ADR 0012).
    /// Built only from a <see cref="Seller"/>: it cannot hold details that never existed.
    /// </summary>
    public sealed record SellerSnapshot
    {
        private SellerSnapshot(Guid sellerId, string legalName, Address address, string siret, string? vatNumber, VatRegime vatRegime)
        {
            SellerId = sellerId;
            LegalName = legalName;
            Address = address;
            Siret = siret;
            VatNumber = vatNumber;
            VatRegime = vatRegime;
        }

        public static SellerSnapshot From(Seller seller)
        {
            ArgumentNullException.ThrowIfNull(seller);
            return new SellerSnapshot(seller.Id, seller.LegalName, seller.Address, seller.Siret, seller.VatNumber, seller.VatRegime);
        }

        /// <summary>Which seller the snapshot was taken from, so the quote can check it is its own.</summary>
        public Guid SellerId { get; }

        public string LegalName { get; }

        public Address Address { get; }

        public string Siret { get; }

        public string? VatNumber { get; }

        public VatRegime VatRegime { get; }
    }
}
