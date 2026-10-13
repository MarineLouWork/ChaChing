using ChaChing.Domain.Billing;
using ChaChing.Domain.Common;

namespace ChaChing.Domain.Tests
{
    public class VatRateTests
    {
        [Fact]
        public void Standard_Is20Percent()
        {
            Assert.Equal(20m, VatRate.Standard.Percent);
            Assert.False(VatRate.Standard.IsExempt);
        }

        [Fact]
        public void Exempt_IsZeroAndExempt()
        {
            Assert.Equal(0m, VatRate.Exempt.Percent);
            Assert.True(VatRate.Exempt.IsExempt);
        }

        [Fact]
        public void SamePercent_AreEqual()
        {
            Assert.Equal(VatRate.Of(5.5m), VatRate.Of(5.5m));
            Assert.Equal(VatRate.Standard, VatRate.Of(20m));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(100)]
        public void Of_OutOfRange_Throws(decimal percent)
        {
            var action = () => VatRate.Of(percent);

            var exception = Assert.Throws<ArgumentOutOfRangeException>(action);
            Assert.Equal("percent", exception.ParamName);
        }

        [Theory]
        [InlineData(20, 100, 20)]
        [InlineData(5.5, 200, 11)]
        [InlineData(20, 0.99, 0.2)]     // 0.198 rounded
        public void VatOn_IsTheRateOfTheAmount(decimal percent, decimal amount, decimal expectedVat)
        {
            Money vat = VatRate.Of(percent).VatOn(new Money(amount, Currency.EUR));

            Assert.Equal(new Money(expectedVat, Currency.EUR), vat);
        }

        [Fact]
        public void VatOn_Exempt_IsZero()
        {
            Money vat = VatRate.Exempt.VatOn(new Money(300m, Currency.EUR));

            Assert.Equal(Money.Zero(Currency.EUR), vat);
        }
    }
}
