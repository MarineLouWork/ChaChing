using ChaChing.Domain.Common;

namespace ChaChing.Domain.Parties
{
    public class Seller
    {
        private Seller(Guid id, string legalName, Address address, string siret, VatRegime vatRegime, string? vatNumber)
        {
            Id = id;
            LegalName = ArgumentValidator.NonBlankText(legalName, nameof(legalName));
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Siret = ValidateSiret(siret);
            VatRegime = ArgumentValidator.Enum(vatRegime, nameof(vatRegime));
            VatNumber = ValidateVatNumber(vatNumber, Siret);
        }

        public static Seller Create(string legalName, Address address, string siret, VatRegime vatRegime, string? vatNumber = null)
            => new Seller(Guid.NewGuid(), legalName, address, siret, vatRegime, vatNumber);

        public Guid Id { get; }

        public string LegalName { get; }

        public Address Address { get; }

        public string Siret { get; }

        public VatRegime VatRegime { get; }

        public string? VatNumber { get; }

        private static string ValidateSiret(string siret)
        {
            string digits = (siret ?? string.Empty).Replace(" ", string.Empty);
            if (digits.Length != 14 || !digits.All(char.IsAsciiDigit))
                throw new ArgumentException("A SIRET has 14 digits.", nameof(siret));
            return digits;
        }

        private static string? ValidateVatNumber(string? vatNumber, string siret)
        {
            if (vatNumber is null)
                return null;
            if (string.IsNullOrWhiteSpace(vatNumber))
                throw new ArgumentException("The VAT number is blank: pass null if there is none.", nameof(vatNumber));

            vatNumber = vatNumber.Replace(" ", string.Empty).ToUpperInvariant();

            if (!vatNumber.StartsWith("FR", StringComparison.Ordinal))
                throw new ArgumentException("The VAT number should start with FR.", nameof(vatNumber));

            if (vatNumber.Length != 13)
                throw new ArgumentException("The VAT number should be 13 characters long.", nameof(vatNumber));

            string siren = siret.Substring(0, 9);
            string vatNumberSiren = vatNumber.Substring(4, 9);
            if (siren != vatNumberSiren)
                throw new ArgumentException("The VAT number should end with SIREN numbers.", nameof(vatNumber));

            return vatNumber;
        }

        public override bool Equals(object? obj) => obj is Seller other && other.Id == Id;

        public override int GetHashCode() => Id.GetHashCode();
    }
}
