using ChaChing.Clients;

namespace ChaChing.Tests
{
    public class ClientTests
    {
        const string nomValide = "Arya";
        const string nomChange = "Senza";
        const string emailValide = "Arya@msn.fr";
        const string emailChange = "Stark@msn.fr";

        [Fact]
        public void Creer_DonneesValides_Assignees()
        {
            Client client = Client.Creer(nomValide, emailValide);

            Assert.NotEqual(Guid.Empty, client.Id);
            Assert.Equal(nomValide, client.Nom);
            Assert.Equal(emailValide, client.Email);
        }

        [Fact]
        public void Creer_DonneesIdentiques_PasEgaux()
        {
            Client clientA = Client.Creer(nomValide, emailValide);
            Client clientB = Client.Creer(nomValide, emailValide);

            Assert.NotEqual(clientA, clientB);
        }

        [Fact]
        public void IdIdentique_Egaux()
        {
            Client clientA = Client.Creer(nomValide, emailValide);
            Client clientAA = Client.Reconstituer(clientA.Id, nomChange, emailChange);

            Assert.Equal(clientA, clientAA);
        }

        //Creer_…_GenereUnId
        [Fact]
        public void Creer_GenereUnId()
        {
            Client client = Client.Creer(nomValide, emailValide);

            Assert.NotEqual(Guid.Empty, client.Id);
        }

        [Fact]
        public void Reconstituer_ConserveUnId()
        {
            Guid id = Guid.NewGuid();

            Client client = Client.Reconstituer(id, nomValide, emailValide);

            Assert.Equal(id, client.Id);
        }

        [Fact]
        public void Reconstituer_IdVide_LeveException()
        {
            var action = () => Client.Reconstituer(Guid.Empty, nomValide, emailValide);

            Assert.Throws<ArgumentException>(action);
        }

        [Theory]
        [InlineData(" " + nomValide + " ", nomValide)]
        [InlineData(" " + nomValide, nomValide)]
        [InlineData(nomValide + " ", nomValide)]
        [InlineData(nomValide + " " + nomValide, nomValide + " " + nomValide)]
        public void Creer_NomAvecEspacesDebutFin_Nettoye(string nom, string nomAttendu)
        {
            Client client = Client.Creer(nom, emailValide);

            Assert.Equal(nomAttendu, client.Nom);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void Creer_NomInvalide_LeveException(string? nom)
        {
            var action = () => Client.Creer(nom!, emailValide); // ! : on force le null exprès, c'est le cas testé

            Assert.Throws<ArgumentException>(action);
        }

        [Theory]
        [InlineData("emailSansArobas")]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void Creer_EmailInvalide_LeveException(string? email)
        {
            var action = () => Client.Creer(nomValide, email!); // ! : on force le null exprès, c'est le cas testé

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Creer_ChangerEmail_EmailValide_Assigne()
        {
            Client client = Client.Creer(nomValide, emailValide);

            client.ChangerEmail(emailChange);

            Assert.Equal(emailChange, client.Email);
        }

        [Fact]
        public void Creer_ChangerEmail_EmailInvalide_LeveException()
        {
            Client client = Client.Creer(nomValide, emailValide);

            var action = () => client.ChangerEmail(nomValide);

            Assert.Throws<ArgumentException>(action);
        }

    }
}
