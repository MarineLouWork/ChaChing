# 0026 — `Invoice.CreateDraft`: its own aggregate, an optional link to a quote

**Status:** Accepted — 01/10/2026

## Context

The invoice is the second billing document. It shares its lines and totals with the quote (ADR 0024), but the two must stay independent: a freelancer can invoice without ever quoting, and an invoice has legal constraints a quote does not have.

## Decision

- **`Invoice` is its own aggregate**, in `Billing/Invoices/`. It never references the `Quote` type.
- **`Invoice.CreateDraft(sellerId, clientId, currency, discount?, quoteId?)`**, same shape as `Quote.CreateDraft` (ADR 0016).
- **`QuoteId` is a `Guid?`**: `null` means "invoiced directly". `Guid.Empty` is refused: an absent quote is said with `null`, never with an empty identifier.
- **A draft has no number**, like a quote (ADR 0013).
- Turning a quote into an invoice is a **use case of the application layer**: it reads the quote and calls `CreateDraft` with its identifier.

## Consequences

- `Quote` and `Invoice` can evolve separately; deleting one folder does not break the other.
- **Price paid:** the two classes repeat their structure (seller, client, currency, lines, `AddLine`, the single-currency rule). The technical checks are shared through `Guard` (ADR 0027); the rest is duplicated on purpose.
- Nothing in the domain checks that `QuoteId` points to an existing, accepted quote: that is the use case's job (as for `ClientId`, ADR 0011).
- Alternatives set aside:
  - **a common base class `BillingDocument`**: removes the repetition, but ties the two lifecycles together by inheritance; to reconsider when the credit note arrives (third document);
  - **`Invoice.FromQuote(quote)`**: convenient, but makes `Invoices/` depend on `Quotes/` (forbidden by ADR 0025).
