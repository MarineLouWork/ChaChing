namespace ChaChing.Domain.Billing
{
    public record Money(decimal Amount, Currency Currency)
    {
        public decimal Amount { get; } = ValidateAmount(Amount);

        public Currency Currency { get; } = ValidateCurrency(Currency);

        public static Money Zero(Currency currency) => new Money(0m, currency);

        static decimal ValidateAmount(decimal amount) => Math.Round(amount, 2, MidpointRounding.ToEven);

        static Currency ValidateCurrency(Currency currency)
        {
            if (!Enum.IsDefined(currency))
                throw new ArgumentException("The currency is not valid.", nameof(currency));
            return currency;
        }

        private void EnsureSameCurrency(Currency currency)
        {
            if (this.Currency != currency)
                throw new ArgumentException("The currencies must be the same.", nameof(currency));
        }

        public static Money operator +(Money a, Money b) => a.Add(b);

        public Money Add(Money money)
        {
            EnsureSameCurrency(money.Currency);
            return new Money(this.Amount + money.Amount, this.Currency);
        }

        public static Money operator -(Money a, Money b) => a.Subtract(b);

        public Money Subtract(Money money)
        {
            EnsureSameCurrency(money.Currency);
            return new Money(this.Amount - money.Amount, this.Currency);
        }

        public static Money operator *(Money a, decimal b) => a.Multiply(b);

        public Money Multiply(decimal factor) => new Money(this.Amount * factor, this.Currency);
    }
}
