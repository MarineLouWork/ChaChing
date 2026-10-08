using ChaChing.Domain.Common;

namespace ChaChing.Domain.Parties
{
    public class Client
    {
        public Guid Id { get; }

        /// <summary>The seller this client belongs to (ADR 0012).</summary>
        public Guid SellerId { get; }

        public string Name { get; }

        public string Email { get; private set; }

        public Address Address { get; }

        /// <summary>9 digits, for a business client (B2B); null for an individual.</summary>
        public string? Siren { get; }

        private Client(Guid id, Guid sellerId, string name, string email, Address address, string? siren)
        {
            Id = id;
            SellerId = ArgumentValidator.Guid(sellerId, nameof(sellerId));
            Name = ArgumentValidator.NonBlankText(name, nameof(name));
            Email = ValidateEmail(email);
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Siren = ValidateSiren(siren);
        }

        public static Client Create(Guid sellerId, string name, string email, Address address, string? siren = null)
            => new Client(Guid.NewGuid(), sellerId, name, email, address, siren);

        public static Client Rehydrate(Guid id, Guid sellerId, string name, string email, Address address, string? siren = null)
        {
            id = ArgumentValidator.Guid(id, nameof(id));
            return new Client(id, sellerId, name, email, address, siren);
        }

        public void ChangeEmail(string email)
        {
            Email = ValidateEmail(email);
        }

        private static string ValidateEmail(string email)
        {
            email = ArgumentValidator.NonBlankText(email, nameof(email));
            if (!email.Contains('@'))
                throw new ArgumentException("The email must contain an @.", nameof(email));
            return email;
        }

        private static string? ValidateSiren(string? siren)
        {
            if (siren is null)
                return null;

            string digits = siren.Replace(" ", string.Empty);
            if (digits.Length != 9 || !digits.All(char.IsAsciiDigit))
                throw new ArgumentException("A SIREN has 9 digits.", nameof(siren));
            return digits;
        }

        public override bool Equals(object? obj) => obj is Client other && other.Id == Id;

        public override int GetHashCode() => Id.GetHashCode();

    }
}
