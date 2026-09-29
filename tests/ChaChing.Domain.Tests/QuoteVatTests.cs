using ChaChing.Domain.Billing;
using ChaChing.Domain.Billing.Discounts;
using ChaChing.Domain.Billing.Quotes;
using ChaChing.Domain.Billing.Taxes;

namespace ChaChing.Domain.Tests
{
    /// <summary>VAT totals of a quote (ADR 0014 and 0020) — for Marine.</summary>
    public class QuoteVatTests
    {
        static readonly VatRate twenty = VatRate.Standard;
        static readonly VatRate fivePointFive = VatRate.Of(5.5m);
        static readonly VatRate ten = VatRate.Of(10m);

        static Money Euros(decimal amount) => new(amount, Currency.EUR);

        static Quote Draft(Discount? discount = null) => Quote.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Currency.EUR, discount);

        static QuoteLine Line(decimal amount, VatRate vatRate, Discount? discount = null)
            => new("valid description", 1m, Euros(amount), discount, vatRate);

        static VatSubtotal SubtotalOf(Quote quote, VatRate rate) => Assert.Single(quote.VatBreakdown, subtotal => subtotal.Rate == rate);

        #region Without discount
        [Fact]
        public void EmptyQuote_HasNoBreakdownAndZeroTotals()
        {
            Quote quote = Draft();

            Assert.Empty(quote.VatBreakdown);
            Assert.Equal(Money.Zero(Currency.EUR), quote.TotalVatAmount);
            Assert.Equal(Money.Zero(Currency.EUR), quote.TotalWithVat);
        }

        [Fact]
        public void SingleRate_OneSubtotalForAllTheLines()
        {
            Quote quote = Draft();
            quote.AddLine(Line(100m, twenty));
            quote.AddLine(Line(50m, twenty));

            VatSubtotal subtotal = Assert.Single(quote.VatBreakdown);

            Assert.Equal(new VatSubtotal(twenty, Euros(150m), Euros(30m)), subtotal);
            Assert.Equal(Euros(30m), quote.TotalVatAmount);
            Assert.Equal(Euros(180m), quote.TotalWithVat);
        }

        [Fact]
        public void TwoRates_OneSubtotalPerRate()
        {
            Quote quote = Draft();
            quote.AddLine(Line(100m, twenty));
            quote.AddLine(Line(200m, fivePointFive));

            Assert.Equal(2, quote.VatBreakdown.Count);
            Assert.Equal(new VatSubtotal(twenty, Euros(100m), Euros(20m)), SubtotalOf(quote, twenty));
            Assert.Equal(new VatSubtotal(fivePointFive, Euros(200m), Euros(11m)), SubtotalOf(quote, fivePointFive));
            Assert.Equal(Euros(31m), quote.TotalVatAmount);
            Assert.Equal(Euros(331m), quote.TotalWithVat);
        }

        [Fact]
        public void ExemptLines_NoVat()
        {
            Quote quote = Draft();
            quote.AddLine(Line(300m, VatRate.Exempt));

            Assert.Equal(new VatSubtotal(VatRate.Exempt, Euros(300m), Euros(0m)), Assert.Single(quote.VatBreakdown));
            Assert.Equal(Money.Zero(Currency.EUR), quote.TotalVatAmount);
            Assert.Equal(Euros(300m), quote.TotalWithVat);
        }

        [Fact]
        public void LineDiscount_VatOnTheDiscountedLine()
        {
            Quote quote = Draft();
            quote.AddLine(new QuoteLine("valid description", 3m, Euros(500m), new PercentageDiscount(10m), twenty));   // 1350, not 1500

            Assert.Equal(new VatSubtotal(twenty, Euros(1350m), Euros(270m)), Assert.Single(quote.VatBreakdown));
        }
        #endregion

        #region With a discount on the total: shared pro rata between the rates
        [Fact]
        public void PercentageDiscountOnTotal_AppliedToEachRate()
        {
            Quote quote = Draft(new PercentageDiscount(10m));
            quote.AddLine(Line(100m, twenty));
            quote.AddLine(Line(200m, fivePointFive));

            Assert.Equal(new VatSubtotal(twenty, Euros(90m), Euros(18m)), SubtotalOf(quote, twenty));
            Assert.Equal(new VatSubtotal(fivePointFive, Euros(180m), Euros(9.9m)), SubtotalOf(quote, fivePointFive));
            Assert.Equal(Euros(27.9m), quote.TotalVatAmount);
            Assert.Equal(Euros(297.9m), quote.TotalWithVat);
        }

        [Fact]
        public void FixedDiscountOnTotal_SharedByWeight()
        {
            // 30 € off 300 € = 10 %: the 100 € base loses 10 €, the 200 € base loses 20 €
            Quote quote = Draft(new FixedAmountDiscount(Euros(30m)));
            quote.AddLine(Line(100m, twenty));
            quote.AddLine(Line(200m, fivePointFive));

            Assert.Equal(Euros(90m), SubtotalOf(quote, twenty).TaxableAmount);
            Assert.Equal(Euros(180m), SubtotalOf(quote, fivePointFive).TaxableAmount);
        }

        [Fact]
        public void DiscountOnTotal_TaxableAmountsAlwaysAddUpToTotalWithoutVat()
        {
            // 10 € off 30 €: each base is 6.666… → rounding must not lose or create a cent
            Quote quote = Draft(new FixedAmountDiscount(Euros(10m)));
            quote.AddLine(Line(10m, twenty));
            quote.AddLine(Line(10m, fivePointFive));
            quote.AddLine(Line(10m, ten));

            Money sumOfBases = quote.VatBreakdown.Aggregate(Money.Zero(Currency.EUR), (sum, subtotal) => sum + subtotal.TaxableAmount);

            Assert.Equal(Euros(20m), quote.TotalWithoutVat);
            Assert.Equal(quote.TotalWithoutVat, sumOfBases);
        }

        [Fact]
        public void DiscountOnTotalAboveTheTotal_NoVat()
        {
            Quote quote = Draft(new FixedAmountDiscount(Euros(1000m)));
            quote.AddLine(Line(100m, twenty));

            Assert.Equal(Money.Zero(Currency.EUR), quote.TotalVatAmount);
            Assert.Equal(Money.Zero(Currency.EUR), quote.TotalWithVat);
        }

        [Fact]
        public void DiscountOnZeroTotal_NoVat()
        {
            Quote quote = Draft(new PercentageDiscount(10m));
            quote.AddLine(Line(0m, twenty));

            Assert.Equal(Money.Zero(Currency.EUR), quote.TotalVatAmount);
            Assert.Equal(Money.Zero(Currency.EUR), quote.TotalWithVat);
            Assert.Single(quote.VatBreakdown);
        }
        #endregion
    }
}
