namespace ChaChing.Domain.Billing.Discounts
{
    public abstract class Discount
    {
        /// <summary>
        /// Apply the discount to the money
        /// </summary>
        /// <param name="initialMoney">The initial money discounted</param>
        /// <returns>The money left after the discount</returns>
        /// <exception cref="ArgumentException">
        /// Throws if <paramref name="initialMoney"/> holds a different currency than the discount's
        /// </exception>
        public Money Apply(Money initialMoney)
        {
            Money balance = Calculate(initialMoney);
            return FloorAtZero(balance);
        }

        protected abstract Money Calculate(Money initialMoney);

        private static Money FloorAtZero(Money money) => money.Amount < 0 ? new Money(0m, money.Currency) : money;
    }
}
