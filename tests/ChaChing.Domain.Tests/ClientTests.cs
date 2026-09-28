using ChaChing.Domain.Clients;
using ChaChing.Domain.Common;

namespace ChaChing.Domain.Tests
{
    public class ClientTests
    {
        const string validName = "Arya";
        const string changedName = "Senza";
        const string validEmail = "Arya@msn.fr";
        const string changedEmail = "Stark@msn.fr";
        static readonly Guid sellerId = Guid.NewGuid();
        static readonly Address address = TestData.Address();

        [Fact]
        public void Create_ValidData_Assigned()
        {
            Client client = Client.Create(sellerId, validName, validEmail, address);

            Assert.NotEqual(Guid.Empty, client.Id);
            Assert.Equal(validName, client.Name);
            Assert.Equal(validEmail, client.Email);
        }

        [Fact]
        public void Create_SameData_NotEqual()
        {
            Client clientA = Client.Create(sellerId, validName, validEmail, address);
            Client clientB = Client.Create(sellerId, validName, validEmail, address);

            Assert.NotEqual(clientA, clientB);
        }

        [Fact]
        public void SameId_Equal()
        {
            Client clientA = Client.Create(sellerId, validName, validEmail, address);
            Client clientAA = Client.Rehydrate(clientA.Id, sellerId, changedName, changedEmail, address);

            Assert.Equal(clientA, clientAA);
        }

        //Create_…_GeneratesId
        [Fact]
        public void Create_GeneratesId()
        {
            Client client = Client.Create(sellerId, validName, validEmail, address);

            Assert.NotEqual(Guid.Empty, client.Id);
        }

        [Fact]
        public void Rehydrate_KeepsId()
        {
            Guid id = Guid.NewGuid();

            Client client = Client.Rehydrate(id, sellerId, validName, validEmail, address);

            Assert.Equal(id, client.Id);
        }

        [Fact]
        public void Rehydrate_EmptyId_Throws()
        {
            var action = () => Client.Rehydrate(Guid.Empty, sellerId, validName, validEmail, address);

            Assert.Throws<ArgumentException>(action);
        }

        [Theory]
        [InlineData(" " + validName + " ", validName)]
        [InlineData(" " + validName, validName)]
        [InlineData(validName + " ", validName)]
        [InlineData(validName + " " + validName, validName + " " + validName)]
        public void Create_NameWithLeadingOrTrailingSpaces_Trimmed(string name, string expectedName)
        {
            Client client = Client.Create(sellerId, name, validEmail, address);

            Assert.Equal(expectedName, client.Name);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void Create_InvalidName_Throws(string? name)
        {
            var action = () => Client.Create(sellerId, name!, validEmail, address); // ! : null is forced on purpose, it is the case under test

            Assert.Throws<ArgumentException>(action);
        }

        [Theory]
        [InlineData("emailWithoutAtSign")]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void Create_InvalidEmail_Throws(string? email)
        {
            var action = () => Client.Create(sellerId, validName, email!, address); // ! : null is forced on purpose, it is the case under test

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Create_ChangeEmail_ValidEmail_Assigned()
        {
            Client client = Client.Create(sellerId, validName, validEmail, address);

            client.ChangeEmail(changedEmail);

            Assert.Equal(changedEmail, client.Email);
        }

        [Fact]
        public void Create_ChangeEmail_InvalidEmail_Throws()
        {
            Client client = Client.Create(sellerId, validName, validEmail, address);

            var action = () => client.ChangeEmail(validName);

            Assert.Throws<ArgumentException>(action);
        }

        #region Seller, address, SIREN (ADR 0012)
        [Fact]
        public void Create_KeepsSellerAndAddress()
        {
            Client client = Client.Create(sellerId, validName, validEmail, address);

            Assert.Equal(sellerId, client.SellerId);
            Assert.Equal(address, client.Address);
            Assert.Null(client.Siren);
        }

        [Fact]
        public void Create_WithEmptySellerId_Throws()
        {
            var action = () => Client.Create(Guid.Empty, validName, validEmail, address);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("sellerId", exception.ParamName);
        }

        [Fact]
        public void Create_WithNullAddress_Throws()
        {
            var action = () => Client.Create(sellerId, validName, validEmail, null!); // ! : null is forced on purpose, it is the case under test

            Assert.Throws<ArgumentNullException>(action);
        }

        [Theory]
        [InlineData("123456789", "123456789")]
        [InlineData("123 456 789", "123456789")]
        public void Create_WithValidSiren_KeepsDigitsOnly(string siren, string expected)
        {
            Client client = Client.Create(sellerId, validName, validEmail, address, siren);

            Assert.Equal(expected, client.Siren);
        }

        [Theory]
        [InlineData("12345678")]
        [InlineData("1234567890")]
        [InlineData("12345678A")]
        [InlineData("")]
        public void Create_WithInvalidSiren_Throws(string siren)
        {
            var action = () => Client.Create(sellerId, validName, validEmail, address, siren);

            Assert.Throws<ArgumentException>(action);
        }
        #endregion
    }
}
