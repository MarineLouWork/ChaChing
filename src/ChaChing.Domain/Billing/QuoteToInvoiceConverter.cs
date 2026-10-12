using ChaChing.Domain.Billing.Invoices;
using ChaChing.Domain.Billing.Quotes;

namespace ChaChing.Domain.Billing
{
    public class QuoteToInvoiceConverter
    {
        public static Invoice Convert(Quote quote)
        {
            if (quote.Status != QuoteStatus.Accepted)
                throw new InvalidOperationException($"This action requires a {QuoteStatus.Accepted} quote; this one is {quote.Status}.");

            Invoice invoice = Invoice.CreateDraft(quote.SellerId,
                                                    quote.ClientId,
                                                    quote.Currency,
                                                    quote.Discount,
                                                    quote.Id);

            foreach (BillingLine line in quote.Lines)
            {
                BillingLine newLine = new(line.Description, line.Quantity, line.UnitPrice, line.Discount, line.VatRate);
                invoice.AddLine(newLine);
            }

            return invoice;
        }
    }
}
