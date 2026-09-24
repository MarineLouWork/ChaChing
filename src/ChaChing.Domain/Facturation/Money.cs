namespace ChaChing.Facturation
{
    public record Money(decimal Montant, Devise Devise)
    {
        public decimal Montant { get; } = ValiderMontant(Montant);

        public Devise Devise { get; } = ValiderDevise(Devise);

        public static Money Zero(Devise devise) => new Money(0m, devise);

        static decimal ValiderMontant(decimal montant) => Math.Round(montant, 2, MidpointRounding.ToEven);

        static Devise ValiderDevise(Devise devise)
        {
            if (!Enum.IsDefined(devise))
                throw new ArgumentException("La devise n'est pas correcte", nameof(devise));
            return devise;
        }

        private void VerifierMemeDevise(Devise devise)
        {
            if (this.Devise != devise)
                throw new ArgumentException("Les devises doivent être identiques", nameof(devise));
        }

        public static Money operator +(Money a, Money b) => a.Ajouter(b);

        public Money Ajouter(Money money)
        {
            VerifierMemeDevise(money.Devise);
            return new Money(this.Montant + money.Montant, this.Devise);
        }

        public static Money operator -(Money a, Money b) => a.Soustraire(b);

        public Money Soustraire(Money money)
        {
            VerifierMemeDevise(money.Devise);
            return new Money(this.Montant - money.Montant, this.Devise);
        }

        public static Money operator *(Money a, decimal b) => a.Multiplier(b);

        public Money Multiplier(decimal facteur) => new Money(this.Montant * facteur, this.Devise);
    }
}
