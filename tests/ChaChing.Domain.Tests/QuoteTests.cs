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
        public void TotalExclTax_WithFixedDiscount_Applies()
        {
            FixedAmountDiscount fixedDiscount = new(new Money(33m, Currency.EUR));

            Quote quote = Quote.CreateDraft(sellerId, clientId, Currency.EUR, fixedDiscount);
            quote.AddLine(Line(2m, 134.5m));
            quote.AddLine(Line(3m, 10m));

            Assert.Equal(new Money(266m, Currency.EUR), quote.TotalExclTax);
        }

        [Fact]
        public void TotalExclTax_WithPercentDiscount_Applies()
        {
            Quote quote = Quote.CreateDraft(sellerId, clientId, Currency.EUR, new PercentageDiscount(10m));
            quote.AddLine(Line(2m, 134.5m));
            quote.AddLine(Line(3m, 10m));

            Assert.Equal(new Money(269.1m, Currency.EUR), quote.TotalExclTax);
        }

        [Fact]
        public void TotalExclTax_WithDiscountedLine_ThenWithDiscountedTotal()
        {
            Quote quote = Quote.CreateDraft(sellerId, clientId, Currency.EUR, new PercentageDiscount(10m));
            quote.AddLine(Line(3m, 500m, discount: new PercentageDiscount(10m)));
            quote.AddLine(Line(1m, 50m));

            Assert.Equal(new Money(1260m, Currency.EUR), quote.TotalExclTax);
        }

        [Fact]
        public void TotalExclTax_WithDiscountedLineThenWithDiscountedTotal_FloorToZero()
        {
            FixedAmountDiscount fixedDiscount = new(new Money(1500m, Currency.EUR));

            Quote quote = Quote.CreateDraft(sellerId, clientId, Currency.EUR, fixedDiscount);
            quote.AddLine(Line(3m, 500m, discount: new PercentageDiscount(10m)));
            quote.AddLine(Line(1m, 50m));

            Assert.Equal(new Money(0m, Currency.EUR), quote.TotalExclTax);
        }
        #endregion

        #region RemoveLine
        [Fact]
        public void RemoveLine_ExistingLine_OnlyThisLineIsRemoved()
        {
            Quote quote = NewEuroQuote();
            QuoteLine kept = Line(1m, 100m);
            QuoteLine removed = Line(1m, 200m);
            quote.AddLine(kept);
            quote.AddLine(removed);

            quote.RemoveLine(removed.Id);

            Assert.Equal(new[] { kept }, quote.Lines);
            Assert.Equal(new Money(100m, Currency.EUR), quote.TotalExclTax);
        }

        [Fact]
        public void RemoveLine_UnknownLine_ThrowsAndKeepsTheLines()
        {
            Quote quote = NewEuroQuote();
            quote.AddLine(Line(1m, 100m));

            var action = () => quote.RemoveLine(Guid.NewGuid());

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("lineId", exception.ParamName);
            Assert.Single(quote.Lines);
        }
        #endregion

        #region UpdateLine
        [Fact]
        public void UpdateLine_WithValidData_UpdatesTheSameLine()
        {
            Quote quote = NewEuroQuote();
            QuoteLine line = Line(1m, 100m);
            quote.AddLine(line);
            Discount discount = new PercentageDiscount(10m);

            quote.UpdateLine(line.Id, "new description", 3m, new Money(50m, Currency.EUR), discount);

            QuoteLine updated = Assert.Single(quote.Lines);
            Assert.Equal(line.Id, updated.Id);
            Assert.Equal("new description", updated.Description);
            Assert.Equal(3m, updated.Quantity);
            Assert.Equal(new Money(50m, Currency.EUR), updated.UnitPrice);
            Assert.Same(discount, updated.Discount);
        }

        [Fact]
        public void UpdateLine_WithValidData_TotalIsRecomputed()
        {
            Quote quote = NewEuroQuote();
            QuoteLine line = Line(1m, 100m);
            quote.AddLine(line);

            quote.UpdateLine(line.Id, "valid description", 3m, new Money(50m, Currency.EUR));

            Assert.Equal(new Money(150m, Currency.EUR), quote.TotalExclTax);
        }

        [Fact]
        public void UpdateLine_UnknownLine_Throws()
        {
            Quote quote = NewEuroQuote();
            quote.AddLine(Line(1m, 100m));

            var action = () => quote.UpdateLine(Guid.NewGuid(), "valid description", 1m, new Money(10m, Currency.EUR));

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("lineId", exception.ParamName);
        }

        [Fact]
        public void UpdateLine_WithInvalidQuantity_ThrowsAndLeavesTheLineUnchanged()
        {
            Quote quote = NewEuroQuote();
            QuoteLine line = Line(1m, 100m);
            quote.AddLine(line);

            // the description is valid and checked first: it must not be applied alone
            var action = () => quote.UpdateLine(line.Id, "new description", 0m, new Money(50m, Currency.EUR));

            Assert.Throws<ArgumentException>(action);
            Assert.Equal("valid description", line.Description);
            Assert.Equal(1m, line.Quantity);
            Assert.Equal(new Money(100m, Currency.EUR), line.UnitPrice);
        }

        [Fact]
        public void UpdateLine_WithDifferentCurrency_ThrowsAndLeavesTheLineUnchanged()
        {
            Quote quote = NewEuroQuote();
            QuoteLine line = Line(1m, 100m);
            quote.AddLine(line);

            var action = () => quote.UpdateLine(line.Id, "new description", 2m, new Money(50m, Currency.GBP));

            Assert.Throws<ArgumentException>(action);
            Assert.Equal("valid description", line.Description);
            Assert.Equal(new Money(100m, Currency.EUR), line.UnitPrice);
        }
        #endregion
    }
}
