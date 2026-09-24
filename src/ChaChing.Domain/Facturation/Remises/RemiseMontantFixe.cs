namespace ChaChing.Facturation.Remises
{
    public class RemiseMontantFixe : Remise
    {
        private readonly Money _montant;

        public RemiseMontantFixe(Money montant)
        {
            ValiderMontant(montant);
            _montant = montant;
        }

        private static void ValiderMontant(Money montant)
        {
            if (montant.Montant < 0m)
                throw new ArgumentException("Le montant de remise n'est pas correct", nameof(montant));
        }

        protected override Money Calculer(Money moneyInitial) => moneyInitial.Soustraire(_montant);
    }

}
