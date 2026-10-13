using ChaChing.Domain.Common;

namespace ChaChing.Domain.Tests
{
    public class AddressTests
    {
        [Fact]
        public void Create_WithValidData_TrimsAndUppercasesTheCountry()
        {
            Address address = new(" 12 rue de la Paix ", " 13001 ", " Marseille ", " fr ");

            Assert.Equal("12 rue de la Paix", address.Line);
            Assert.Equal("13001", address.PostalCode);
            Assert.Equal("Marseille", address.City);
            Assert.Equal("FR", address.CountryCode);
        }

        [Fact]
        public void SameValues_AreEqual()
        {
            Assert.Equal(TestData.Address(), TestData.Address());
        }

        [Theory]
        [InlineData("", "13001", "Marseille", "FR", "Line")]
        [InlineData("12 rue de la Paix", " ", "Marseille", "FR", "PostalCode")]
        [InlineData("12 rue de la Paix", "13001", null, "FR", "City")]
        public void Create_WithMissingPart_Throws(string? line, string? postalCode, string? city, string countryCode, string expectedParamName)
        {
            var action = () => new Address(line!, postalCode!, city!, countryCode); // ! : null is forced on purpose, it is the case under test

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal(expectedParamName, exception.ParamName);
        }

        [Theory]
        [InlineData("F")]
        [InlineData("FRA")]
        [InlineData("F1")]
        [InlineData("")]
        public void Create_WithInvalidCountryCode_Throws(string countryCode)
        {
            var action = () => new Address("12 rue de la Paix", "13001", "Marseille", countryCode);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("CountryCode", exception.ParamName);
        }
    }
}
