namespace ChaChing.Domain.Billing.Taxes
{
    /// <summary>One line of the VAT breakdown of a document (EN 16931 BG-23): per rate, the taxable amount and the VAT.</summary>
    public sealed record VatSubtotal(VatRate Rate, Money TaxableAmount, Money VatAmount);
}
