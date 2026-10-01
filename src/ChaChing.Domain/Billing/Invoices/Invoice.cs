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

        public InvoiceStatus Status { get; }

        public DateOnly? IssueDate { get; private set; }

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