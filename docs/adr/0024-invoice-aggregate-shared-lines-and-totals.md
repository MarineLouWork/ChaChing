# 0024 — `Invoice` is its own aggregate; lines and totals are shared types

**Status:** Accepted

## Context

An invoice needs what a quote already has: lines with their validation, and the computation of totals and VAT breakdown. But the two must stay independent: a freelancer can invoice without ever quoting, and an invoice is an autonomous legal document with constraints a quote does not have. Using the quote's code from the quote's folder would make the invoice depend on the quote; duplicating it would let a fixed bug survive on one side.

## Decision

- **`Invoice` is its own aggregate** in `Billing/Invoices/`. It never references the `Quote` type. `Invoice.CreateDraft(sellerId, clientId, currency, discount?, quoteId?)` has the same shape as `Quote.CreateDraft`.
- **`BillingLine`** (`Billing/`) is the line of both documents. Each document owns **its own list** of lines; only the type is shared.
- **`BillingTotals`** (`Billing/`) is a value object built by `BillingTotals.Compute(lines, discount, currency)`. It holds `TotalNet`, `TotalWithoutVat`, `VatBreakdown`, `TotalVatAmount`, `TotalWithVat`, immutable. Each document exposes a single `Totals`, computed on read.
- **`QuoteId` on the invoice is a `Guid?`:** `null` means "invoiced directly", for traceability only. `Guid.Empty` is refused: an absent quote is said with `null`.
- A draft invoice has no number (ADR 0013).
- Turning a quote into an invoice is a business operation handled outside both aggregates (ADR 0032).

## Consequences

- One place for the pro rata, the cent on the last rate and the zero-total guard; `Quote` and `Invoice` evolve separately.
- `BillingTotals` is testable alone, without a document.
- **Price paid:**
  - the two classes repeat their structure (seller, client, currency, lines, `AddLine`, the single-currency rule); the technical checks are shared (ADR 0027), the rest is duplicated on purpose;
  - `quote.Totals.TotalWithVat` is one step longer to write;
  - the day an invoice line needs a field a quote line does not have, the two line types must be split;
  - nothing in the domain checks that `QuoteId` points to an existing, accepted quote: that is the use case's job.
- Alternatives set aside:
  - **a base class `BillingDocument`**: removes the repetition but welds two life cycles together (statuses, due date, credit note); composition over inheritance. To reconsider when the credit note, a third document, arrives;
  - **an injected `ITotalsCalculator`**: takes the VAT rule out of the domain and leaves anaemic documents;
  - **a static domain service**: works, but a computation that takes values and returns a value is a value object;
  - **`Invoice.FromQuote(quote)`**: convenient, but `Invoices/` would depend on `Quotes/`.
