using ChaChing.Domain.Billing;
using ChaChing.Domain.Common;
using ChaChing.Domain.Parties;

namespace ChaChing.Domain.Tests
{
    public class BillingLineTests
    {
        static readonly Money fiveHundredEuros = new Money(500m, Currency.EUR);

        #region BillingLine Tests
        [Fact]
        public void BillingLine_Allocated_IsValid()
        {
            BillingLine ValidBillingLine = new("refound of the branding of Stark's family", 2m, fiveHundredEuros);

            Assert.NotEqual(Guid.Empty, ValidBillingLine.Id);
            Assert.Equal("refound of the branding of Stark's family", ValidBillingLine.Description);
            Assert.Equal(2m, ValidBillingLine.Quantity);
            Assert.Equal(new Money(500m, Currency.EUR), ValidBillingLine.UnitPrice);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void BillingLine_WithBlankOrEmptyDescription_Throws(string? description)
        {
            var action = () => new BillingLine(description!, 2m, fiveHundredEuros);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("description", exception.ParamName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-0.01)]
        public void BillingLine_WithInvalidQuantity_Throws(decimal quantity)
        {
            var action = () => new BillingLine("valid description", quantity, fiveHundredEuros);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("quantity", exception.ParamName);
        }

        [Fact]
        public void BillingLine_WithSpecificQuantity_IsValid()
        {
            BillingLine ValidBillingLine = new("valid description", 0.01m, fiveHundredEuros);

            Assert.Equal(0.01m, ValidBillingLine.Quantity);
        }

        [Fact]
        public void BillingLine_WithNoPrice_IsValid()
        {
            BillingLine ValidBillingLine = new("valid description", 1m, new Money(0m, Currency.EUR));

            Assert.Equal(new Money(0m, Currency.EUR), ValidBillingLine.UnitPrice);
        }

        [Fact]
        public void BillingLine_WithNegativeUnitPrice_Throws()
        {
            var action = () => new BillingLine("valid description", 2m, new Money(-0.01m, Currency.EUR));

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("unitPrice", exception.ParamName);
        }

        [Theory]
        [InlineData(2, 134.5, 269)]
        [InlineData(18, 59.62, 1073.16)]
        public void BillingLine_Allocated_TotalPriceIsValid(decimal quantity, decimal unitPriceAmount, decimal result)
        {
            BillingLine quoteLine = new("valid description", quantity, new Money(unitPriceAmount, Currency.EUR));

            Assert.Equal(result, quoteLine.TotalPrice.Amount);
        }

        [Fact]
        public void BillingLine_AllocatedWithDiscount_TotalPriceIsValid()
        {
            Discount tenPercentDiscount = new PercentageDiscount(10m);

            BillingLine quoteLine = new("valid description", 3, fiveHundredEuros, tenPercentDiscount);

            Assert.Equal(1350m, quoteLine.TotalPrice.Amount);
        }
        #endregion

        #region VAT rate (ADR 0014)
        [Fact]
        public void BillingLine_WithoutVatRate_IsStandardRate()
        {
            BillingLine quoteLine = new("valid description", 1m, fiveHundredEuros);

            Assert.Equal(VatRate.Standard, quoteLine.VatRate);
        }

        [Fact]
        public void BillingLine_WithVatRate_KeepsIt()
        {
            BillingLine quoteLine = new("valid description", 1m, fiveHundredEuros, vatRate: VatRate.Of(5.5m));

            Assert.Equal(VatRate.Of(5.5m), quoteLine.VatRate);
        }
        #endregion

        #region IsAllowedFor (shared by Quote and Invoice)
        [Theory]
        [InlineData(VatRegime.Exempt, true, true)]      // exempt seller, exempt line
        [InlineData(VatRegime.Exempt, false, false)]    // exempt seller, 20 % line: the forbidden case
        [InlineData(VatRegime.Subject, false, true)]    // subject seller, 20 % line
        [InlineData(VatRegime.Subject, true, true)]     // subject seller may bill an exempt line
        public void IsAllowedFor_DependsOnTheSellerRegimeAndTheLineRate(VatRegime regime, bool lineIsExempt, bool expected)
        {
            VatRate rate = lineIsExempt ? VatRate.Exempt : VatRate.Standard;
            BillingLine line = new("valid description", 1m, fiveHundredEuros, vatRate: rate);

            Assert.Equal(expected, line.IsAllowedFor(regime));
        }
        #endregion
    }
}
