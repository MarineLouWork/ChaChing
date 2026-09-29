# 0022 — `TotalNet`: the sum of the lines, before the quote discount

**Status:** Accepted — 29/09/2026 · Completes ADR 0020 and 0021

## Context

Sharing the quote discount between the VAT rates (ADR 0020) needs two totals: the sum of the lines **before** the quote discount, and `TotalWithoutVat`, **after** it. Only the second one had a name, and it said nothing about the discount: while coding the breakdown, Marine applied the discount a second time to `TotalWithoutVat`, which was already discounted. EN 16931 names the first total *Sum of Invoice line net amount* (BT-106).

## Decision

- The quote exposes **`TotalNet`**: the sum of the line totals, before the quote discount. It is public: it is also the subtotal printed above the discount line.
- `TotalWithoutVat` = `TotalNet` with the quote discount applied.
- The pro rata of ADR 0020 reads `TotalWithoutVat / TotalNet` (e.g. 270 / 300 = 0.9), and is not computed when `TotalNet` is zero.
- An XML doc on both properties says "before" and "after" the quote discount.

## Consequences

- Each step of the computation has its own name (`docs/domain-model.md` §4), and the ratio can be read in the code.
- **Price paid:** "net" does not say *before which discount*. In everyday French, the *net à payer* is the final amount, so a reader may take `TotalNet` for the discounted total; only the XML doc tells the difference. The name is also a shortening of the EN 16931 one (ADR 0015/0021), to be mapped in the export adapter.
- Alternatives set aside:
  - **`LinesTotal`**: says "sum of the lines" with no ambiguity, but Marine prefers `TotalNet`, which matches the other `Total…` names;
  - **`SumOfLineNetAmounts`**, the exact EN 16931 name: unambiguous, but long;
  - **suffixes on every total** (`…Discounted`): explicit, but breaks the EN 16931 names and gives names like `TotalWithVatDiscounted`.
