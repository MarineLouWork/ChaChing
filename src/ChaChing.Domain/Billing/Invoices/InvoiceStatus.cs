namespace ChaChing.Domain.Billing.Invoices
{
    public enum InvoiceStatus
    {
        Draft = 0,

        Sent = 1,

        Declined = 2,

        Paid = 3,

        Closed = 4,

        Late = 5,

        Conflicted = 6
    }
}
