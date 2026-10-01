using ChaChing.Domain.Common;

namespace ChaChing.Domain.Parties
{
    public class Seller
    {
        private Seller(Guid id, string legalName, Address address, string siret, VatRegime vatRegime, string? vatNumber)
        {
            Id = id;
            LegalName = ValidateLegalName(legalName);
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Siret = ValidateSiret(siret);
            VatRegime = Guard.ValidateEnum(vatRegime, nameof(vatRegime));
            VatNumber = ValidateVatNumber(vatNumber);
        }

        public static Seller Create(string legalName, Address address, string siret, VatRegime vatRegime, string? vatNumber = null)
            => new Seller(Guid.NewGuid(), legalName, address, siret, vatRegime, vatNumber);

        public Guid Id { get; }

        public string LegalName { get; }

        public Address Address { get; }

        public string Siret { get; }

        public VatRegime VatRegime { get; }

        public string? VatNumber { get; }

        private static string ValidateLegalName(string legalName)
        {
            if (string.IsNullOrWhiteSpace(legalName))
                throw new ArgumentException("The legal name is required.", nameof(legalName));
            return legalName.Trim();
        }

        private static string ValidateSiret(string siret)
        {
            string digits = (siret ?? string.Empty).Replace(" ", string.Empty);
            if (digits.Length != 14 || !digits.All(char.IsAsciiDigit))
                throw new ArgumentException("A SIRET has 14 digits.", nameof(siret));
            return digits;
        }

        private static string? ValidateVatNumber(string? vatNumber)
        {
            if (vatNumber is null)
                return null;
            if (string.IsNullOrWhiteSpace(vatNumber))
                throw new ArgumentException("The VAT number is blank: pass null if there is none.", nameof(vatNumber));
            return vatNumber.Replace(" ", string.Empty).ToUpperInvariant();
        }

        public override bool Equals(object? obj) => obj is Seller other && other.Id == Id;

        public override int GetHashCode() => Id.GetHashCode();
    }
}
