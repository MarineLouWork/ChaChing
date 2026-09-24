namespace ChaChing.Domain.Billing.Discounts
{
    public abstract class Discount
    {
        public Money Apply(Money initialMoney)
        {
            Money balance = Calculate(initialMoney);
            return FloorAtZero(balance);
        }

        protected abstract Money Calculate(Money initialMoney);

        private static Money FloorAtZero(Money money) => money.Amount < 0 ? new Money(0m, money.Currency) : money;
    }
}
