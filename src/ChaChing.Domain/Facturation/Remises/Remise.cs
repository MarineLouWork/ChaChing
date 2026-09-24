namespace ChaChing.Facturation.Remises
{
    public abstract class Remise
    {
        public Money Appliquer(Money moneyInitial)
        {
            Money solde = Calculer(moneyInitial);
            return Plafonner(solde);
        }

        protected abstract Money Calculer(Money moneyInitial);

        private static Money Plafonner(Money money) => money.Montant < 0 ? new Money(0m, money.Devise) : money;
    }
}