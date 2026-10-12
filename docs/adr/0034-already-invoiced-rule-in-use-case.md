# 0034 — "Already invoiced" is decided by the use case; the port returns statuses

**Status:** Accepted

## Context

`ConvertToInvoice` must refuse to invoice the same quote twice (ADR 0032). To do so it must know which invoices already exist for a quote, through `IInvoiceRepository`. Two shapes of that question were possible.

In the domain, a draft invoice cannot be cancelled, so a draft blocks any new conversion until it is issued. A cancelled invoice does not block a new one.

## Decision

- **The port returns the statuses** of the invoices already linked to a quote (`QuoteId`), not a yes/no.
- **The use case decides** what counts as "already invoiced": any status except `Cancelled`.
- The repository only answers a factual question ("which invoices come from this quote?") and holds no business rule.

## Consequences

- If the rule changes (say, a draft stops blocking), only the use case and its tests change: no adapter, no SQL.
- The fake repository in tests is trivial: a list of statuses.
- **Price paid:** the port returns more data than a boolean and the check runs in memory, which costs nothing with a handful of invoices per quote. It does **not** replace the database guarantee: a unique index still protects against two concurrent conversions, since the use case alone cannot (read-then-write race).
- Alternative set aside: **`bool ExistsActiveInvoiceFor(quoteId)`**, simpler for the caller, but "active" would be defined inside the adapter (`WHERE Status <> Cancelled`), hiding a business rule in infrastructure and duplicating it in every adapter.
