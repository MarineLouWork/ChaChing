using ChaChing.Domain.Billing;
using ChaChing.Domain.Billing.Discounts;

namespace ChaChing.Domain.Tests
{
    public class DiscountTests
    {
        static readonly Money threeEuros = new Money(3m, Currency.EUR);
        static readonly Money tenEuros = new Money(10m, Currency.EUR);

        static readonly Money fifteenEuros = new Money(15m, Currency.EUR);

        static readonly FixedAmountDiscount threeEurosDiscount = new(threeEuros);

        static readonly FixedAmountDiscount fifteenEurosDiscount = new(fifteenEuros);

        static readonly PercentageDiscount tenPercentDiscount = new PercentageDiscount(10m);

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
        public void PercentageDiscount_TenPercent_Applied()
        {
            Money balance = tenPercentDiscount.Apply(tenEuros);

            Assert.Equal(9m, balance.Amount);
            Assert.Equal(Currency.EUR, balance.Currency);
        }

        [Theory]
        [InlineData(0.01, 10)]
        [InlineData(99.99, 0)]
        [InlineData(100, 0)]
        public void PercentageDiscount_ValidRate_Applied(decimal rate, decimal result)
        {
            Discount discount = new PercentageDiscount(rate);

            Money balance = discount.Apply(tenEuros);

            Assert.Equal(result, balance.Amount);
            Assert.Equal(Currency.EUR, balance.Currency);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-0.01)]
        [InlineData(-10)]
        [InlineData(110)]
        [InlineData(100.01)]
        public void PercentageDiscount_InvalidRate_Throws(decimal rate)
        {
            var action = () => new PercentageDiscount(rate);

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Discounts_PercentageAndFixed_Applied()
        {
            Discount[] discounts = [tenPercentDiscount, threeEurosDiscount];
            Money balance = new(10m, Currency.EUR);

            foreach (Discount d in discounts)
            {
                balance = d.Apply(balance);
            }

            Assert.Equal(6m, balance.Amount);
            Assert.Equal(Currency.EUR, balance.Currency);
        }

    }
}
