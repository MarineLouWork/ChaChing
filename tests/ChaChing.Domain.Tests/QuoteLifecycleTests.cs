using ChaChing.Domain.Billing;
using ChaChing.Domain.Billing.Quotes;
using ChaChing.Domain.Common;
using ChaChing.Domain.Parties;

namespace ChaChing.Domain.Tests
{
    public class QuoteLifecycleTests
    {
        static readonly BillingNumber number = new(TypeDocument.DEV, 2026, 42);
        static readonly DateOnly issueDate = new(2026, 9, 28);
        static readonly DateOnly validUntil = new(2026, 10, 28);
        static readonly Seller seller = TestData.Seller();
        static readonly Client client = TestData.Client(seller.Id);
        static readonly SellerSnapshot sellerSnapshot = SellerSnapshot.From(seller);
        static readonly ClientSnapshot clientSnapshot = ClientSnapshot.From(client);

        static BillingLine Line() => new("valid description", 1m, new Money(100m, Currency.EUR));

        static Quote DraftWithOneLine()
        {
            Quote quote = Quote.CreateDraft(seller.Id, client.Id, Currency.EUR);
            quote.AddLine(Line());
            return quote;
        }

        static Quote SentQuote()
        {
            Quote quote = DraftWithOneLine();
            quote.MarkAsSent(number, issueDate, validUntil, sellerSnapshot, clientSnapshot);
            return quote;
        }

        static Quote DeclinedQuote()
        {
            Quote quote = SentQuote();
            quote.MarkAsDeclined();
            return quote;
        }

        static Quote AcceptedQuote()
        {
            Quote quote = SentQuote();
            quote.MarkAsAccepted();
            return quote;
        }

        #region MarkAsSent
        [Fact]
        public void MarkAsSent_DraftWithLines_IsSentWithNumberAndDates()
        {
            Quote quote = DraftWithOneLine();

            quote.MarkAsSent(number, issueDate, validUntil, sellerSnapshot, clientSnapshot);

            Assert.Equal(QuoteStatus.Sent, quote.Status);
            Assert.Equal(number, quote.Number);
            Assert.Equal(issueDate, quote.IssueDate);
            Assert.Equal(validUntil, quote.ValidUntil);
        }

        [Fact]
        public void MarkAsSent_DraftWithoutLines_ThrowsAndStaysDraft()
        {
            Quote quote = Quote.CreateDraft(seller.Id, client.Id, Currency.EUR);

            var action = () => quote.MarkAsSent(number, issueDate, validUntil, sellerSnapshot, clientSnapshot);

            Assert.Throws<InvalidOperationException>(action);
            Assert.Equal(QuoteStatus.Draft, quote.Status);
            Assert.Null(quote.Number);
        }

