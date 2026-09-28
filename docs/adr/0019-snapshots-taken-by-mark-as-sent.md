# 0019 — Snapshots are built from the aggregates and checked by the quote

**Status:** Accepted — 28/09/2026 · Details ADR 0012

## Context

ADR 0012 freezes the seller's and the client's details when a quote is sent. `Quote` only references them by id (ADR 0010, 0011): it cannot load them itself. Someone has to build the snapshots, and nothing must let a quote freeze the details of **another** seller or client.

## Decision

- **`SellerSnapshot` and `ClientSnapshot` are value objects with a private constructor**, built only by `SellerSnapshot.From(seller)` and `ClientSnapshot.From(client)`. A snapshot cannot hold details that never existed.
- **Each snapshot carries the id of its source** (`SellerId`, `ClientId`).
- **The use case builds them** from the loaded aggregates and passes them to `MarkAsSent(number, issueDate, validUntil, seller, client)`.
- **The quote checks them:** a snapshot whose id is not the quote's `SellerId` / `ClientId` is rejected (`ArgumentException`). An **exempt** seller cannot send a line that carries VAT (`InvalidOperationException`: the lines are in the wrong state). All checks run before any change.
- `Address` is a value object shared by `Seller`, `Client` and the snapshots, in `Domain.Common`.

## Consequences

- A quote can only freeze its own parties, and the 293 B mention can never sit next to a VAT amount.
- `Quote` still depends on no other aggregate: it receives plain value objects.
- **Price paid:** `Billing.Parties` depends on `Sellers` and `Clients` (for `From`). The snapshots carry an id that is not printed on the document, only used for the check.
- Alternatives set aside:
  - **`MarkAsSent(Seller, Client)`** with the aggregates themselves: the quote would depend on two other aggregates;
  - **public snapshot constructors**: simpler to build in tests, but any caller could invent details;
  - **no id check**: a wrong use case could freeze another freelancer's details on a quote, silently.
