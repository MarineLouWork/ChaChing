# 0019 — Snapshots are built from the aggregates and checked by the document

**Status:** Accepted

## Context

ADR 0012 freezes the parties' details when a document is sent. A document only references them by id (ADR 0010), so it cannot load them. Someone has to build the snapshots, and nothing must let a document freeze the details of **another** seller or client.

## Decision

- **`SellerSnapshot` and `ClientSnapshot` have a private constructor** and are built only by `SellerSnapshot.From(seller)` and `ClientSnapshot.From(client)`. A snapshot cannot hold details that never existed.
- **Each snapshot carries the id of its source** (`SellerId`, `ClientId`).
- **The use case builds them** from the loaded aggregates and passes them to `MarkAsSent` / `MarkedAsIssued`.
- **The document checks them:** a snapshot whose id is not the document's `SellerId` / `ClientId` is rejected (`ArgumentException`), and an exempt seller cannot send a line carrying VAT (ADR 0014). All checks run before any change.
- `Address` is a value object shared by `Seller`, `Client` and the snapshots, in `Common/`.

## Consequences

- A document can only freeze its own parties.
- The documents still depend on no other aggregate: they receive plain value objects.
- **Price paid:** `Parties` snapshots depend on `Seller` and `Client` (for `From`); the id they carry is not printed, only used for the check.
- Alternatives set aside:
  - **`MarkAsSent(Seller, Client)` with the aggregates themselves**: the document would depend on two other aggregates;
  - **public snapshot constructors**: simpler to build in tests, but any caller could invent details;
  - **no id check**: a faulty use case could silently freeze another freelancer's details on a document.
