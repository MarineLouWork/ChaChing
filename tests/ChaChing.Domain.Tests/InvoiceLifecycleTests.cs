using ChaChing.Domain.Billing;
using ChaChing.Domain.Billing.Invoices;
using ChaChing.Domain.Common;
using ChaChing.Domain.Parties;

namespace ChaChing.Domain.Tests
{
    public class InvoiceLifecycleTests
    {
        static readonly BillingNumber number = new(TypeDocument.INV, 2026, 12);
        static readonly DateOnly issueDate = new(2026, 10, 15);
        static readonly DateOnly dueDate = new(2026, 11, 14);
        static readonly Seller seller = TestData.Seller();
        static readonly Client client = TestData.Client(seller.Id);
        static readonly SellerSnapshot sellerSnapshot = SellerSnapshot.From(seller);
        static readonly ClientSnapshot clientSnapshot = ClientSnapshot.From(client);

        static Invoice DraftWithOneLine()
        {
            Invoice invoice = Invoice.CreateDraft(seller.Id, client.Id, Currency.EUR);
            invoice.AddLine(new BillingLine("valid description", 1m, new Money(100m, Currency.EUR)));
            return invoice;
        }

        static Invoice IssuedInvoice()
        {
            Invoice invoice = DraftWithOneLine();
            invoice.MarkedAsIssued(number, issueDate, dueDate, sellerSnapshot, clientSnapshot);
            return invoice;
        }

        #region Issue
        [Fact]
        public void Issue_WithoutLines_Throws()
        {
            Invoice invoice = Invoice.CreateDraft(seller.Id, client.Id, Currency.EUR);

            var action = () => invoice.MarkedAsIssued(number, issueDate, dueDate, sellerSnapshot, clientSnapshot);

            Assert.Throws<InvalidOperationException>(action);
            Assert.Equal(InvoiceStatus.Draft, invoice.Status);
        }

        [Fact]
        public void Issue_FreezesNumberDatesAndSnapshots()
        {
            Invoice invoice = DraftWithOneLine();

            invoice.MarkedAsIssued(number, issueDate, dueDate, sellerSnapshot, clientSnapshot);

            Assert.Equal(InvoiceStatus.Issued, invoice.Status);
            Assert.Equal(number, invoice.Number);
            Assert.Equal(issueDate, invoice.IssueDate);
            Assert.Equal(dueDate, invoice.DueDate);
            Assert.Same(sellerSnapshot, invoice.SellerSnapshot);
            Assert.Same(clientSnapshot, invoice.ClientSnapshot);
        }

        [Fact]
        public void Issue_WithAQuoteNumber_ThrowsAndStaysDraft()
        {
            Invoice invoice = DraftWithOneLine();
            BillingNumber quoteNumber = new(TypeDocument.QUO, 2026, 12);

            var action = () => invoice.MarkedAsIssued(quoteNumber, issueDate, dueDate, sellerSnapshot, clientSnapshot);

            Assert.Throws<InvalidOperationException>(action);
            Assert.Equal(InvoiceStatus.Draft, invoice.Status);
            Assert.Null(invoice.Number);
        }

        [Fact]
        public void Issue_WithDueDateBeforeIssueDate_Throws()
        {
            Invoice invoice = DraftWithOneLine();

            var action = () => invoice.MarkedAsIssued(number, issueDate, issueDate.AddDays(-1), sellerSnapshot, clientSnapshot);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("dueDate", exception.ParamName);
        }

        [Fact]
        public void Issue_WithDueDateEqualToIssueDate_IsAccepted()
        {
            Invoice invoice = DraftWithOneLine();

            invoice.MarkedAsIssued(number, issueDate, issueDate, sellerSnapshot, clientSnapshot);

            Assert.Equal(issueDate, invoice.DueDate);
        }

        [Fact]
        public void Issue_OnAlreadyIssuedInvoice_ThrowsAndKeepsItsNumber()
        {
            Invoice invoice = IssuedInvoice();
            BillingNumber otherNumber = new(TypeDocument.INV, 2026, 13);

            var action = () => invoice.MarkedAsIssued(otherNumber, issueDate, dueDate, sellerSnapshot, clientSnapshot);

            Assert.Throws<InvalidOperationException>(action);
            Assert.Equal(number, invoice.Number);
        }

