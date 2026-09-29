using ChaChing.Domain.Common;

namespace ChaChing.Domain.Billing
{
    public class BillingLine
    {
        /// <param name="vatRate">Null means <see cref="VatRate.Standard"/> (20 %).</param>
        public BillingLine(string description, decimal quantity, Money unitPrice, Discount? discount = null, VatRate? vatRate = null)
        {
            Description = ValidateDescription(description);
            Quantity = ValidateQuantity(quantity);
            UnitPrice = ValidateUnitPrice(unitPrice);
            Discount = discount;
            VatRate = vatRate ?? VatRate.Standard;
            Id = Guid.NewGuid();
        }

        /// <param name="vatRate">Null means <see cref="VatRate.Standard"/> (20 %), as in the constructor.</param>
        internal void Update(string description, decimal quantity, Money unitPrice, Discount? discount = null, VatRate? vatRate = null)
        {
            string validDescription = ValidateDescription(description);
            decimal validQuantity = ValidateQuantity(quantity);
            Money validUnitPrice = ValidateUnitPrice(unitPrice);

            Description = validDescription;
            Quantity = validQuantity;
            UnitPrice = validUnitPrice;
            Discount = discount;
            VatRate = vatRate ?? VatRate.Standard;
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

        public Discount? Discount { get; private set; }

        public VatRate VatRate { get; private set; }

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
