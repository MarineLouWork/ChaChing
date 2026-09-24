using ChaChing.Domain.Clients;

namespace ChaChing.Domain.Tests
{
    public class ClientTests
    {
        const string validName = "Arya";
        const string changedName = "Senza";
        const string validEmail = "Arya@msn.fr";
        const string changedEmail = "Stark@msn.fr";

        [Fact]
        public void Create_ValidData_Assigned()
        {
            Client client = Client.Create(validName, validEmail);

            Assert.NotEqual(Guid.Empty, client.Id);
            Assert.Equal(validName, client.Name);
            Assert.Equal(validEmail, client.Email);
        }

        [Fact]
        public void Create_SameData_NotEqual()
        {
            Client clientA = Client.Create(validName, validEmail);
            Client clientB = Client.Create(validName, validEmail);

            Assert.NotEqual(clientA, clientB);
        }

        [Fact]
        public void SameId_Equal()
        {
            Client clientA = Client.Create(validName, validEmail);
            Client clientAA = Client.Rehydrate(clientA.Id, changedName, changedEmail);

            Assert.Equal(clientA, clientAA);
        }

        //Create_…_GeneratesId
        [Fact]
        public void Create_GeneratesId()
        {
            Client client = Client.Create(validName, validEmail);

            Assert.NotEqual(Guid.Empty, client.Id);
        }

        [Fact]
        public void Rehydrate_KeepsId()
        {
            Guid id = Guid.NewGuid();

            Client client = Client.Rehydrate(id, validName, validEmail);

            Assert.Equal(id, client.Id);
        }

        [Fact]
        public void Rehydrate_EmptyId_Throws()
        {
            var action = () => Client.Rehydrate(Guid.Empty, validName, validEmail);

            Assert.Throws<ArgumentException>(action);
        }

        [Theory]
        [InlineData(" " + validName + " ", validName)]
        [InlineData(" " + validName, validName)]
        [InlineData(validName + " ", validName)]
        [InlineData(validName + " " + validName, validName + " " + validName)]
        public void Create_NameWithLeadingOrTrailingSpaces_Trimmed(string name, string expectedName)
        {
            Client client = Client.Create(name, validEmail);

            Assert.Equal(expectedName, client.Name);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void Create_InvalidName_Throws(string? name)
        {
            var action = () => Client.Create(name!, validEmail); // ! : null is forced on purpose, it is the case under test

            Assert.Throws<ArgumentException>(action);
        }

        [Theory]
        [InlineData("emailWithoutAtSign")]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void Create_InvalidEmail_Throws(string? email)
        {
            var action = () => Client.Create(validName, email!); // ! : null is forced on purpose, it is the case under test

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Create_ChangeEmail_ValidEmail_Assigned()
        {
            Client client = Client.Create(validName, validEmail);

            client.ChangeEmail(changedEmail);

            Assert.Equal(changedEmail, client.Email);
        }

        [Fact]
        public void Create_ChangeEmail_InvalidEmail_Throws()
        {
            Client client = Client.Create(validName, validEmail);

            var action = () => client.ChangeEmail(validName);

            Assert.Throws<ArgumentException>(action);
        }

    }
}
