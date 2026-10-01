namespace ChaChing.Domain.Billing
{
    /// <summary>The series a number belongs to. Its name is the prefix of the displayed reference. Starts at 1: a forgotten value is never a valid one.</summary>
    public enum TypeDocument
    {
        DEV = 1, // DEVIS
        FAC = 2 // FACTURE
    }
}
