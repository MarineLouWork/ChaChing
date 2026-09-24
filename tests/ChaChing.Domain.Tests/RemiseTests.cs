using ChaChing.Facturation;
using ChaChing.Facturation.Remises;

namespace ChaChing.Tests
{
    public class RemiseTests
    {
        static readonly Money troisEuros = new Money(3m, Devise.EUR);
        static readonly Money dixEuros = new Money(10m, Devise.EUR);

        static readonly Money quinzeEuros = new Money(15m, Devise.EUR);

        static readonly RemiseMontantFixe remiseTroisEuros = new(troisEuros);

        static readonly RemiseMontantFixe remiseQuinzeEuros = new(quinzeEuros);

        static readonly RemisePourcentage remiseDixPourcents = new RemisePourcentage(10m);

        [Fact]
        public void RemiseFixe_TroisEuros_Appliquee()
        {
            Money solde = remiseTroisEuros.Appliquer(dixEuros);

            Assert.Equal(7m, solde.Montant);
            Assert.Equal(Devise.EUR, solde.Devise);
        }

        [Fact]
        public void RemiseFixe_Superieure_Plafonnee()
        {
            Money solde = remiseQuinzeEuros.Appliquer(dixEuros);

            Assert.Equal(0m, solde.Montant);
            Assert.Equal(Devise.EUR, solde.Devise);
        }

        [Fact]
        public void RemiseFixe_Negative_LeveException()
        {
            var action = () => new RemiseMontantFixe(new Money(-0.01m, Devise.EUR));

            Assert.Throws<ArgumentException>(action);
        }


        [Fact]
        public void RemiseTaux_DixPourCent_Appliquee()
        {
            Money solde = remiseDixPourcents.Appliquer(dixEuros);

            Assert.Equal(9m, solde.Montant);
            Assert.Equal(Devise.EUR, solde.Devise);
        }

        [Theory]
        [InlineData(0.01, 10)]
        [InlineData(99.99, 0)]
        [InlineData(100, 0)]
        public void RemiseTaux_Valide_Appliquee(decimal taux, decimal result)
        {
            Remise remise = new RemisePourcentage(taux);

            Money solde = remise.Appliquer(dixEuros);

            Assert.Equal(result, solde.Montant);
            Assert.Equal(Devise.EUR, solde.Devise);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-0.01)]
        [InlineData(-10)]
        [InlineData(110)]
        [InlineData(100.01)]
        public void RemiseTaux_Invalide_LeveException(decimal taux)
        {
            var action = () => new RemisePourcentage(taux);

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Remises_PourcentsEtFixe_Appliquees()
        {
            Remise[] remises = [remiseDixPourcents, remiseTroisEuros];
            Money solde = new(10m, Devise.EUR);

            foreach (Remise r in remises)
            {
                solde = r.Appliquer(solde);
            }

            Assert.Equal(6m, solde.Montant);
            Assert.Equal(Devise.EUR, solde.Devise);
        }

    }
}
