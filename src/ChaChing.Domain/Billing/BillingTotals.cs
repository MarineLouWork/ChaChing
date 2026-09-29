using ChaChing.Domain.Common;

namespace ChaChing.Domain.Billing
{
    /// <summary>The totals of a billing document (quote, invoice), computed from its lines and its discount on the total.</summary>
    public sealed class BillingTotals
    {
        #region Constructor, Creator
        private BillingTotals(Money totalNet, Money totalWithoutVat, IReadOnlyList<VatSubtotal> vatBreakdown)
        {
            TotalNet = totalNet;
            TotalWithoutVat = totalWithoutVat;
            VatBreakdown = vatBreakdown;
            TotalVatAmount = CalculateTotalVatAmount(vatBreakdown, totalNet.Currency);
            TotalWithVat = TotalWithoutVat + TotalVatAmount;
        }

        public static BillingTotals Compute(IReadOnlyList<BillingLine> lines, Discount? discount, Currency currency)
        {
            ArgumentNullException.ThrowIfNull(lines);

            Money totalNet = CalculateTotalNet(lines, currency);
            Money totalWithoutVat = discount?.Apply(totalNet) ?? totalNet;
            IReadOnlyList<VatSubtotal> vatBreakdown = CalculateVatBreakdown(lines, totalNet, totalWithoutVat, currency);

            return new BillingTotals(totalNet, totalWithoutVat, vatBreakdown);
        }
        #endregion

        #region Fields
        /// <summary>The sum of the line totals, BEFORE the discount on the total (EN 16931 BT-106).</summary>
        public Money TotalNet { get; }

        /// <summary>After the discount on the total (EN 16931 BT-109): what the VAT is computed on. See <see cref="TotalNet"/> for before.</summary>
        public Money TotalWithoutVat { get; }

        public IReadOnlyList<VatSubtotal> VatBreakdown { get; }

        public Money TotalVatAmount { get; }

        public Money TotalWithVat { get; }
        #endregion

        #region Calculates
        private static Money CalculateTotalNet(IReadOnlyList<BillingLine> lines, Currency currency)
        {
            Money totalNet = Money.Zero(currency);

            foreach (BillingLine line in lines)
                totalNet += line.TotalPrice;

            return totalNet;
        }

        private static Money CalculateTotalVatAmount(IReadOnlyList<VatSubtotal> vatBreakdown, Currency currency)
        {
            Money totalVatAmount = Money.Zero(currency);

            foreach (VatSubtotal detail in vatBreakdown)
                totalVatAmount += detail.VatAmount;

            return totalVatAmount;
        }

        private static IReadOnlyList<VatSubtotal> CalculateVatBreakdown(IReadOnlyList<BillingLine> lines, Money totalNet, Money totalWithoutVat, Currency currency)
        {
            List<VatSubtotal> list = new List<VatSubtotal>();

            // If there is no line, nothing to compute
            if (lines.Count == 0)
                return list;

            // If there is a discount on the total, we compute the ratio discount per product before applying VAT
            decimal ratio = totalNet.Amount == 0 ? 1m : totalWithoutVat.Amount / totalNet.Amount;

            // We group lines by VAT rate
            var linesByVatRate = lines.GroupBy(l => l.VatRate).ToList();
            Money alreadyDistributed = Money.Zero(currency);

            for (int i = 0; i < linesByVatRate.Count; i++)
            {
                var group = linesByVatRate[i];

                // We sum up the total prices of every lines
                Money groupTotal = Money.Zero(currency);
                foreach (BillingLine line in group)
                    groupTotal += line.TotalPrice;

                // The last group takes the rest, so the bases always add up to TotalWithoutVat
                bool isLast = i == linesByVatRate.Count - 1;
                Money taxableAmount = isLast ? totalWithoutVat - alreadyDistributed : groupTotal * ratio;

                alreadyDistributed += taxableAmount;

                Money vatAmount = group.Key.VatOn(taxableAmount);
                list.Add(new VatSubtotal(group.Key, taxableAmount, vatAmount));
            }

            return list;
        }
        #endregion
    }
}
