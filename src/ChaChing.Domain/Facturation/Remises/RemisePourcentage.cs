namespace ChaChing.Facturation.Remises
{
    public class RemisePourcentage : Remise
    {
        private readonly decimal _taux;
        public RemisePourcentage(decimal taux)
        {
            ValiderTaux(taux);
            _taux = taux;
        }

        private static void ValiderTaux(decimal taux)
        {
            if (taux <= 0m || taux > 100m)
                throw new ArgumentException("Le taux de remise n'est pas correct", nameof(taux));
        }

        protected override Money Calculer(Money moneyInitial)
        {
            return moneyInitial.Multiplier(1 - _taux / 100);
        }
    }
}