        [Fact]
        public void Issue_WithNullNumber_Throws()
        {
            Invoice invoice = DraftWithOneLine();

            var action = () => invoice.MarkedAsIssued(null!, issueDate, dueDate, sellerSnapshot, clientSnapshot); // ! : null is forced on purpose, it is the case under test

            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Issue_WithNullSellerSnapshot_Throws()
        {
            Invoice invoice = DraftWithOneLine();

            var action = () => invoice.MarkedAsIssued(number, issueDate, dueDate, null!, clientSnapshot); // ! : null is forced on purpose, it is the case under test

            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Issue_WithNullClientSnapshot_Throws()
        {
            Invoice invoice = DraftWithOneLine();

            var action = () => invoice.MarkedAsIssued(number, issueDate, dueDate, sellerSnapshot, null!); // ! : null is forced on purpose, it is the case under test

            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Issue_WithAnotherClientSnapshot_ThrowsAndStaysDraft()
        {
            Invoice invoice = DraftWithOneLine();
            ClientSnapshot otherClient = ClientSnapshot.From(TestData.Client(seller.Id));

            var action = () => invoice.MarkedAsIssued(number, issueDate, dueDate, sellerSnapshot, otherClient);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("client", exception.ParamName);
            Assert.Equal(InvoiceStatus.Draft, invoice.Status);
            Assert.Null(invoice.ClientSnapshot);
        }

        [Fact]
        public void Issue_WithAnotherSellerSnapshot_ThrowsAndStaysDraft()
        {
            Invoice invoice = DraftWithOneLine();
            SellerSnapshot otherSeller = SellerSnapshot.From(TestData.Seller());

            var action = () => invoice.MarkedAsIssued(number, issueDate, dueDate, otherSeller, clientSnapshot);

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("seller", exception.ParamName);
            Assert.Equal(InvoiceStatus.Draft, invoice.Status);
            Assert.Null(invoice.SellerSnapshot);
        }

        [Fact]
        public void Issue_SnapshotStaysFrozen_WhenTheClientIsLaterRenamed()
        {
            Invoice invoice = IssuedInvoice();
            Client renamed = Client.Rehydrate(client.Id, seller.Id, "Winterfell & Fils", "contact@winterfell.fr", TestData.Address(), "123456789");

            Assert.Equal("Winterfell SAS", invoice.ClientSnapshot!.Name);
            Assert.NotEqual(renamed.Name, invoice.ClientSnapshot.Name);
        }
        #endregion

        #region Untouchable once issued
        [Fact]
        public void AddLine_OnIssuedInvoice_Throws()
        {
            Invoice invoice = IssuedInvoice();

            var action = () => invoice.AddLine(new BillingLine("valid description", 1m, new Money(50m, Currency.EUR)));

            Assert.Throws<InvalidOperationException>(action);
            Assert.Single(invoice.Lines);
        }
        #endregion

        #region MarkAsPaid
        [Fact]
        public void MarkAsPaid_OnIssuedInvoice_IsPaidWithItsDate()
        {
            Invoice invoice = IssuedInvoice();

            invoice.MarkAsPaid(new DateOnly(2026, 11, 20));

            Assert.Equal(InvoiceStatus.Paid, invoice.Status);
            Assert.Equal(new DateOnly(2026, 11, 20), invoice.PaymentDate);
        }

        [Fact]
        public void MarkAsPaid_OnDraft_Throws()
        {
            Invoice invoice = DraftWithOneLine();

            var action = () => invoice.MarkAsPaid(new DateOnly(2026, 11, 20));

            Assert.Throws<InvalidOperationException>(action);
            Assert.Null(invoice.PaymentDate);
        }

        [Fact]
        public void MarkAsPaid_OnPaidInvoice_ThrowsAndKeepsTheFirstDate()
        {
            Invoice invoice = IssuedInvoice();
            invoice.MarkAsPaid(new DateOnly(2026, 11, 20));

            var action = () => invoice.MarkAsPaid(new DateOnly(2026, 11, 25));

            Assert.Throws<InvalidOperationException>(action);
            Assert.Equal(new DateOnly(2026, 11, 20), invoice.PaymentDate);
        }

        [Fact]
        public void MarkAsPaid_BeforeTheIssueDate_ThrowsAndStaysIssued()
        {
            Invoice invoice = IssuedInvoice();

            var action = () => invoice.MarkAsPaid(issueDate.AddDays(-1));

            var exception = Assert.Throws<ArgumentException>(action);
            Assert.Equal("paymentDate", exception.ParamName);
            Assert.Equal(InvoiceStatus.Issued, invoice.Status);
            Assert.Null(invoice.PaymentDate);
        }

        [Fact]
        public void MarkAsPaid_OnTheIssueDate_IsAccepted()
        {
            Invoice invoice = IssuedInvoice();

            invoice.MarkAsPaid(issueDate);

            Assert.Equal(issueDate, invoice.PaymentDate);
        }
        #endregion

        #region IsOverdue (today is a parameter, ADR 0029)
        [Fact]
        public void IsOverdue_OnDraft_IsFalse()
        {
            Invoice invoice = DraftWithOneLine();

            Assert.False(invoice.IsOverdue(new DateOnly(2030, 1, 1)));
        }

        [Fact]
        public void IsOverdue_OnTheDueDate_IsFalse()
        {
            Invoice invoice = IssuedInvoice();

            Assert.False(invoice.IsOverdue(dueDate));
        }

        [Fact]
        public void IsOverdue_TheDayAfterTheDueDate_IsTrue()
        {
            Invoice invoice = IssuedInvoice();

            Assert.True(invoice.IsOverdue(dueDate.AddDays(1)));
        }

        [Fact]
        public void IsOverdue_WhenPaid_IsFalse()
        {
            Invoice invoice = IssuedInvoice();
            invoice.MarkAsPaid(new DateOnly(2026, 11, 20));

            Assert.False(invoice.IsOverdue(new DateOnly(2026, 12, 31)));
        }
        #endregion
    }
}
