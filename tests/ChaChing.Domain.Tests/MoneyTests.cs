using ChaChing.Domain.Common;

namespace ChaChing.Domain.Tests
{
    public class MoneyTests
    {
        static readonly Money tenEuros = new Money(10m, Currency.EUR);
        static readonly Money twelveEuros = new Money(12m, Currency.EUR);
        static readonly Money twelveDollars = new Money(12m, Currency.USD);

        [Fact]
        public void TwoMoneys_Identical_Equal() => Assert.Equal(twelveEuros, new Money(12m, Currency.EUR));

        [Fact]
        public void TwoMoneys_Different_NotEqual() => Assert.NotEqual(tenEuros, twelveEuros);

        [Theory]
        [InlineData(Currency.EUR)]
        [InlineData(Currency.USD)]
        [InlineData(Currency.GBP)]
        public void Currency_Valid_Assigned(Currency currency)
        {
            Money moneyA = new Money(10m, currency);

            Assert.Equal(currency, moneyA.Currency);
        }

        [Fact]
        public void Currency_Invalid_Throws()
        {
            var action = () => new Money(10m, (Currency)888);

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Add_Amounts_ReturnsSum()
        {
            Money money = tenEuros.Add(twelveEuros);

            Assert.Equal(22m, money.Amount);
            Assert.Equal(Currency.EUR, money.Currency);
        }

        [Fact]
        public void Amounts_PlusOperator_ReturnsSum()
        {
            Money money = tenEuros + twelveEuros;

            Assert.Equal(22m, money.Amount);
        }

        [Fact]
        public void Add_DifferentCurrencies_Throws()
        {
            var action = () => tenEuros.Add(twelveDollars);

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Subtract_Amounts_ReturnsDifference()
        {
            Money money = tenEuros.Subtract(twelveEuros);

            Assert.Equal(-2m, money.Amount);
            Assert.Equal(Currency.EUR, money.Currency);
        }

        [Fact]
        public void Amounts_MinusOperator_ReturnsDifference()
        {
            Money money = tenEuros - twelveEuros;

            Assert.Equal(-2m, money.Amount);
        }

        [Fact]
        public void Subtract_DifferentCurrencies_Throws()
        {
            var action = () => tenEuros.Subtract(twelveDollars);

            Assert.Throws<ArgumentException>(action);
        }

        [Theory]
        [InlineData(2, 20)]
        [InlineData(1.5, 15)]
        [InlineData(1.75, 17.5)]
        [InlineData(1.725, 17.25)]
        [InlineData(1.7215, 17.22)]
        [InlineData(1.7225, 17.22)]
        public void Multiply_Amounts_ReturnsResult(decimal factor, decimal result)
        {
            Money money = tenEuros.Multiply(factor);

            Assert.Equal(result, money.Amount);
            Assert.Equal(Currency.EUR, money.Currency);
        }

        [Fact]
        public void Amounts_TimesOperator_ReturnsResult()
        {
            Money money = tenEuros * 1.5m;

            Assert.Equal(15m, money.Amount);
        }

        [Fact]
        public void Multiply_Amounts_RoundsToEven()
        {
            Money money = tenEuros.Multiply(1.7225m);

            Assert.Equal(17.22m, money.Amount);
            Assert.Equal(Currency.EUR, money.Currency);
        }

        [Fact]
        public void Zero_InvalidCurrency_Throws()
        {
            var action = () => Money.Zero((Currency)999);

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Zero_ValidCurrency_ReturnsZero()
        {
            var money = Money.Zero(Currency.GBP);

            Assert.Equal(0m, money.Amount);
            Assert.Equal(Currency.GBP, money.Currency);
        }

        [Fact]
        public void Zero_AddTenEuros_EqualsTenEuros()
        {
            Money money = Money.Zero(Currency.EUR).Add(tenEuros);

            Assert.Equal(10m, money.Amount);
            Assert.Equal(Currency.EUR, money.Currency);
        }
    }
}
