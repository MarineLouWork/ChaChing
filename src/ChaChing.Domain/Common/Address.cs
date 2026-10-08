namespace ChaChing.Domain.Common
{
    public record Address(string Line, string PostalCode, string City, string CountryCode)
    {
        public string Line { get; } = ArgumentValidator.NonBlankText(Line, nameof(Line));

        public string PostalCode { get; } = ArgumentValidator.NonBlankText(PostalCode, nameof(PostalCode));

        public string City { get; } = ArgumentValidator.NonBlankText(City, nameof(City));

        public string CountryCode { get; } = ValidateCountryCode(CountryCode);

        static string ValidateCountryCode(string countryCode)
        {
            string code = ArgumentValidator.NonBlankText(countryCode, nameof(CountryCode)).ToUpperInvariant();
            if (code.Length != 2 || !code.All(char.IsAsciiLetter))
                throw new ArgumentException("The country code must be 2 letters (ISO 3166-1).", nameof(CountryCode));
            return code;
        }
    }
}
