# 0024 — Lines and totals shared by quote and invoice

**Status:** Accepted — 29/09/2026 · Prepares `Invoice`

## Context

`Invoice` needs what `Quote` already has: lines (description, quantity, unit price, discount, VAT rate, with their validation) and the computation of the totals and of the VAT breakdown (ADR 0020, 0022, 0023). Both lived in `Billing/Quotes/`. Using them from there would make the invoice depend on the quote, which Marine rules out: an invoice is an autonomous legal document. Duplicating them would let a fixed bug survive on one side.

## Decision

- **`BillingLine`** (`Billing/`): the former `QuoteLine`, renamed and moved, nothing else changed. Each document owns **its own** list of lines; only the type is shared.
- **`BillingTotals`** (`Billing/`): a **value object** built by `BillingTotals.Compute(lines, discount, currency)`. It holds `TotalNet`, `TotalWithoutVat`, `VatBreakdown`, `TotalVatAmount`, `TotalWithVat`, computed once, immutable.
- `Quote` exposes a single `Totals` property, computed on read, never stored. `Invoice` will do the same.
- **`Invoice` never references the `Quote` type.** Invoicing a quote is an application use case: it reads the quote and hands its lines to the invoice. The invoice keeps an optional `Guid? QuoteId`, for traceability only (like `ClientId`).

## Consequences

- One place for the pro rata, the cent on the last rate and the zero total guard; the 179 existing tests went through the refactor unchanged, except `quote.X` → `quote.Totals.X`.
- `BillingTotals` can be tested on its own, without a document.
- **Price paid:** `quote.Totals.TotalWithVat` is one step longer to write; the day an invoice line needs a field a quote line does not have, the two line types will have to be split.
- Alternatives set aside:
  - **an injected application service** (`ITotalsCalculator`): takes the VAT rule out of the domain and leaves anemic documents; an entity does not get services injected;
  - **a base class `BillingDocument`**: welds two documents whose life cycles differ (credit note, due date, statuses); composition over inheritance;
  - **a static domain service**: works, but a computation that takes values and returns a value is a value object;
  - **the name `DocumentTotals`**: vaguer, and `BillingTotals` pairs with `BillingLine`.
