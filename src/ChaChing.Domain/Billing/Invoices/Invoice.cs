using ChaChing.Domain.Common;
using ChaChing.Domain.Parties;

namespace ChaChing.Domain.Billing.Invoices
{
    public class Invoice
    {
        #region Constructor, Creator
        private Invoice(Guid sellerId, Guid clientId, Currency currency, Discount? discount = null, Guid? quoteId = null)
        {
            Id = Guid.NewGuid();

            Status = InvoiceStatus.Draft;
            SellerId = Guard.ValidateGuid(sellerId, nameof(sellerId));
            ClientId = Guard.ValidateGuid(clientId, nameof(clientId));
            Currency = Guard.ValidateEnum(currency, nameof(currency));
            QuoteId = Guard.ValidateNullableGuid(quoteId, nameof(quoteId));
            Discount = discount;
        }

        public static Invoice CreateDraft(Guid sellerId, Guid clientId, Currency currency, Discount? discount = null, Guid? quoteId = null)
        {
            return new Invoice(sellerId, clientId, currency, discount, quoteId);
        }
        #endregion

        #region Fields
        public Guid Id { get; }

        public BillingNumber? Number { get; private set; }

        public Guid? QuoteId { get; }

        public InvoiceStatus Status { get; private set; }

        public DateOnly? IssueDate { get; private set; }

        public DateOnly? DueDate { get; private set; }

        public DateOnly? PaymentDate { get; private set; }

        public Guid SellerId { get; }

        public Guid ClientId { get; }

        public SellerSnapshot? SellerSnapshot { get; private set; }

        public ClientSnapshot? ClientSnapshot { get; private set; }

        private readonly List<BillingLine> _lines = new();
        public IReadOnlyList<BillingLine> Lines => _lines;

        public Currency Currency { get; }

        public Discount? Discount { get; }

        /// <summary>Lines total, discount on the total, VAT breakdown: computed on read, never stored.</summary>
        public BillingTotals Totals => BillingTotals.Compute(_lines, Discount, Currency);
        #endregion

        #region Lines (draft only)
        public void AddLine(BillingLine line)
        {
            EnsureStatus(InvoiceStatus.Draft);
            ArgumentNullException.ThrowIfNull(line);
            EnsureSameCurrency(line.UnitPrice.Currency, nameof(line));

            _lines.Add(line);
        }
        #endregion

        #region Lifecycle
        public void MarkedAsIssued(BillingNumber number, DateOnly issueDate, DateOnly dueDate, SellerSnapshot seller, ClientSnapshot client)
        {
            EnsureStatus(InvoiceStatus.Draft);
            ArgumentNullException.ThrowIfNull(number);
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(seller);

            if (dueDate < issueDate)
                throw new ArgumentException($"The due date {dueDate} can't be before the issue date {issueDate}.", nameof(dueDate));

            if (_lines.Count == 0)
                throw new InvalidOperationException("An invoice without lines cannot be issued.");

            if (client.ClientId != ClientId)
                throw new ArgumentException("The client is not valid.", nameof(client));

            if (seller.SellerId != SellerId)
                throw new ArgumentException("The seller is not valid.", nameof(seller));

            Number = number.ValidateType(TypeDocument.INV);
            IssueDate = issueDate;
            DueDate = dueDate;
            SellerSnapshot = seller;
            ClientSnapshot = client;
            Status = InvoiceStatus.Issued;
        }

        public void MarkAsPaid(DateOnly paymentDate)
        {
            EnsureStatus(InvoiceStatus.Issued);

            if (paymentDate < IssueDate)
                throw new ArgumentException("The payment date is not valid.", nameof(paymentDate));

            PaymentDate = paymentDate;
            Status = InvoiceStatus.Paid;
        }

        public bool IsOverdue(DateOnly dateNow)
        {
            if (PaymentDate != null || Status != InvoiceStatus.Issued || DueDate >= dateNow)
                return false;
            else
                return true;
        }

        public void MarkAsCancelled()
        {
            EnsureStatus(InvoiceStatus.Issued);
            DueDate = null;
            Status = InvoiceStatus.Cancelled;
        }
        #endregion


        #region Validation
        private void EnsureStatus(InvoiceStatus expected)
        {
            if (Status != expected)
                throw new InvalidOperationException($"This action requires a {expected} invoice; this one is {Status}.");
        }

        private void EnsureSameCurrency(Currency currency, string paramName)
        {
            if (currency != Currency)
                throw new ArgumentException("An invoice holds a single currency.", paramName);
        }
        #endregion
    }
}