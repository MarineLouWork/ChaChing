namespace ChaChing.Domain.Billing.Taxes
{
    /// <summary>
    /// The VAT rate of a line (ADR 0014). Either a percentage, or <see cref="Exempt"/> (EN 16931 category "E", 0 %).
    /// Built only through <see cref="Of"/> and the static values: no invalid rate can exist.
    /// </summary>
    public sealed record VatRate
    {
        private VatRate(decimal percent, bool isExempt)
        {
            Percent = percent;
            IsExempt = isExempt;
        }

        public decimal Percent { get; }

        public bool IsExempt { get; }

        public static VatRate Exempt { get; } = new(0m, true);

        public static VatRate Standard { get; } = Of(20m);

        public static VatRate Of(decimal percent)
        {
            if (percent <= 0m || percent >= 100m)
                throw new ArgumentOutOfRangeException(nameof(percent), "A VAT rate is above 0 and below 100. Use VatRate.Exempt for no VAT.");
            return new VatRate(percent, false);
        }

        public Money VatOn(Money taxableAmount) => taxableAmount * (Percent / 100m);

        public override string ToString() => IsExempt ? "Exempt" : $"{Percent} %";
    }
}
