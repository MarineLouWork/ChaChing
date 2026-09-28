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

        public Currency Currency { get; }

        public QuoteStatus Status { get; private set; }

        private readonly List<QuoteLine> _lines = new();
        public IReadOnlyList<QuoteLine> Lines => _lines;

        public Money TotalExclTax
        {
            get => CalculateTotalExclTax();
        }

        public Discount? Discount { get; }

        public QuoteNumber? Number { get; private set; }

        public DateOnly? IssueDate { get; private set; }

        public DateOnly? ValidUntil { get; private set; }

        // Later:
        // SellerSnapshot? SellerSnapshot, ClientSnapshot? ClientSnapshot (taken by Send)
        // Money TotalVat, TotalInclTax
        // Rehydrate(...) only if persistence needs it (ADR 0016)

        #region Lifecycle (ADR 0018) — TODO Marine
        /// <summary>Draft → Sent. At least one line; validUntil not before issueDate; a quote keeps its number forever.</summary>
        public void Send(QuoteNumber number, DateOnly issueDate, DateOnly validUntil)
        {
            throw new NotImplementedException();
        }

        /// <summary>Sent → Accepted.</summary>
        public void Accept()
        {
            throw new NotImplementedException();
        }

        /// <summary>Sent → Refused.</summary>
        public void Refuse()
        {
            throw new NotImplementedException();
        }

        /// <summary>Refused → Draft, to rework it. Keeps its number.</summary>
        public void ReturnToDraft()
        {
            throw new NotImplementedException();
        }
        #endregion

        public void AddLine(QuoteLine line)
        {
            ArgumentNullException.ThrowIfNull(line);
            EnsureSameCurrency(line.UnitPrice.Currency, nameof(line));

            _lines.Add(line);
        }

        public void RemoveLine(Guid lineId) => _lines.Remove(FindLine(lineId));

        public void UpdateLine(Guid lineId, string description, decimal quantity, Money unitPrice, Discount? discount = null)
        {
            QuoteLine line = FindLine(lineId);
            EnsureSameCurrency(unitPrice.Currency, nameof(unitPrice));
            line.Update(description, quantity, unitPrice, discount);
        }

        private QuoteLine FindLine(Guid lineId) => _lines.FirstOrDefault(l => l.Id == lineId) ?? throw new ArgumentException("The line is not in this quote.", nameof(lineId));

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
