using ChaChing.Domain.Billing.Discounts;
using ChaChing.Domain.Billing.Parties;
using ChaChing.Domain.Billing.Taxes;

namespace ChaChing.Domain.Billing.Quotes
{
    public class Quote
    {
        private Quote(Guid sellerId, Guid clientId, Currency currency, Discount? discount = null)
        {
            Id = Guid.NewGuid();
            SellerId = ValidateId(sellerId, nameof(sellerId));
            ClientId = ValidateId(clientId, nameof(clientId));
            Currency = ValidateCurrency(currency);
            Status = QuoteStatus.Draft;
            Discount = discount;
        }
        public static Quote CreateDraft(Guid sellerId, Guid clientId, Currency currency, Discount? discount = null) => new Quote(sellerId, clientId, currency, discount);

        public Guid Id { get; }

        public Guid SellerId { get; }

        public Guid ClientId { get; }

        public QuoteNumber? Number { get; private set; }

        public QuoteStatus Status { get; private set; }

        public DateOnly? IssueDate { get; private set; }

        public DateOnly? ValidUntil { get; private set; }

        private readonly List<QuoteLine> _lines = new();
        public IReadOnlyList<QuoteLine> Lines => _lines;

        public Currency Currency { get; }

        public Money TotalExclTax
        {
            get => CalculateTotalExclTax();
        }

        public Discount? Discount { get; }

        public SellerSnapshot? SellerSnapshot { get; private set; }

        public ClientSnapshot? ClientSnapshot { get; private set; }

        #region VAT (ADR 0020) — TODO Marine
        /// <summary>
        /// One subtotal per VAT rate: the taxable amount (after the quote discount, shared pro rata between the rates)
        /// and its VAT. The taxable amounts always add up to <see cref="TotalExclTax"/>.
        /// </summary>
        public IReadOnlyList<VatSubtotal> VatBreakdown
        {
            get => throw new NotImplementedException();
        }

        public Money TotalVat
        {
            get => throw new NotImplementedException();
        }

        public Money TotalInclTax
        {
            get => throw new NotImplementedException();
        }
        #endregion

        #region Lifecycle (ADR 0018, 0019)

        public void MarkAsSent(QuoteNumber number, DateOnly issueDate, DateOnly validUntil, SellerSnapshot seller, ClientSnapshot client)
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

            Number = number;
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

        private void EnsureStatus(QuoteStatus expected)
        {
            if (Status != expected)
                throw new InvalidOperationException($"This action requires a {expected} quote; this one is {Status}.");
        }
        #endregion

        #region Lines (draft only)
        public void AddLine(QuoteLine line)
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
            QuoteLine line = FindLine(lineId);
            EnsureSameCurrency(unitPrice.Currency, nameof(unitPrice));
            line.Update(description, quantity, unitPrice, discount, vatRate);
        }

        private QuoteLine FindLine(Guid lineId) => _lines.FirstOrDefault(l => l.Id == lineId) ?? throw new ArgumentException("The line is not in this quote.", nameof(lineId));

        #endregion

        private static Guid ValidateId(Guid id, string paramName)
        {
            if (id == Guid.Empty)
                throw new ArgumentException($"The {paramName} is empty.", paramName);
            return id;
        }

        private static Currency ValidateCurrency(Currency currency)
        {
            if (!Enum.IsDefined(currency))
                throw new ArgumentException("The currency is not valid.", nameof(currency));
            return currency;
        }

        private void EnsureSameCurrency(Currency currency, string paramName)
        {
            if (currency != Currency)
                throw new ArgumentException("A quote holds a single currency.", paramName);
        }

        private Money CalculateTotalExclTax()
        {
            Money totalExclTax = Money.Zero(Currency);

            foreach (QuoteLine line in Lines)
            {
                totalExclTax += line.TotalPrice;
            }

            if (Discount != null)
                totalExclTax = Discount.Apply(totalExclTax);

            return totalExclTax;
        }
    }
}
