namespace ChaChing.Domain.Common
{
    public record Address(string Line, string PostalCode, string City, string CountryCode)
    {
        public string Line { get; } = Required(Line, nameof(Line));

        public string PostalCode { get; } = Required(PostalCode, nameof(PostalCode));

        public string City { get; } = Required(City, nameof(City));

        public string CountryCode { get; } = ValidateCountryCode(CountryCode);

        static string Required(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"The {paramName} is required.", paramName);
            return value.Trim();
        }

        static string ValidateCountryCode(string countryCode)
        {
            string code = Required(countryCode, nameof(CountryCode)).ToUpperInvariant();
            if (code.Length != 2 || !code.All(char.IsAsciiLetter))
                throw new ArgumentException("The country code must be 2 letters (ISO 3166-1).", nameof(CountryCode));
            return code;
        }
    }
}