        [Fact]
        public void MarkAsSent_WithNullNumber_Throws()
        {
            Quote quote = DraftWithOneLine();

            var action = () => quote.MarkAsSent(null!, issueDate, validUntil, sellerSnapshot, clientSnapshot);

            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void MarkAsSent_ValidUntilBeforeIssueDate_ThrowsAndStaysDraft()
        {
            Quote quote = DraftWithOneLine();

            var action = () => quote.MarkAsSent(number, issueDate, issueDate.AddDays(-1), sellerSnapshot, clientSnapshot);

            Assert.Throws<ArgumentException>(action);
            Assert.Equal(QuoteStatus.Draft, quote.Status);
        }

        [Fact]
        public void MarkAsSent_ValidUntilSameDayAsIssueDate_IsSent()
        {
            Quote quote = DraftWithOneLine();

            quote.MarkAsSent(number, issueDate, issueDate, sellerSnapshot, clientSnapshot);

            Assert.Equal(QuoteStatus.Sent, quote.Status);
        }

        [Fact]
        public void MarkAsSent_AlreadySent_Throws()
        {
            Quote quote = SentQuote();

            var action = () => quote.MarkAsSent(number, issueDate, validUntil, sellerSnapshot, clientSnapshot);

            Assert.Throws<InvalidOperationException>(action);
        }
        #endregion

        #region MarkAsAccepted / MarkAsDeclined
        [Fact]
        public void MarkAsAccepted_Sent_IsAccepted()
        {
            Quote quote = SentQuote();

            quote.MarkAsAccepted();

            Assert.Equal(QuoteStatus.Accepted, quote.Status);
        }

        [Fact]
        public void MarkAsDeclined_Sent_IsDeclined()
        {
            Quote quote = SentQuote();

            quote.MarkAsDeclined();

            Assert.Equal(QuoteStatus.Declined, quote.Status);
        }

        [Fact]
        public void MarkAsAccepted_Draft_Throws()
        {
            Quote quote = DraftWithOneLine();

            var action = () => quote.MarkAsAccepted();

            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void MarkAsDeclined_Draft_Throws()
        {
            Quote quote = DraftWithOneLine();

            var action = () => quote.MarkAsDeclined();

            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void MarkAsDeclined_Accepted_Throws()
        {
            Quote quote = AcceptedQuote();

            var action = () => quote.MarkAsDeclined();

            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void MarkAsAccepted_Declined_Throws()
        {
            Quote quote = DeclinedQuote();

            var action = () => quote.MarkAsAccepted();

            Assert.Throws<InvalidOperationException>(action);
        }
        #endregion

        #region ReturnToDraft
        [Fact]
        public void ReturnToDraft_Declined_IsDraftAndKeepsItsNumber()
        {
            Quote quote = DeclinedQuote();

            quote.ReturnToDraft();

            Assert.Equal(QuoteStatus.Draft, quote.Status);
            Assert.Equal(number, quote.Number);
        }

        [Fact]
        public void ReturnToDraft_Accepted_Throws()
        {
            Quote quote = AcceptedQuote();

            var action = () => quote.ReturnToDraft();

            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void ReturnToDraft_Sent_Throws()
        {
            Quote quote = SentQuote();

            var action = () => quote.ReturnToDraft();

            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void MarkAsSent_ReworkedQuoteWithItsOwnNumber_IsSentAgain()
        {
            Quote quote = DeclinedQuote();
            quote.ReturnToDraft();

            quote.MarkAsSent(number, issueDate.AddDays(5), validUntil.AddDays(5), sellerSnapshot, clientSnapshot);

            Assert.Equal(QuoteStatus.Sent, quote.Status);
            Assert.Equal(issueDate.AddDays(5), quote.IssueDate);
        }

        [Fact]
        public void MarkAsSent_ReworkedQuoteWithAnotherNumber_Throws()
        {
            Quote quote = DeclinedQuote();
            quote.ReturnToDraft();

            var action = () => quote.MarkAsSent(new BillingNumber(TypeDocument.DEV, 2026, 43), issueDate, validUntil, sellerSnapshot, clientSnapshot);

            Assert.Throws<InvalidOperationException>(action);
        }
        #endregion

        #region Only a draft can be edited
        [Fact]
        public void AddLine_OnSentQuote_Throws()
        {
            Quote quote = SentQuote();

            var action = () => quote.AddLine(Line());

            Assert.Throws<InvalidOperationException>(action);
            Assert.Single(quote.Lines);
        }

        [Fact]
        public void RemoveLine_OnSentQuote_Throws()
        {
            Quote quote = SentQuote();

            Guid lineId = quote.Lines[0].Id;

            var action = () => quote.RemoveLine(lineId);

            Assert.Throws<InvalidOperationException>(action);
            Assert.Single(quote.Lines);
        }

        [Fact]
        public void UpdateLine_OnAcceptedQuote_ThrowsAndLeavesTheLineUnchanged()
        {
            Quote quote = AcceptedQuote();
            BillingLine line = quote.Lines[0];

            var action = () => quote.UpdateLine(line.Id, "new description", 5m, new Money(1m, Currency.EUR));

            Assert.Throws<InvalidOperationException>(action);
            Assert.Equal(1m, line.Quantity);
        }

        [Fact]
        public void AddLine_OnReworkedQuote_IsAllowed()
        {
            Quote quote = DeclinedQuote();
            quote.ReturnToDraft();

            quote.AddLine(Line());

            Assert.Equal(2, quote.Lines.Count);
        }
        #endregion

        #region Snapshots (ADR 0019) — for Marine
        [Fact]
        public void MarkAsSent_TakesBothSnapshots()
        {
            Quote quote = DraftWithOneLine();

            quote.MarkAsSent(number, issueDate, validUntil, sellerSnapshot, clientSnapshot);

            Assert.Same(sellerSnapshot, quote.SellerSnapshot);
            Assert.Same(clientSnapshot, quote.ClientSnapshot);
        }

        [Fact]
        public void Draft_HasNoSnapshots()
        {
            Quote quote = DraftWithOneLine();

            Assert.Null(quote.SellerSnapshot);
            Assert.Null(quote.ClientSnapshot);
        }

        [Fact]
        public void MarkAsSent_WithNullSellerSnapshot_Throws()
        {
            Quote quote = DraftWithOneLine();

            var action = () => quote.MarkAsSent(number, issueDate, validUntil, null!, clientSnapshot); // ! : null is forced on purpose, it is the case under test

            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void MarkAsSent_WithNullClientSnapshot_Throws()
        {
            Quote quote = DraftWithOneLine();

            var action = () => quote.MarkAsSent(number, issueDate, validUntil, sellerSnapshot, null!); // ! : null is forced on purpose, it is the case under test

            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void MarkAsSent_WithAnotherSellerSnapshot_ThrowsAndStaysDraft()
        {
            Quote quote = DraftWithOneLine();
            SellerSnapshot otherSeller = SellerSnapshot.From(TestData.Seller());

            var action = () => quote.MarkAsSent(number, issueDate, validUntil, otherSeller, clientSnapshot);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("seller", exception.ParamName);
            Assert.Equal(QuoteStatus.Draft, quote.Status);
            Assert.Null(quote.SellerSnapshot);
        }

        [Fact]
        public void MarkAsSent_WithAnotherClientSnapshot_ThrowsAndStaysDraft()
        {
            Quote quote = DraftWithOneLine();
            ClientSnapshot otherClient = ClientSnapshot.From(TestData.Client(seller.Id));

            var action = () => quote.MarkAsSent(number, issueDate, validUntil, sellerSnapshot, otherClient);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("client", exception.ParamName);
            Assert.Equal(QuoteStatus.Draft, quote.Status);
            Assert.Null(quote.ClientSnapshot);
        }

        [Fact]
        public void MarkAsSent_ExemptSellerWithVatLine_ThrowsAndStaysDraft()
        {
            Seller exemptSeller = TestData.Seller(VatRegime.Exempt);
            Quote quote = Quote.CreateDraft(exemptSeller.Id, client.Id, Currency.EUR);
            quote.AddLine(Line());      // 20 % by default

            var action = () => quote.MarkAsSent(number, issueDate, validUntil, SellerSnapshot.From(exemptSeller), clientSnapshot);

            Assert.Throws<InvalidOperationException>(action);
            Assert.Equal(QuoteStatus.Draft, quote.Status);
        }

        [Fact]
        public void MarkAsSent_ExemptSellerWithExemptLines_IsSent()
        {
            Seller exemptSeller = TestData.Seller(VatRegime.Exempt);
            Quote quote = Quote.CreateDraft(exemptSeller.Id, client.Id, Currency.EUR);
            quote.AddLine(new BillingLine("valid description", 1m, new Money(100m, Currency.EUR), vatRate: VatRate.Exempt));

            quote.MarkAsSent(number, issueDate, validUntil, SellerSnapshot.From(exemptSeller), clientSnapshot);

            Assert.Equal(QuoteStatus.Sent, quote.Status);
        }
        #endregion
    }
}
