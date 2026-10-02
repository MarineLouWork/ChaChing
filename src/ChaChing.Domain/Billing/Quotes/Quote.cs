using ChaChing.Domain.Common;
using ChaChing.Domain.Parties;

namespace ChaChing.Domain.Billing.Quotes
{
    public class Quote
    {
        #region Constructor, Creator
        private Quote(Guid sellerId, Guid clientId, Currency currency, Discount? discount = null)
        {
            Id = Guid.NewGuid();
            Status = QuoteStatus.Draft;
            SellerId = Guard.ValidateGuid(sellerId, nameof(sellerId));
            ClientId = Guard.ValidateGuid(clientId, nameof(clientId));
            Currency = Guard.ValidateEnum(currency, nameof(currency));
            Discount = discount;
        }

        public static Quote CreateDraft(Guid sellerId, Guid clientId, Currency currency, Discount? discount = null)
        {
            return new Quote(sellerId, clientId, currency, discount);
        }
        #endregion

        #region Fields
        public Guid Id { get; }

        public BillingNumber? Number { get; private set; }

        public QuoteStatus Status { get; private set; }

        public DateOnly? IssueDate { get; private set; }

        public DateOnly? ValidUntil { get; private set; }

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

        #region Lifecycle (ADR 0018, 0019)
        public void MarkAsSent(BillingNumber number, DateOnly issueDate, DateOnly validUntil, SellerSnapshot seller, ClientSnapshot client)
        {
            ArgumentNullException.ThrowIfNull(number);
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(seller);

            EnsureStatus(QuoteStatus.Draft);

            if (validUntil < issueDate)
                throw new ArgumentException("The validity date cannot be before the issue date.", nameof(validUntil));

            if (_lines.Count == 0)
                throw new InvalidOperationException("A quote without lines cannot be sent.");

            if (Number != null && Number != number)
                throw new InvalidOperationException($"This quote already has the number {Number}.");

            if (client.ClientId != ClientId)
                throw new ArgumentException("The client is not valid.", nameof(client));

            if (seller.SellerId != SellerId)
                throw new ArgumentException("The seller is not valid.", nameof(seller));

            if (seller.VatRegime == VatRegime.Exempt && _lines.Any(line => !line.VatRate.IsExempt))
                throw new InvalidOperationException("An exempt seller cannot send lines with VAT.");

            Number = number.ValidateType(TypeDocument.QUO);
            IssueDate = issueDate;
            ValidUntil = validUntil;
            Status = QuoteStatus.Sent;
            ClientSnapshot = client;
            SellerSnapshot = seller;
        }

        public void MarkAsAccepted()
        {
            EnsureStatus(QuoteStatus.Sent);
            Status = QuoteStatus.Accepted;
        }

        public void MarkAsDeclined()
        {
            EnsureStatus(QuoteStatus.Sent);
            Status = QuoteStatus.Declined;
        }

        public void ReturnToDraft()
        {
            EnsureStatus(QuoteStatus.Declined);
            Status = QuoteStatus.Draft;
        }
        #endregion

        #region Lines (draft only)
        public void AddLine(BillingLine line)
        {
            EnsureStatus(QuoteStatus.Draft);
            ArgumentNullException.ThrowIfNull(line);
            EnsureSameCurrency(line.UnitPrice.Currency, nameof(line));

            _lines.Add(line);
        }

        public void RemoveLine(Guid lineId)
        {
            EnsureStatus(QuoteStatus.Draft);
            _lines.Remove(FindLine(lineId));
        }

        public void UpdateLine(Guid lineId, string description, decimal quantity, Money unitPrice, Discount? discount = null, VatRate? vatRate = null)
        {
            EnsureStatus(QuoteStatus.Draft);
            BillingLine line = FindLine(lineId);
            EnsureSameCurrency(unitPrice.Currency, nameof(unitPrice));
            line.Update(description, quantity, unitPrice, discount, vatRate);
        }

        private BillingLine FindLine(Guid lineId) => _lines.FirstOrDefault(l => l.Id == lineId) ?? throw new ArgumentException("The line is not in this quote.", nameof(lineId));

        #endregion

        #region Validation
        private void EnsureStatus(QuoteStatus expected)
        {
            if (Status != expected)
                throw new InvalidOperationException($"This action requires a {expected} quote; this one is {Status}.");
        }

        private void EnsureSameCurrency(Currency currency, string paramName)
        {
            if (currency != Currency)
                throw new ArgumentException("A quote holds a single currency.", paramName);
        }
        #endregion
    }
}
