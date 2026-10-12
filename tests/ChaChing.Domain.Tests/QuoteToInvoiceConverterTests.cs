using ChaChing.Domain.Billing;
using ChaChing.Domain.Billing.Invoices;
using ChaChing.Domain.Billing.Quotes;
using ChaChing.Domain.Common;
using ChaChing.Domain.Parties;

namespace ChaChing.Domain.Tests
{
    public class QuoteToInvoiceConverterTests
    {
        static readonly BillingNumber quoteNumber = new(TypeDocument.QUO, 2026, 42);
        static readonly DateOnly issueDate = new(2026, 10, 15);
        static readonly DateOnly dueDate = new(2026, 11, 14);
        static readonly Seller seller = TestData.Seller();
        static readonly Client client = TestData.Client(seller.Id);
        static readonly SellerSnapshot sellerSnapshot = SellerSnapshot.From(seller);
        static readonly ClientSnapshot clientSnapshot = ClientSnapshot.From(client);

        static Quote QuoteWithStatus(QuoteStatus status)
        {
            Quote quote = Quote.CreateDraft(seller.Id, client.Id, Currency.EUR, new PercentageDiscount(10m));
            quote.AddLine(new BillingLine("Logo design", 2m, new Money(100m, Currency.EUR), new PercentageDiscount(5m)));
            quote.AddLine(new BillingLine("Hosting", 1m, new Money(50m, Currency.EUR), null, VatRate.Of(10m)));

            if (status == QuoteStatus.Draft)
                return quote;

            quote.MarkAsSent(quoteNumber, issueDate, dueDate, sellerSnapshot, clientSnapshot);
            if (status == QuoteStatus.Accepted)
                quote.MarkAsAccepted();
            if (status == QuoteStatus.Declined)
                quote.MarkAsDeclined();
            return quote;
        }

        static Quote AcceptedQuote() => QuoteWithStatus(QuoteStatus.Accepted);

        [Fact]
        public void Convert_AcceptedQuote_ReturnsADraftInvoiceWithoutNumber()
        {
            Quote quote = AcceptedQuote();

            Invoice invoice = QuoteToInvoiceConverter.Convert(quote);

            Assert.Equal(InvoiceStatus.Draft, invoice.Status);
            Assert.Null(invoice.Number);
        }

        [Fact]
        public void Convert_AcceptedQuote_TakesOverSellerClientCurrencyDiscountAndQuoteId()
        {
            Quote quote = AcceptedQuote();

            Invoice invoice = QuoteToInvoiceConverter.Convert(quote);

            Assert.Equal(quote.SellerId, invoice.SellerId);
            Assert.Equal(quote.ClientId, invoice.ClientId);
            Assert.Equal(quote.Currency, invoice.Currency);
            Assert.NotNull(quote.Discount);
            Assert.Same(quote.Discount, invoice.Discount);
            Assert.Equal(quote.Id, invoice.QuoteId);
        }

        [Fact]
        public void Convert_AcceptedQuote_CopiesLinesWithNewIds()
        {
            Quote quote = AcceptedQuote();

            Invoice invoice = QuoteToInvoiceConverter.Convert(quote);

            Assert.Equal(quote.Lines.Count, invoice.Lines.Count);
            for (int i = 0; i < quote.Lines.Count; i++)
            {
                BillingLine fromQuote = quote.Lines[i];
                BillingLine onInvoice = invoice.Lines[i];

                Assert.NotSame(fromQuote, onInvoice);
                Assert.NotEqual(fromQuote.Id, onInvoice.Id);
                Assert.Equal(fromQuote.Description, onInvoice.Description);
                Assert.Equal(fromQuote.Quantity, onInvoice.Quantity);
                Assert.Equal(fromQuote.UnitPrice, onInvoice.UnitPrice);
                Assert.Equal(fromQuote.VatRate, onInvoice.VatRate);
                Assert.Equal(fromQuote.TotalPrice, onInvoice.TotalPrice);
            }
            Assert.Equal(quote.Totals.TotalWithVat, invoice.Totals.TotalWithVat);
        }

        [Theory]
        [InlineData(QuoteStatus.Draft)]
        [InlineData(QuoteStatus.Sent)]
        [InlineData(QuoteStatus.Declined)]
        public void Convert_QuoteNotAccepted_Throws(QuoteStatus status)
        {
            Quote quote = QuoteWithStatus(status);

            var action = () => QuoteToInvoiceConverter.Convert(quote);

            Assert.Throws<InvalidOperationException>(action);
        }

    }
}
