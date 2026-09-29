# 0023 — A zero total keeps its VAT breakdown

**Status:** Accepted — 29/09/2026 · Replaces the "not computed when `TotalNet` is zero" point of ADR 0022

## Context

The pro rata of ADR 0020 divides by `TotalNet`. A quote can have lines and still a `TotalNet` of zero: a line at 0 €, or a line with a 100 % discount (a "free" line). The division then threw a `DivideByZeroException` on every read of `VatBreakdown`, `TotalVatAmount` and `TotalWithVat`.

The first fix returned an empty breakdown. But the lines still carry a VAT rate, and EN 16931 expects one VAT breakdown per rate present on the lines, even at zero.

## Decision

- Only an **empty quote** returns an empty breakdown.
- When `TotalNet` is zero, the **ratio is 1**: there is no discount to share, and every group comes out with a taxable amount of 0 and a VAT of 0.
- The division is avoided by a guard on the ratio, not caught: a zero total is an expected case, not an exception.

## Consequences

- A quote with a free line keeps one `VatSubtotal` per rate, ready for a Factur-X export; a test pins it (`DiscountOnZeroTotal_NoVat`).
- **Price paid:** the value 1 is arbitrary (any value gives 0 × ratio = 0); it is chosen because it reads "nothing to share", and it relies on a comment to be understood.
- Alternatives set aside:
  - **return an empty breakdown** (first fix): simple, but a line at 20 % would have no 20 % subtotal, a gap that would only show at invoice export;
  - **catch the `DivideByZeroException`**: hides a predictable case behind an exception;
  - **forbid lines at zero**: a free line is a real commercial gesture.
