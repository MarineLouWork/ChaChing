namespace ChaChing.Domain.Billing
{
    /// <summary>The series a number belongs to. Its name is the prefix of the displayed reference. Starts at 1: a forgotten value is never a valid one.</summary>
    public enum TypeDocument
    {
        QUO = 1, // QUOTE
        INV = 2, // INVOICE
        CRE = 3 // CREDIT NOTE
    }
}
