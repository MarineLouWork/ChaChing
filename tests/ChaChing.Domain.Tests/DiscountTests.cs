using ChaChing.Domain.Billing;
using ChaChing.Domain.Billing.Discounts;

namespace ChaChing.Domain.Tests
{
    public class DiscountTests
    {
        static readonly Money tenEuros = new Money(10m, Currency.EUR);
        static readonly FixedAmountDiscount threeEurosDiscount = new(new Money(3m, Currency.EUR));
        static readonly PercentageDiscount tenPercentDiscount = new PercentageDiscount(10m);

        [Fact]
        public void FixedDiscount_0Euros_DoesNotThrow()
        {
            FixedAmountDiscount zeroEurosDiscount = new(new Money(0m, Currency.EUR));

            Money balance = zeroEurosDiscount.Apply(tenEuros);

            Assert.Equal(10m, balance.Amount);
            Assert.Equal(Currency.EUR, balance.Currency);
        }

        [Fact]
        public void FixedDiscount_ThreeEuros_Applied()
        {
            Money balance = threeEurosDiscount.Apply(tenEuros);

            Assert.Equal(7m, balance.Amount);
            Assert.Equal(Currency.EUR, balance.Currency);
        }

        [Fact]
        public void FixedDiscount_GreaterThanTotal_FlooredAtZero()
        {
            FixedAmountDiscount fifteenEurosDiscount = new(new Money(15m, Currency.EUR));

            Money balance = fifteenEurosDiscount.Apply(tenEuros);

            Assert.Equal(0m, balance.Amount);
            Assert.Equal(Currency.EUR, balance.Currency);
        }

        [Fact]
        public void FixedDiscount_Negative_Throws()
        {
            var action = () => new FixedAmountDiscount(new Money(-0.01m, Currency.EUR));

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void FixedDiscount_DifferentCurrency_Throws()
        {
            var action = () => threeEurosDiscount.Apply(new Money(3m, Currency.GBP));

            Assert.Throws<ArgumentException>(action);
        }


        [Fact]
        public void PercentageDiscount_TenPercent_Applied()
        {
            Money balance = tenPercentDiscount.Apply(tenEuros);

            Assert.Equal(9m, balance.Amount);
            Assert.Equal(Currency.EUR, balance.Currency);
        }

        [Theory]
        [InlineData(0, 100)]
        [InlineData(0.01, 99.99)]
        [InlineData(100, 0)]
        public void PercentageDiscount_ValidRate_Applied(decimal rate, decimal result)
        {
            Discount discount = new PercentageDiscount(rate);

            Money balance = discount.Apply(new Money(100m, Currency.EUR));

            Assert.Equal(result, balance.Amount);
            Assert.Equal(Currency.EUR, balance.Currency);
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(100.01)]
        public void PercentageDiscount_InvalidRate_Throws(decimal rate)
        {
            var action = () => new PercentageDiscount(rate);

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Discounts_PercentageThenFixed_Applied()
        {
            Discount[] discounts = [tenPercentDiscount, threeEurosDiscount];
            Money balance = new(10m, Currency.EUR);

            foreach (Discount discount in discounts)
            {
                balance = discount.Apply(balance);
            }

            Assert.Equal(6m, balance.Amount);
            Assert.Equal(Currency.EUR, balance.Currency);
        }

    }
}
