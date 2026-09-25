using ChaChing.Domain.Billing.Discounts;

namespace ChaChing.Domain.Billing.Quotes
{
    public class QuoteLine
    {
        public QuoteLine(string description, decimal quantity, Money unitPrice, Discount? discount = null)
        {
            Description = ValidateDescription(description);
            Quantity = ValidateQuantity(quantity);
            UnitPrice = ValidateUnitPrice(unitPrice);
            Discount = discount;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }

        public string Description { get; private set; }

        public decimal Quantity { get; private set; }

        public Money UnitPrice { get; private set; }

        public Money TotalPrice
        {
            get
            {
                return CalculateTotalPrice();
            }
        }

        public Discount? Discount { get; }

        private static string ValidateDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("The description is required.", nameof(description));
            return description.Trim();
        }

        private static decimal ValidateQuantity(decimal quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("The quantity can't be so low.", nameof(quantity));
            return quantity;
        }

        private static Money ValidateUnitPrice(Money unitPrice)
        {
            if (unitPrice.Amount < 0)
                throw new ArgumentException("The unitPrice can't be so low", nameof(unitPrice));
            return unitPrice;
        }

        private Money CalculateTotalPrice() => this.Discount?.Apply(this.UnitPrice.Multiply(this.Quantity)) ?? this.UnitPrice.Multiply(this.Quantity);

    }
}
