using ChaChing.Domain.Billing;
using ChaChing.Domain.Billing.Discounts;
using ChaChing.Domain.Billing.Quotes;

namespace ChaChing.Domain.Tests
{
    public class QuoteTests
    {
        static readonly Guid clientId = Guid.NewGuid();
        static readonly Guid sellerId = Guid.NewGuid();

        static Quote NewEuroQuote() => Quote.CreateDraft(sellerId, clientId, Currency.EUR);

        static QuoteLine Line(decimal quantity, decimal unitPrice, Currency currency = Currency.EUR, Discount? discount = null)
            => new("valid description", quantity, new Money(unitPrice, currency), discount);

        #region Creation
        [Fact]
        public void CreateDraft_WithValidData_IsAnEmptyDraft()
        {
            Quote quote = Quote.CreateDraft(sellerId, clientId, Currency.EUR);

            Assert.NotEqual(Guid.Empty, quote.Id);
            Assert.Equal(sellerId, quote.SellerId);
            Assert.Equal(clientId, quote.ClientId);
            Assert.Equal(Currency.EUR, quote.Currency);
            Assert.Equal(QuoteStatus.Draft, quote.Status);
            Assert.Empty(quote.Lines);
        }

        [Fact]
        public void CreateDraft_WithEmptySellerId_Throws()
        {
            var action = () => Quote.CreateDraft(Guid.Empty, clientId, Currency.EUR);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("sellerId", exception.ParamName);
        }

        [Fact]
        public void CreateDraft_WithEmptyClientId_Throws()
        {
            var action = () => Quote.CreateDraft(sellerId, Guid.Empty, Currency.EUR);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("clientId", exception.ParamName);
        }

        [Fact]
        public void CreateDraft_WithUndefinedCurrency_Throws()
        {
            var action = () => Quote.CreateDraft(sellerId, clientId, (Currency)999);

            Assert.Throws<ArgumentException>(action);
        }
        #endregion

        #region AddLine
        [Fact]
        public void AddLine_OnDraft_LineIsInLines()
        {
            Quote quote = NewEuroQuote();
            QuoteLine line = Line(2m, 500m);

            quote.AddLine(line);

            Assert.Single(quote.Lines);
            Assert.Same(line, quote.Lines[0]);
        }

        [Fact]
        public void AddLine_Twice_KeepsTheOrder()
        {
            Quote quote = NewEuroQuote();
            QuoteLine first = Line(1m, 100m);
            QuoteLine second = Line(1m, 200m);

            quote.AddLine(first);
            quote.AddLine(second);

            Assert.Equal(new[] { first, second }, quote.Lines);
        }

        [Fact]
        public void AddLine_WithDifferentCurrency_Throws()
        {
            Quote quote = NewEuroQuote();

            var action = () => quote.AddLine(Line(2m, 500m, Currency.GBP));

            Assert.Throws<ArgumentException>(action);
            Assert.Empty(quote.Lines);
        }

        [Fact]
        public void AddLine_WithNullLine_Throws()
        {
            Quote quote = NewEuroQuote();

            var action = () => quote.AddLine(null!); // ! : null is forced on purpose, it is the case under test

            Assert.Throws<ArgumentNullException>(action);
        }
        #endregion

        #region Total
        [Fact]
        public void TotalExclTax_WithoutLines_IsZeroInQuoteCurrency()
        {
            Quote quote = NewEuroQuote();

            Assert.Equal(Money.Zero(Currency.EUR), quote.TotalExclTax);
        }

        [Fact]
        public void TotalExclTax_WithSeveralLines_IsTheSumOfTheLines()
        {
            Quote quote = NewEuroQuote();
            quote.AddLine(Line(2m, 134.5m));
            quote.AddLine(Line(3m, 10m));

            Assert.Equal(new Money(299m, Currency.EUR), quote.TotalExclTax);
        }

        [Fact]
        public void TotalExclTax_WithDiscountedLine_UsesTheDiscountedLineTotal()
        {
            Quote quote = NewEuroQuote();
            quote.AddLine(Line(3m, 500m, discount: new PercentageDiscount(10m)));
            quote.AddLine(Line(1m, 50m));

            Assert.Equal(new Money(1400m, Currency.EUR), quote.TotalExclTax);
        }

        [Fact]
        public void TotalExclTax_AfterAddLine_IsRecomputed()
        {
            Quote quote = NewEuroQuote();
            quote.AddLine(Line(1m, 100m));
            Money before = quote.TotalExclTax;

            quote.AddLine(Line(1m, 20m));

            Assert.Equal(new Money(100m, Currency.EUR), before);
            Assert.Equal(new Money(120m, Currency.EUR), quote.TotalExclTax);
        }

        [Fact]
        public void TotalExclTax_WithFixedDiscount_IsTheSumOfTheLines()
        {
            FixedAmountDiscount fixedDiscount = new(new Money(33m, Currency.EUR));
            Quote quote = Quote.CreateDraft(sellerId, clientId, Currency.EUR, fixedDiscount);
            quote.AddLine(Line(2m, 134.5m));
            quote.AddLine(Line(3m, 10m));

            Assert.Equal(new Money(266m, Currency.EUR), quote.TotalExclTax);
        }

        [Fact]
        public void TotalExclTax_WithPercentDiscount_IsTheSumOfTheLines()
        {
            PercentageDiscount percentDiscount = new PercentageDiscount(10m);
            Quote quote = Quote.CreateDraft(sellerId, clientId, Currency.EUR, percentDiscount);
            quote.AddLine(Line(2m, 134.5m));
            quote.AddLine(Line(3m, 10m));

            Assert.Equal(new Money(269.1m, Currency.EUR), quote.TotalExclTax);
        }
        #endregion
    }
}
