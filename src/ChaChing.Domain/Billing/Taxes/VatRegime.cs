namespace ChaChing.Domain.Billing.Taxes
{
    public enum VatRegime
    {
        /// <summary>Charges VAT on its sales.</summary>
        Subject = 1,

        /// <summary>Franchise en base: no VAT, "TVA non applicable, art. 293 B du CGI".</summary>
        Exempt = 2
    }
}
