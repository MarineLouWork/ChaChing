using ChaChing.Domain.Billing;
using ChaChing.Domain.Billing.Discounts;
using ChaChing.Domain.Billing.Quotes;

namespace ChaChing.Domain.Tests
{
    public class QuoteTests
    {
        static readonly Money fiveHundredEuros = new Money(500m, Currency.EUR);

        [Fact]
        public void QuoteLine_Allocated_IsValid()
        {
            QuoteLine ValidQuoteLine = new("refound of the branding of Stark's family", 2m, fiveHundredEuros);

            Assert.NotEqual(Guid.Empty, ValidQuoteLine.Id);
            Assert.Equal("refound of the branding of Stark's family", ValidQuoteLine.Description);
            Assert.Equal(2m, ValidQuoteLine.Quantity);
            Assert.Equal(new Money(500m, Currency.EUR), ValidQuoteLine.UnitPrice);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void QuoteLine_WithBlankOrEmptyDescription_Throws(string? description)
        {
            var action = () => new QuoteLine(description!, 2m, fiveHundredEuros);

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void QuoteLine_WithZeroQuantity_Throws()
        {
            var action = () => new QuoteLine("valid description", 0, fiveHundredEuros);

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void QuoteLine_WithNegativeOrNullUnitPrice_Throws()
        {
            var action = () => new QuoteLine("valid description", 2m, new Money(-0.01m, Currency.EUR));

            Assert.Throws<ArgumentException>(action);
        }

        [Theory]
        [InlineData(2, 134.5, 269)]
        [InlineData(18, 59.62, 1073.16)]
        public void QuoteLine_Allocated_TotalPriceIsValid(decimal quantity, decimal unitPriceAmount, decimal result)
        {
            QuoteLine quoteLine = new("valid description", quantity, new Money(unitPriceAmount, Currency.EUR));

            Assert.Equal(result, quoteLine.TotalPrice.Amount);
        }

        [Fact]
        public void QuoteLine_AllocatedWithDiscount_TotalPriceIsValid()
        {
            Discount tenPercentDiscount = new PercentageDiscount(10m);

            QuoteLine quoteLine = new("valid description", 3, fiveHundredEuros, tenPercentDiscount);

            Assert.Equal(1350m, quoteLine.TotalPrice.Amount);
        }
    }
}
