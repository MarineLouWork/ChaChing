using ChaChing.Domain.Billing.Discounts;

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

        /// <summary>A quote is always born as an empty draft.</summary>
        public static Quote CreateDraft(Guid sellerId, Guid clientId, Currency currency, Discount? discount = null) => new Quote(sellerId, clientId, currency, discount);

        public Guid Id { get; }

        public Guid SellerId { get; }

        public Guid ClientId { get; }

        public QuoteNumber? Number { get; private set; }

        public QuoteStatus Status { get; private set; }

        public DateOnly? IssueDate { get; private set; }

        public DateOnly? ValidUntil { get; private set; }

        public Currency Currency { get; }

        private readonly List<QuoteLine> _lines = new();
        public IReadOnlyList<QuoteLine> Lines => _lines;

        public Money TotalExclTax
        {
            get => CalculateTotalExclTax();
        }

        public Discount? Discount { get; }

        // Later:
        // SellerSnapshot? SellerSnapshot, ClientSnapshot? ClientSnapshot (taken by Send)
        // Money TotalVat, TotalInclTax
        // Rehydrate(...) only if persistence needs it (ADR 0016)

        #region Lifecycle (ADR 0018)
        // The domain records what happened; it does not send any email itself: hence "MarkAs…".

        /// <summary>Draft → Sent. At least one line; validUntil not before issueDate; a quote keeps its number forever.</summary>
        public void MarkAsSent(QuoteNumber number, DateOnly issueDate, DateOnly validUntil)
        {
            ArgumentNullException.ThrowIfNull(number);
            EnsureStatus(QuoteStatus.Draft);

            if (_lines.Count == 0)
                throw new InvalidOperationException("A quote without lines cannot be sent.");

            if (validUntil < issueDate)
                throw new ArgumentException("The validity date cannot be before the issue date.", nameof(validUntil));

            if (Number != null && Number != number)
                throw new InvalidOperationException($"This quote already has the number {Number}.");

            Number = number;
            IssueDate = issueDate;
            ValidUntil = validUntil;
            Status = QuoteStatus.Sent;
        }

        /// <summary>Sent → Accepted.</summary>
        public void MarkAsAccepted()
        {
            EnsureStatus(QuoteStatus.Sent);
            Status = QuoteStatus.Accepted;
        }

        /// <summary>Sent → Declined.</summary>
        public void MarkAsDeclined()
        {
            EnsureStatus(QuoteStatus.Sent);
            Status = QuoteStatus.Declined;
        }

        /// <summary>Declined → Draft, to rework it. Keeps its number.</summary>
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

        public void UpdateLine(Guid lineId, string description, decimal quantity, Money unitPrice, Discount? discount = null)
        {
            EnsureStatus(QuoteStatus.Draft);
            QuoteLine line = FindLine(lineId);
            EnsureSameCurrency(unitPrice.Currency, nameof(unitPrice));
            line.Update(description, quantity, unitPrice, discount);
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
