# 0020 — VAT breakdown per rate, the document discount shared pro rata

**Status:** Accepted

## Context

EN 16931 computes VAT **per rate**, on the sum of the line amounts of that rate (VAT breakdown, BG-23), not line by line. A document can also carry a discount on its total (ADR 0007), and VAT is due on the amount **after** that discount, so with several rates the discount has to be split between them.

## Decision

- **`VatBreakdown`:** one `VatSubtotal(Rate, TaxableAmount, VatAmount)` per rate present on the document, exempt lines included (VAT 0).
- **VAT is computed and rounded once per rate**, on its taxable amount (`VatRate.VatOn`), not per line.
- **The document discount is shared pro rata:** each rate keeps its share of the lines (base of the rate ÷ `TotalNet`) of `TotalWithoutVat`. The **last** rate takes the remainder, so the taxable amounts always add up exactly to `TotalWithoutVat`.
- **A zero total keeps its breakdown.** `TotalNet` can be zero with lines present (a €0 line, or a 100 % discount). The ratio is then 1 ("nothing to share") and every rate comes out with a taxable amount and a VAT of 0, one `VatSubtotal` per rate. Only an empty document has an empty breakdown. The division is avoided by a guard, not caught.
- `TotalVatAmount` is the sum of the VAT of each rate; `TotalWithVat = TotalWithoutVat + TotalVatAmount`. Everything is computed on read, never stored (`BillingTotals`, ADR 0024).

## Consequences

- The breakdown maps directly to a Factur-X export, and no cent is lost or created by rounding.
- A free line keeps its rate in the breakdown, so there is no gap at export time. A test pins it.
- **Price paid:**
  - the cent of rounding lands on one rate chosen by order, not by a business rule; the rounding mode is `ToEven` (ADR 0003) where French practice often rounds half up, to settle with an accountant;
  - the ratio value 1 for a zero total is arbitrary (any value gives 0 × ratio = 0) and relies on a comment to be understood.
- Alternatives set aside:
  - **VAT rounded per line, then summed**: simpler, but can differ by a few cents from the per-rate computation the standard expects;
  - **a VAT rate of its own on the discount** (EN 16931 allows it for document allowances): exact, but the user would have to choose a rate for a discount;
  - **forbidding a document discount when rates are mixed**: a real limitation for the user;
  - **an empty breakdown for a zero total**, or **catching `DivideByZeroException`**: hides a predictable case, and leaves a gap at export;
  - **forbidding lines at zero**: a free line is a real commercial gesture.
