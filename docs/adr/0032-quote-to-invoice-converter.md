# 0032 — A domain converter turns an accepted quote into a draft invoice

**Status:** Accepted

## Context

Invoicing a quote carries business rules: which quotes may be invoiced and what the invoice takes over. Those belong in the domain, not in the orchestration. Yet `Quote` and `Invoice` must not know each other (ADR 0024).

Facts in the code that shape the decision:

- a quote's lines can only change in `Draft`; an `Accepted` quote can no longer be modified;
- `BillingLine` is mutable and carries its own `Id`: the same instance in a quote and in an invoice would put one line in two aggregates;
- an invoice gets its number, dates and snapshots at issue, not at creation;
- a `Draft` invoice cannot be cancelled.

## Decision

- **A stateless class in the Domain, `QuoteToInvoiceConverter`**, holds the conversion rules. It knows both types; `Quote` and `Invoice` still know nothing of each other. It reads and writes nothing: `Convert(Quote)` returns a new draft invoice.
- **Rule in the converter:** only an `Accepted` quote can be converted, otherwise `InvalidOperationException`.
- **The draft takes over** the seller id, client id, currency, discount, and the quote's `Id` as `QuoteId`. **Lines are copied** with the `BillingLine` constructor (new `Id`), never shared.
- **No number, no dates, no snapshots at conversion:** they come at issue, as for any invoice.
- **"A quote is invoiced at most once" is checked by the use case**, before calling the converter, through the invoice port (ADR 0034). A quote may be invoiced again only if every invoice already linked to it is `Cancelled`.

## Consequences

- The conversion is tested without a database or a repository.
- **Price paid:**
  - the double-billing rule lives in the Application layer: it is tested with the use case and a fake repository, not in the domain tests;
  - checking then inserting does not stop two simultaneous requests: a unique index in the database (one non-cancelled invoice per `QuoteId`) must back the check;
  - a `Draft` invoice cannot be cancelled, so it blocks a new conversion until it is issued: a "discard a draft invoice" use case will be needed;
  - one more class in the domain, which knows two aggregates.
- Alternatives set aside:
  - **the converter receives the quote's invoices** and applies the double-billing rule itself: the rule would be testable in the domain, but objects are moved around just to read a status;
  - **the whole conversion in the use case**: no extra class, but business rules leave the domain;
  - **`Invoice.CreateFrom(Quote)`**: `Invoice` would reference `Quote`;
  - **`Quote.ConvertToInvoice()`**: `Quote` would reference `Invoice` and could no longer be tested alone;
  - **a `Converted` status on `Quote`**: not needed, and it would forbid a new conversion after a cancellation.
