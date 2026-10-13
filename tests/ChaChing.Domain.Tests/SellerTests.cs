using ChaChing.Domain.Common;
using ChaChing.Domain.Parties;

namespace ChaChing.Domain.Tests
{
    public class SellerTests
    {
        static readonly Address address = TestData.Address();
        const string validSiret = "12345678900012";

        [Fact]
        public void Create_WithValidData_IsCreated()
        {
            Seller seller = Seller.Create(" Arya Stark Design ", address, "123 456 789 00012", VatRegime.Subject, " fr on 123 456 789 ");

            Assert.NotEqual(Guid.Empty, seller.Id);
            Assert.Equal("Arya Stark Design", seller.LegalName);
            Assert.Equal(address, seller.Address);
            Assert.Equal(validSiret, seller.Siret);
            Assert.Equal(VatRegime.Subject, seller.VatRegime);
            Assert.Equal("FRON123456789", seller.VatNumber);
        }

        [Fact]
        public void Create_WithoutVatNumber_IsNull()
        {
            Seller seller = Seller.Create("Arya Stark Design", address, validSiret, VatRegime.Exempt);

            Assert.Null(seller.VatNumber);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void Create_WithBlankLegalName_Throws(string? legalName)
        {
            var action = () => Seller.Create(legalName!, address, validSiret, VatRegime.Subject); // ! : null is forced on purpose, it is the case under test

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("legalName", exception.ParamName);
        }

        [Theory]
        [InlineData("1234567890001")]
        [InlineData("123456789000123")]
        [InlineData("1234567890001A")]
        [InlineData("")]
        public void Create_WithInvalidSiret_Throws(string siret)
        {
            var action = () => Seller.Create("Arya Stark Design", address, siret, VatRegime.Subject);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("siret", exception.ParamName);
        }

        [Fact]
        public void Create_WithUndefinedVatRegime_Throws()
        {
            var action = () => Seller.Create("Arya Stark Design", address, validSiret, (VatRegime)0);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("vatRegime", exception.ParamName);
        }


        [Theory]
        [InlineData("12345678900012", "   ")]
        [InlineData("12345678900012", "EN ON 123456789")]
        [InlineData("12345678900012", "FR ON 1234")]
        [InlineData("12345678900012", "FR ON 123444444")]
        [InlineData("12345678900012", "FR ON 155456789")]
        public void Create_WithInvalidVatNumber_Throws(string siret, string vatNumber)
        {
            var action = () => Seller.Create("Arya Stark Design", address, siret, VatRegime.Subject, vatNumber);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("vatNumber", exception.ParamName);
        }

        [Fact]
        public void Create_WithNullAddress_Throws()
        {
            var action = () => Seller.Create("Arya Stark Design", null!, validSiret, VatRegime.Subject); // ! : null is forced on purpose, it is the case under test

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("address", exception.ParamName);
        }

        [Fact]
        public void TwoSellersWithSameData_AreNotEqual()
        {
            Seller first = TestData.Seller();
            Seller second = TestData.Seller();

            Assert.NotEqual(first, second);
        }
    }
}
