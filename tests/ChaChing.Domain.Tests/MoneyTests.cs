using ChaChing.Facturation;

namespace ChaChing.Tests
{
    public class MoneyTests
    {
        static readonly Money dixEuros = new Money(10m, Devise.EUR);
        static readonly Money douzeEuros = new Money(12m, Devise.EUR);
        static readonly Money douzeDollars = new Money(12m, Devise.USD);

        [Fact]
        public void DeuxMoneys_Identiques_Egaux() => Assert.Equal(douzeEuros, new Money(12m, Devise.EUR));

        [Fact]
        public void DeuxMoneys_Differents_PasEgaux() => Assert.NotEqual(dixEuros, douzeEuros);

        [Theory]
        [InlineData(Devise.EUR)]
        [InlineData(Devise.USD)]
        [InlineData(Devise.GBP)]
        public void Devise_Valide_Assignee(Devise devise)
        {
            Money moneyA = new Money(10m, devise);

            Assert.Equal(devise, moneyA.Devise);
        }

        [Fact]
        public void Devise_Invalide_LeveException()
        {
            var action = () => new Money(10m, (Devise)888);

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Ajouter_Montants_RenvoieLaSomme()
        {
            Money money = dixEuros.Ajouter(douzeEuros);

            Assert.Equal(22m, money.Montant);
            Assert.Equal(Devise.EUR, money.Devise);
        }

        [Fact]
        public void Montants_OperatorPlus_RenvoieLaSomme()
        {
            Money money = dixEuros + douzeEuros;

            Assert.Equal(22m, money.Montant);
        }

        [Fact]
        public void Ajouter_DevisesDifferentes_LeveException()
        {
            var action = () => dixEuros.Ajouter(douzeDollars);

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Soustraire_Montants_RenvoieLaDifference()
        {
            Money money = dixEuros.Soustraire(douzeEuros);

            Assert.Equal(-2m, money.Montant);
            Assert.Equal(Devise.EUR, money.Devise);
        }

        [Fact]
        public void Montants_OperatorMoins_RenvoieLaSomme()
        {
            Money money = dixEuros - douzeEuros;

            Assert.Equal(-2m, money.Montant);
        }

        [Fact]
        public void Soustraire_DevisesDifferentes_LeveException()
        {
            var action = () => dixEuros.Soustraire(douzeDollars);

            Assert.Throws<ArgumentException>(action);
        }

        [Theory]
        [InlineData(2, 20)]
        [InlineData(1.5, 15)]
        [InlineData(1.75, 17.5)]
        [InlineData(1.725, 17.25)]
        [InlineData(1.7215, 17.22)]
        [InlineData(1.7225, 17.22)]
        public void Multiplier_Montants_RenvoieLeResultat(decimal facteur, decimal result)
        {
            Money money = dixEuros.Multiplier(facteur);

            Assert.Equal(result, money.Montant);
            Assert.Equal(Devise.EUR, money.Devise);
        }

        [Fact]
        public void Montants_OperatorFois_RenvoieLaSomme()
        {
            Money money = dixEuros * 1.5m;

            Assert.Equal(15m, money.Montant);
        }

        [Fact]
        public void Multiplier_Montants_ArronditEnEven()
        {
            Money money = dixEuros.Multiplier(1.7225m);

            Assert.Equal(17.22m, money.Montant);
            Assert.Equal(Devise.EUR, money.Devise);
        }

        [Fact]
        public void Zero_DeviseInvalide_LeveException()
        {
            var action = () => Money.Zero((Devise)999);

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Zero_DeviseValide_RenvoieZero()
        {
            var money = Money.Zero(Devise.GBP);

            Assert.Equal(0m, money.Montant);
            Assert.Equal(Devise.GBP, money.Devise);
        }

        [Fact]
        public void Zero_Ajouter10Euros_Vaut10Euros()
        {
            Money money = Money.Zero(Devise.EUR).Ajouter(dixEuros);

            Assert.Equal(10m, money.Montant);
            Assert.Equal(Devise.EUR, money.Devise);
        }
    }
}
