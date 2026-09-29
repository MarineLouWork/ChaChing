using ChaChing.Domain.Common;

namespace ChaChing.Domain.Billing
{
    public class FixedAmountDiscount : Discount
    {
        private readonly Money _amount;

        public FixedAmountDiscount(Money amount)
        {
            ValidateAmount(amount);
            _amount = amount;
        }

        private static void ValidateAmount(Money amount)
        {
            if (amount.Amount < 0m)
                throw new ArgumentException("The discount amount is not valid.", nameof(amount));
        }

        protected override Money Calculate(Money initialMoney) => initialMoney.Subtract(_amount);
    }

}
