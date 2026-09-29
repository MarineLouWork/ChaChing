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
            Seller seller = Seller.Create(" Arya Stark Design ", address, "123 456 789 00012", VatRegime.Subject, "fr 12 345678901");

            Assert.NotEqual(Guid.Empty, seller.Id);
            Assert.Equal("Arya Stark Design", seller.LegalName);
            Assert.Equal(address, seller.Address);
            Assert.Equal(validSiret, seller.Siret);
            Assert.Equal(VatRegime.Subject, seller.VatRegime);
            Assert.Equal("FR12345678901", seller.VatNumber);
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

            Assert.Throws<ArgumentException>(action);
        }

        [Theory]
        [InlineData("1234567890001")]
        [InlineData("123456789000123")]
        [InlineData("1234567890001A")]
        [InlineData("")]
        public void Create_WithInvalidSiret_Throws(string siret)
        {
            var action = () => Seller.Create("Arya Stark Design", address, siret, VatRegime.Subject);

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Create_WithUndefinedVatRegime_Throws()
        {
            var action = () => Seller.Create("Arya Stark Design", address, validSiret, (VatRegime)0);

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Create_WithBlankVatNumber_Throws()
        {
            var action = () => Seller.Create("Arya Stark Design", address, validSiret, VatRegime.Subject, " ");

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Create_WithNullAddress_Throws()
        {
            var action = () => Seller.Create("Arya Stark Design", null!, validSiret, VatRegime.Subject); // ! : null is forced on purpose, it is the case under test

            Assert.Throws<ArgumentNullException>(action);
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
