# 0020 — VAT breakdown per rate, the quote discount shared pro rata

**Status:** Accepted — 28/09/2026 · Details ADR 0014

## Context

ADR 0014 puts a VAT rate on each line and leaves the rounding open. EN 16931 computes VAT **per rate**, on the sum of the line amounts of that rate (VAT breakdown, BG-23), not line by line.

A quote can also carry a discount on its total (ADR 0010). VAT is due on the amount **after** discount, so with several rates the discount has to be split between them.

## Decision

- **`VatBreakdown`:** one `VatSubtotal(Rate, TaxableAmount, VatAmount)` per rate present on the quote, exempt lines included (VAT 0).
- **The VAT is computed and rounded once per rate**, on its taxable amount (`VatRate.VatOn`), not line by line.
- **The discount on the total is shared pro rata:** each rate keeps its share of the lines (base of the rate ÷ sum of the lines) of the discounted total. The **last** rate takes the remainder, so the taxable amounts always add up exactly to `TotalExclTax`.
- `TotalVat` = sum of the VAT of each rate; `TotalInclTax` = `TotalExclTax` + `TotalVat`. Everything is computed on read, never stored.

## Consequences

- The breakdown maps directly to a Factur-X export, and no cent is lost or created by rounding.
- **Price paid:** the cent of rounding lands on one rate chosen by order, not by a business rule; the rounding mode is still `ToEven` (ADR 0006), where French practice often rounds half up — to revisit with an accountant.
- Alternatives set aside:
  - **VAT rounded per line, then summed**: simpler, but can differ by a few cents from the per-rate computation the standard expects;
  - **the discount given a VAT rate of its own** (as EN 16931 allows for document allowances): exact, but the user would have to choose a rate for a discount;
  - **forbidding a discount on the total when rates are mixed**: simple, but a real limitation for the user.
