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

        // Later:

        // QuoteNumber? Number, DateOnly? IssueDate, DateOnly? ValidUntil
        // SellerSnapshot? SellerSnapshot, ClientSnapshot? ClientSnapshot
        // Send(number, issueDate, validUntil, seller, client)
        // Money TotalVat, TotalInclTax
        // Rehydrate(...) only if persistence needs it (ADR 0016)

        public void AddLine(QuoteLine line)
        {
            if (line == null)
                ArgumentNullException.ThrowIfNull(line);
            if (line.Id == Guid.Empty)
                throw new ArgumentException($"The line is empty.", nameof(line));
            if (line.TotalPrice.Currency != Currency)
                throw new ArgumentException($"Two different currencies can't be added to the same quote", nameof(line));
            _lines.Add(line);
        }

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
