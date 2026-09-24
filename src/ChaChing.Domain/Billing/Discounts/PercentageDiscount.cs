namespace ChaChing.Domain.Billing.Discounts
{
    public class PercentageDiscount : Discount
    {
        private readonly decimal _rate;
        public PercentageDiscount(decimal rate)
        {
            ValidateRate(rate);
            _rate = rate;
        }

        private static void ValidateRate(decimal rate)
        {
            if (rate <= 0m || rate > 100m)
                throw new ArgumentException("The discount rate is not valid.", nameof(rate));
        }

        protected override Money Calculate(Money initialMoney)
        {
            return initialMoney.Multiply(1 - _rate / 100);
        }
    }
}
