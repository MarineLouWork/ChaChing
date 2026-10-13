using ChaChing.Domain.Billing;
using ChaChing.Domain.Billing.Invoices;
using ChaChing.Domain.Common;

namespace ChaChing.Domain.Tests
{

    public class InvoiceTests
    {
        static readonly Guid clientId = Guid.NewGuid();
        static readonly Guid sellerId = Guid.NewGuid();

        static Invoice NewEuroInvoice() => Invoice.CreateDraft(sellerId, clientId, Currency.EUR);

        static BillingLine Line(decimal unitPrice, Currency currency = Currency.EUR)
            => new("valid description", 1m, new Money(unitPrice, currency));

        #region Creation
        [Fact]
        public void CreateDraft_WithValidData_IsAnEmptyDraftWithoutQuote()
        {
            Invoice invoice = Invoice.CreateDraft(sellerId, clientId, Currency.EUR);

            Assert.NotEqual(Guid.Empty, invoice.Id);
            Assert.Null(invoice.QuoteId);
            Assert.Null(invoice.Number);
            Assert.Equal(InvoiceStatus.Draft, invoice.Status);
            Assert.Equal(sellerId, invoice.SellerId);
            Assert.Equal(clientId, invoice.ClientId);
            Assert.Equal(Currency.EUR, invoice.Currency);
            Assert.Empty(invoice.Lines);
        }

        [Fact]
        public void CreateDraft_FromAQuote_KeepsTheQuoteId()
        {
            Guid quoteId = Guid.NewGuid();

            Invoice invoice = Invoice.CreateDraft(sellerId, clientId, Currency.EUR, quoteId: quoteId);

            Assert.Equal(quoteId, invoice.QuoteId);
        }

        [Fact]
        public void CreateDraft_WithEmptyQuoteId_Throws()
        {
            var action = () => Invoice.CreateDraft(sellerId, clientId, Currency.EUR, quoteId: Guid.Empty);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("quoteId", exception.ParamName);
        }

        [Fact]
        public void CreateDraft_WithEmptySellerId_Throws()
        {
            var action = () => Invoice.CreateDraft(Guid.Empty, clientId, Currency.EUR);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("sellerId", exception.ParamName);
        }

        [Fact]
        public void CreateDraft_WithEmptyClientId_Throws()
        {
            var action = () => Invoice.CreateDraft(sellerId, Guid.Empty, Currency.EUR);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("clientId", exception.ParamName);
        }

        [Fact]
        public void CreateDraft_WithUndefinedCurrency_Throws()
        {
            var action = () => Invoice.CreateDraft(sellerId, clientId, (Currency)999);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("currency", exception.ParamName);
        }
        #endregion

        #region AddLine
        [Fact]
        public void AddLine_OnDraft_LineIsInLines()
        {
            Invoice invoice = NewEuroInvoice();
            BillingLine line = Line(500m);

            invoice.AddLine(line);

            Assert.Same(line, Assert.Single(invoice.Lines));
        }

        [Fact]
        public void AddLine_WithDifferentCurrency_Throws()
        {
            Invoice invoice = NewEuroInvoice();
            BillingLine line = Line(500m, Currency.USD);

            var action = () => invoice.AddLine(line);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("line", exception.ParamName);
        }

        [Fact]
        public void AddLine_WithNullLine_Throws()
        {
            Invoice invoice = NewEuroInvoice();

            var action = () => invoice.AddLine(null!);

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("line", exception.ParamName);
        }
        #endregion

        #region Totals
        [Fact]
        public void Totals_WithLinesAndDiscount_AreComputedLikeAnyBillingDocument()
        {
            // The computation itself is covered by QuoteVatTests: here, only that the invoice uses it
            Invoice invoice = Invoice.CreateDraft(sellerId, clientId, Currency.EUR, new PercentageDiscount(10m));
            invoice.AddLine(Line(100m));
            invoice.AddLine(Line(200m));

            Assert.Equal(new Money(300m, Currency.EUR), invoice.Totals.TotalNet);
            Assert.Equal(new Money(270m, Currency.EUR), invoice.Totals.TotalWithoutVat);
            Assert.Equal(new Money(324m, Currency.EUR), invoice.Totals.TotalWithVat);
        }
        #endregion
    }
}
